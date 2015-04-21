using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Data;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace Mdameer.ContentOutputCache
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("OutputCachePart",
                cfg => cfg.Attachable()
                );

            return 1;
        }
    }
}
