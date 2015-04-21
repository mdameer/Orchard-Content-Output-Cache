using System.ComponentModel.DataAnnotations;
namespace Mdameer.ContentOutputCache.Settings
{
    public class OutputCachePartSettings
    {

        public OutputCachePartSettings()
        {
            EnableCache = true;
            CacheDuration = 600;
            CacheGraceTime = 60;
            AllowOverride = false;
            VaryByQueryStringParameters = "";
            VaryByRequestHeaders = "";
            VaryByCulture = false;
            VaryByAuthenticationState = false;
            VaryByUser = false;
            VaryByUrl = false;
        }

        public bool EnableCache { get; set; }

        [Range(0, int.MaxValue)]
        public int CacheDuration { get; set; }

        [Range(0, int.MaxValue)]
        public int CacheGraceTime { get; set; }

        public bool AllowOverride { get; set; }

        public string VaryByQueryStringParameters { get; set; }
        public string VaryByRequestHeaders { get; set; }

        public bool VaryByCulture { get; set; }
        public bool VaryByAuthenticationState { get; set; }
        public bool VaryByUser { get; set; }
        public bool VaryByUrl { get; set; }
    }
}