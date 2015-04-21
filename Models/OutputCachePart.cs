using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Mdameer.ContentOutputCache.Models
{
    public class OutputCachePart : ContentPart
    {
        public int CacheDuration
        {
            get { return this.Retrieve(x => x.CacheDuration, 600); }
            set { this.Store(x => x.CacheDuration, value); }
        }

        public int CacheGraceTime
        {
            get { return this.Retrieve(x => x.CacheGraceTime, 60); }
            set { this.Store(x => x.CacheGraceTime, value); }
        }

        public bool EnableCache
        {
            get { return this.Retrieve(x => x.EnableCache, true); }
            set { this.Store(x => x.EnableCache, value); }
        }

        public bool EnableOverride
        {
            get { return this.Retrieve(x => x.EnableOverride, false); }
            set { this.Store(x => x.EnableOverride, value); }
        }

        public string VaryByQueryStringParameters
        {
            get { return this.Retrieve(x => x.VaryByQueryStringParameters, string.Empty); }
            set { this.Store(x => x.VaryByQueryStringParameters, value); }
        }

        public string VaryByRequestHeaders
        {
            get { return this.Retrieve(x => x.VaryByRequestHeaders, string.Empty); }
            set { this.Store(x => x.VaryByRequestHeaders, value); }
        }

        public bool VaryByCulture
        {
            get { return this.Retrieve(x => x.VaryByCulture, false); }
            set { this.Store(x => x.VaryByCulture, value); }
        }

        public bool VaryByAuthenticationState
        {
            get { return this.Retrieve(x => x.VaryByAuthenticationState, false); }
            set { this.Store(x => x.VaryByAuthenticationState, value); }
        }

        public bool VaryByUser
        {
            get { return this.Retrieve(x => x.VaryByUser, false); }
            set { this.Store(x => x.VaryByUser, value); }
        }

        public bool VaryByUrl
        {
            get { return this.Retrieve(x => x.VaryByUrl, false); }
            set { this.Store(x => x.VaryByUrl, value); }
        }

        public string GenericSignalName
        {
            get { return "OutputCachePartSignal"; }
        }

        public string ContentSignalName
        {
            get { return "OutputCachePartSignal-" + Id; }
        }

        public string TypeSignalName
        {
            get { return "OutputCachePartSignal-" + ContentItem.ContentType; }
        }
    }
}