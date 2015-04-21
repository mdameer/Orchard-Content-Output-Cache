using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Mdameer.ContentOutputCache.Settings;

namespace Mdameer.ContentOutputCache.Models
{
    public class OutputCacheSettings
    {
        public const string CacheKey = "GMR_ContentOutputCache_OutputCacheSettings";

        public OutputCacheSettings(IContent content)
        {
            var cachePart = content.As<OutputCachePart>();
            var settings = cachePart.TypePartDefinition.Settings.GetModel<OutputCachePartSettings>();

            if (settings.AllowOverride && cachePart.EnableOverride)
            {
                EnableCache = cachePart.EnableCache;
                    CacheDuration = cachePart.CacheDuration;
                    CacheGraceTime = cachePart.CacheGraceTime;
                    VaryByQueryStringParameters = String.IsNullOrWhiteSpace(cachePart.VaryByQueryStringParameters) ? new HashSet<string>() : new HashSet<string>(cachePart.VaryByQueryStringParameters.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray());
                    VaryByRequestHeaders = String.IsNullOrWhiteSpace(cachePart.VaryByRequestHeaders) ? new HashSet<string>() : new HashSet<string>(cachePart.VaryByRequestHeaders.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray());
                    VaryByCulture = cachePart.VaryByCulture;
                    VaryByAuthenticationState = cachePart.VaryByAuthenticationState;
                    VaryByUser = cachePart.VaryByUser;
                    VaryByUrl = cachePart.VaryByUrl;
            }
            else
            {
                EnableCache = settings.EnableCache;
                CacheDuration = settings.CacheDuration;
                CacheGraceTime = settings.CacheGraceTime;
                VaryByQueryStringParameters = String.IsNullOrWhiteSpace(settings.VaryByQueryStringParameters) ? new HashSet<string>() : new HashSet<string>(settings.VaryByQueryStringParameters.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray());
                VaryByRequestHeaders = String.IsNullOrWhiteSpace(settings.VaryByRequestHeaders) ? new HashSet<string>() : new HashSet<string>(settings.VaryByRequestHeaders.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray());
                VaryByCulture = settings.VaryByCulture;
                VaryByAuthenticationState = settings.VaryByAuthenticationState;
                VaryByUser = settings.VaryByUser;
                VaryByUrl = settings.VaryByUrl;
            }
        }

        public int CacheDuration { get; private set; }
        public int CacheGraceTime { get; private set; }
        public bool EnableCache { get; private set; }
        public ISet<string> VaryByQueryStringParameters { get; private set; }
        public ISet<string> VaryByRequestHeaders { get; private set; }
        public bool VaryByCulture { get; private set; }
        public bool VaryByAuthenticationState { get; private set; }
        public bool VaryByUser { get; private set; }
        public bool VaryByUrl { get; private set; }
    }
}