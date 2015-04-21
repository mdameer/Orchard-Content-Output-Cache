using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Mdameer.ContentOutputCache
{
    [OrchardSuppressDependency("Orchard.OutputCache.AdminMenu")]
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder
                .Add(T("Settings"), menu => menu
                    .Add(T("Cache Statistics"), "10.0", subMenu => subMenu.Action("Index", "Statistics", new { area = "Mdameer.ContentOutputCache" }).Permission(StandardPermissions.SiteOwner)
                    ));
        }
    }
}
