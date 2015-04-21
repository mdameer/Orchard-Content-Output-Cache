using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.UI.Zones;
using Orchard.Environment.Extensions;
using Orchard.ContentManagement;
using Orchard;
using Mdameer.ContentOutputCache.Models;
using Mdameer.ContentOutputCache.Settings;
using System.Text;
using Orchard.Themes;
using Orchard.Environment.Configuration;
using System.Globalization;
using Orchard.Utility.Extensions;
using Orchard.Caching;
using Orchard.Services;
using Orchard.UI.Resources;
using System.Linq;
using System.Collections.Concurrent;
using Orchard.OutputCache.Services;
using Orchard.OutputCache.Models;
using System.Threading;
using Orchard.UI.Admin;

namespace Mdameer.ContentOutputCache.ContentManagement
{
    [OrchardSuppressDependency("Orchard.ContentManagement.DefaultContentDisplay")]
    public class MdameerContentDisplay : DefaultContentDisplay, IContentDisplay
    {
        private static string _refreshKey = "__r";
        private static long _epoch = new DateTime(2014, DateTimeKind.Utc).Ticks;
        private static readonly ConcurrentDictionary<string, object> _cacheKeyLocks = new ConcurrentDictionary<string, object>();

        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IShapeFactory _shapeFactory;
        private readonly Lazy<IShapeTableLocator> _shapeTableLocator;

        private readonly RequestContext _requestContext;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IOrchardServices _orchardServices;
        private readonly IShapeDisplay _shapeDisplay;

        private readonly IThemeManager _themeManager;
        private readonly ShellSettings _shellSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;
        private readonly ITagCache _tagCache;
        private readonly IClock _clock;
        private readonly ISignals _signals;
        private readonly IResourceManager _resourceManager;
        private readonly IJsonConverter _jsonConverter;

