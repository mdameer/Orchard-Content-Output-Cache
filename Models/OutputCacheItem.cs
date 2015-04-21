using Orchard.UI.Resources;
using System;
using System.Collections.Generic;

namespace Mdameer.ContentOutputCache.Models
{
    [Serializable]
    public class OutputCacheItem
    {
        public OutputCacheItem()
        {
            Resources = new List<OutputCacheItemResource>();
        }

        public int Id { get; set; }
        public string Output { get; set; }
        public IList<OutputCacheItemResource> Resources { get; set; }
    }

    public class OutputCacheItemResource
    {
        public string Name { get; set; }
        public string BasePath { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string UrlDebug { get; set; }
        public ResourceLocation Location { get; set; }
    }
}