using Orchard;
using Orchard.DisplayManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Mdameer.ContentOutputCache
{
    public class Shapes : IDependency
    {
        [Shape]
        public void RawOutput(dynamic Shape, TextWriter Output, string Content)
        {
            if (Content == null)
                return;

            Output.Write(Content);
        }
    }
}