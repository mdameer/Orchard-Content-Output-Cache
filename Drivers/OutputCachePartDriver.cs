using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using Mdameer.ContentOutputCache.Models;
using Mdameer.ContentOutputCache.Settings;

namespace Mdameer.ContentOutputCache.Drivers
{
    public class OutputCachePartDriver : ContentPartDriver<OutputCachePart>
    {

        public OutputCachePartDriver()
        {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(OutputCachePart part, dynamic shapeHelper)
        {
            var settings = part.TypePartDefinition.Settings.GetModel<OutputCachePartSettings>();
            if (settings.AllowOverride)
            {
                return ContentShape("Parts_OutputCache_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts.OutputCache.Edit", Model: part, Prefix: Prefix));
            }
            else
            {
                return null;
            }
        }

        protected override DriverResult Editor(OutputCachePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}