        public MdameerContentDisplay(
            Lazy<IEnumerable<IContentHandler>> handlers,
            IShapeFactory shapeFactory,
            Lazy<IShapeTableLocator> shapeTableLocator,
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider,
            IWorkContextAccessor workContextAccessor,
            IOrchardServices orchardServices,
            IShapeDisplay shapeDisplay,
            IThemeManager themeManager,
            ShellSettings shellSettings,
            ICacheManager cacheManager,
            IOutputCacheStorageProvider cacheStorageProvider,
            ITagCache tagCache,
            IClock clock,
            ISignals signals,
            IResourceManager resourceManager,
            IJsonConverter jsonConverter) :
            base(handlers, shapeFactory, shapeTableLocator, requestContext, virtualPathProvider, workContextAccessor)
        {
            _handlers = handlers;
            _shapeFactory = shapeFactory;
            _shapeTableLocator = shapeTableLocator;
            _requestContext = requestContext;
            _virtualPathProvider = virtualPathProvider;
            _workContextAccessor = workContextAccessor;
            _orchardServices = orchardServices;
            _shapeDisplay = shapeDisplay;
            _themeManager = themeManager;
            _shellSettings = shellSettings;
            _cacheManager = cacheManager;
            _cacheStorageProvider = cacheStorageProvider;
            _tagCache = tagCache;
            _clock = clock;
            _signals = signals;
            _resourceManager = resourceManager;
            _jsonConverter = jsonConverter;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        private DateTime _now;
        private string _cacheKey;
        private string _invariantCacheKey;
        private OutputCacheSettings _cacheSettings;

        public dynamic BuildDisplay(IContent content, string displayType, string groupId)
        {
            dynamic result = null;
            try
            {
                var useCache = UseCache(content);
                result = useCache
                        ? BuildCachedShape(content, displayType, groupId)
                        : BuildShape(content, displayType, groupId);
            }
            finally
            {
                if (_cacheKey != null)
                {
                    object cacheKeyLock;
                    if (_cacheKeyLocks.TryGetValue(_cacheKey, out cacheKeyLock) && Monitor.IsEntered(cacheKeyLock))
                    {
                        Monitor.Exit(cacheKeyLock);
                    }
                }
            }
            return result;
        }

        private bool UseCache(IContent content)
        {
            var cachePart = content.As<OutputCachePart>();
            if (cachePart == null) return false;

            if (AdminFilter.IsApplied(new RequestContext(_workContextAccessor.GetContext().HttpContext, new RouteData()))) return false;

            var settings = cachePart.TypePartDefinition.Settings.GetModel<OutputCachePartSettings>();

            if (settings == null) return false;

            if (!settings.EnableCache)
            {
                if (!settings.AllowOverride || !cachePart.EnableOverride || !cachePart.EnableCache) return false;
            }
            else
            {
                if (settings.AllowOverride && cachePart.EnableOverride && !cachePart.EnableCache) return false;
            }
            return true;
        }

        private string ComputeCacheKey(IContent content, string displayType, string groupId)
        {
            var sb = new StringBuilder();
            var workContext = _workContextAccessor.GetContext();
            var theme = _themeManager.GetRequestTheme(workContext.HttpContext.Request.RequestContext).Id;

            var parameters = new Dictionary<string, object>();

            var queryString = workContext.HttpContext.Request.QueryString;
            foreach (var varyByQueryString in _cacheSettings.VaryByQueryStringParameters)
            {
                if (queryString.AllKeys.Contains(varyByQueryString))
                    parameters["QueryString:" + varyByQueryString] = queryString[varyByQueryString];
            }

            var requestHeaders = workContext.HttpContext.Request.Headers;
            foreach (var varyByRequestHeader in _cacheSettings.VaryByRequestHeaders)
            {
                if (requestHeaders.AllKeys.Contains(varyByRequestHeader))
                    parameters["HEADER:" + varyByRequestHeader] = requestHeaders[varyByRequestHeader];
            }

            if (_cacheSettings.VaryByCulture)
            {
                parameters["culture"] = workContext.CurrentCulture.ToLowerInvariant();
            }
            
            if (_cacheSettings.VaryByAuthenticationState)
            {
                parameters["auth"] = workContext.HttpContext.User.Identity.IsAuthenticated.ToString().ToLowerInvariant();
            }

            if (_cacheSettings.VaryByUser)
            {
                parameters["user"] = workContext.HttpContext.User.Identity.IsAuthenticated ?
                    workContext.HttpContext.User.Identity.Name.ToLowerInvariant() :
                    "anonymous";
            }

            if (_cacheSettings.VaryByUrl)
            {
                parameters["url"] = workContext.HttpContext.Request.Url.AbsolutePath.ToLowerInvariant();
            }

            //sb.Append("layer=").Append(content.LayerId.ToString(CultureInfo.InvariantCulture)).Append(";");
            //sb.Append("zone=").Append(content.Zone).Append(";");
            sb.Append("id=").Append(content.Id.ToString(CultureInfo.InvariantCulture)).Append(";");
            sb.Append("displaytype=").Append(displayType).Append(";");
            sb.Append("groupid=").Append(groupId).Append(";");
            sb.Append("tenant=").Append(_shellSettings.Name).Append(";");
            sb.Append("theme=").Append(theme.ToLowerInvariant()).Append(";");

            foreach (var pair in parameters)
            {
                sb.AppendFormat("{0}={1};", pair.Key.ToLowerInvariant(), Convert.ToString(pair.Value).ToLowerInvariant());
            }

            return sb.ToString();
        }

        private dynamic BuildShape(IContent content, string displayType, string groupId)
        {
            return base.BuildDisplay(content, displayType, groupId);
        }

        private dynamic BuildCachedShape(IContent content, string displayType, string groupId)
        {
            _cacheSettings = GetOutputCacheSettings(content);
            _cacheKey = ComputeCacheKey(content, displayType, groupId);
            _invariantCacheKey = string.Format("tenant={0};id={1};", _shellSettings.Name, content.ContentItem.Id);
            var cacheKeyLock = _cacheKeyLocks.GetOrAdd(_cacheKey, x => new object());
            var cacheDuration = _cacheSettings.CacheDuration;
            _now = _clock.UtcNow;
            //dynamic result = null;

            try
            {
                var cacheItem = _cacheStorageProvider.GetCacheItem(_cacheKey);
                if (cacheItem != null)
                {
                    if (_now > cacheItem.ValidUntilUtc && _now < cacheItem.CachedOnUtc.AddSeconds(cacheDuration + _cacheSettings.CacheGraceTime))
                    {
                        if (Monitor.TryEnter(cacheKeyLock))
                        {
                           return BeginRenderItem(content, displayType, groupId);
                        }
                    }

                    return ServeCachedItem(cacheItem, content, displayType, groupId);
                }

                if (Monitor.TryEnter(cacheKeyLock))
                {
                    cacheItem = _cacheStorageProvider.GetCacheItem(_cacheKey);
                    if (cacheItem != null)
                    {
                        Monitor.Exit(cacheKeyLock);
                        return ServeCachedItem(cacheItem, content, displayType, groupId);
                    }
                }

                return BeginRenderItem(content, displayType, groupId);
            }
            catch
            {
                if (Monitor.IsEntered(cacheKeyLock))
                    Monitor.Exit(cacheKeyLock);
                throw;
            }
        }

        private dynamic ServeCachedItem(CacheItem cacheItem, IContent content, string displayType, string groupId)
        {
            var outputCacheItem = _jsonConverter.Deserialize<OutputCacheItem>(cacheItem.Output);
            foreach (var resource in outputCacheItem.Resources)
            {
                if (!string.IsNullOrWhiteSpace(resource.BasePath))
                {
                    _resourceManager.Require(resource.Type, resource.Name).AtLocation(resource.Location);
                }
                else
                {
                    _resourceManager.Include(resource.Type, resource.Url, resource.UrlDebug, resource.BasePath).AtLocation(resource.Location);
                }
            }

            var actualDisplayType = string.IsNullOrWhiteSpace(displayType) ? "Detail" : displayType;
            var itemShape = _orchardServices.New.RawOutput(Content: outputCacheItem.Output);
            itemShape.ContentItem = content.ContentItem;
            itemShape.Metadata.DisplayType = actualDisplayType;
            return itemShape;
        }

        private dynamic BeginRenderItem(IContent content, string displayType, string groupId)
        {
            WorkContext workContext = _workContextAccessor.GetContext();
            var output = _shapeDisplay.Display(BuildShape(content, displayType, groupId));
            _cacheSettings = GetOutputCacheSettings(content);
            _cacheKey = ComputeCacheKey(content, displayType, groupId);
            _invariantCacheKey = string.Format("tenant={0};id={1};", _shellSettings.Name, content.ContentItem.Id);

            var cacheItem = new CacheItem()
            {
                CachedOnUtc = _now,
                ValidFor = _cacheSettings.CacheDuration,
                Output = _jsonConverter.Serialize(new OutputCacheItem
                {
                    Id = content.ContentItem.Id,
                    Output = output,
                    Resources = _resourceManager.GetRequiredResources("script")
                        .Concat(_resourceManager.GetRequiredResources("stylesheet")).Select(GetCacheItemResource).ToList()
                }),
                ContentType = content.ContentItem.ContentType,
                QueryString = workContext.HttpContext.Request.Url.Query,
                CacheKey = _cacheKey,
                InvariantCacheKey = _invariantCacheKey,
                Url = workContext.HttpContext.Request.Url.AbsolutePath,
                Tenant = _shellSettings.Name,
                StatusCode = workContext.HttpContext.Response.StatusCode,
                Tags = new[] { _invariantCacheKey, content.ContentItem.Id.ToString(CultureInfo.InvariantCulture) }
            };

            _cacheStorageProvider.Remove(_cacheKey);
            _cacheStorageProvider.Set(_cacheKey, cacheItem);

            foreach (var tag in cacheItem.Tags)
            {
                _tagCache.Tag(tag, _cacheKey);
            }

            return ServeCachedItem(cacheItem, content, displayType, groupId);
        }

        private OutputCacheItemResource GetCacheItemResource(RequireSettings settings)
        {
            var definition = _resourceManager.FindResource(settings);
            return new OutputCacheItemResource()
            {
                Location = settings.Location,
                Name = settings.Name,
                Type = settings.Type,
                Url = definition.Url,
                UrlDebug = definition.UrlDebug,
                BasePath = definition.BasePath
            };
        }

        private OutputCacheSettings GetOutputCacheSettings(IContent content)
        {
            return new OutputCacheSettings(content);
        }
    }
}
