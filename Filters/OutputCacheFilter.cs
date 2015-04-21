using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Filters;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.OutputCache.ViewModels;
using Orchard.Services;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.Utility.Extensions;
using Orchard.Environment.Extensions;
using Orchard;
using Orchard.Localization;

namespace Mdameer.ContentOutputCache.Filters
{
    [OrchardSuppressDependency("Orchard.OutputCache.Filters.OutputCacheFilter")]
    public class OutputCacheFilter : FilterProvider, IResultFilter
    {
        public OutputCacheFilter(IOrchardServices _services)
        {
            Services = _services;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.Controller is Orchard.OutputCache.Controllers.AdminController || filterContext.Controller is Orchard.OutputCache.Controllers.StatisticsController)
            {
                filterContext.Cancel = true;
                Services.Notifier.Add(Orchard.UI.Notify.NotifyType.Information, T("The orchard default Output Cache filter has been disabled, becuase the content output cache module is active."));
            }
        }
    }
}