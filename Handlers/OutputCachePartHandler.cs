using Mdameer.ContentOutputCache.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement.Shapes;
using System.Web;
using System.Linq;
using System.Collections;
using Orchard.ContentManagement;
using System.Collections.Generic;
using Orchard;
using System;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Drivers.Coordinators;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Navigation.Models;
using Orchard.UI.Admin;
using System.Web.Routing;
using Orchard.Caching;
using Orchard.Services;
using Mdameer.ContentOutputCache.Settings;
using Orchard.UI.Resources;
using System.Text.RegularExpressions;
using Orchard.OutputCache.Services;
using Orchard.Environment.Configuration;
using Orchard.Data;
using Orchard.OutputCache.Models;
using Orchard.Environment.Extensions;

namespace Mdameer.ContentOutputCache.Handlers
{
    public class OutputCachePartHandler : ContentHandler
    {
        public OutputCachePartHandler(ShellSettings shellSettings,
            IOutputCacheStorageProvider cacheStorageProvider,
            IWorkContextAccessor workContextAccessor)
        {
            OnUpdated<OutputCachePart>((context, part) =>
            {
                var key = string.Format("tenant={0};id={1};", shellSettings.Name, part.Id);

                if (cacheStorageProvider is DefaultCacheStorageProvider)
                {
                    var items = workContextAccessor.GetContext().HttpContext.Cache.AsParallel().Cast<DictionaryEntry>().Where(i => (i.Value as CacheItem).InvariantCacheKey == key).Select(i => i.Value as CacheItem);
                    foreach (var item in items)
                    {
                        cacheStorageProvider.Remove((string)item.CacheKey);
                    }
                }
            });
        }
    }

    [UseExplicit]
    [OrchardFeature("Orchard.OutputCache.Database")]
    public class DatabaseOutputCachePartHandler : ContentHandler
    {
        public DatabaseOutputCachePartHandler(ShellSettings shellSettings,
            IRepository<CacheItemRecord> _repository,
            IOutputCacheStorageProvider cacheStorageProvider,
            IWorkContextAccessor workContextAccessor)
        {
            OnUpdated<OutputCachePart>((context, part) =>
            {
                var key = string.Format("tenant={0};id={1};", shellSettings.Name, part.Id);
                lock (String.Intern(key))
                {
                    var records = _repository.Table.Where(x => x.InvariantCacheKey == key).ToList();

                    foreach (var record in records)
                    {
                        _repository.Delete(record);
                    }
                }
            });
        }
    }
}