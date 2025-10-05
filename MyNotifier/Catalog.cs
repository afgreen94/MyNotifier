using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts.Base;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier
{
    public class Catalog
    {
        public IDictionary<Guid, Definition> InterestIdDefinitionMap { get; set; }
        public IDictionary<Guid, IUpdaterDefinition> UpdaterIdDefinitionMap { get; set; }
        public IDictionary<Guid, Definition> NotificationTypeMap { get; set; }
    }
}
