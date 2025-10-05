using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Base
{
    public interface IDefinition
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
    }

    public class Definition : IDefinition
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
