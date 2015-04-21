using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;

namespace Mdameer.ContentOutputCache.Settings
{
    public class OutputCachePartSettingsEvents : ContentDefinitionEditorEventsBase {

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "OutputCachePart")
                yield break;

            var settings = definition.Settings.GetModel<OutputCachePartSettings>();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "OutputCachePart")
                yield break;

            var settings = new OutputCachePartSettings
            {
            };

            if (updateModel.TryUpdateModel(settings, "OutputCachePartSettings", null, null))
            {
                builder.WithSetting("OutputCachePartSettings.EnableCache", settings.EnableCache.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("OutputCachePartSettings.CacheDuration", settings.CacheDuration.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("OutputCachePartSettings.CacheGraceTime", settings.CacheGraceTime.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("OutputCachePartSettings.AllowOverride", settings.AllowOverride.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("OutputCachePartSettings.VaryByQueryStringParameters", settings.VaryByQueryStringParameters);
                builder.WithSetting("OutputCachePartSettings.VaryByRequestHeaders", settings.VaryByRequestHeaders);
                builder.WithSetting("OutputCachePartSettings.VaryByCulture", settings.VaryByCulture.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("OutputCachePartSettings.VaryByAuthenticationState", settings.VaryByAuthenticationState.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("OutputCachePartSettings.VaryByUser", settings.VaryByUser.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("OutputCachePartSettings.VaryByUrl", settings.VaryByUrl.ToString(CultureInfo.InvariantCulture));
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
