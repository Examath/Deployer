using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deployer
{
    public class BomHeaderMap
    {
        public string FilenameCol { get; private set; } = "L";

        public string PartNumberCol { get; private set; } = "B";

        public string QuantityCol { get; private set; } = "F";

        public string DescriptionCol { get; private set; } = "H";

        public string RevisionCol { get; private set; } = "I";

        public string MassCol { get; private set; } = "J";

        public string MaterialCol { get; private set; } = "K";

        public BomHeaderMap() { }
    }
}
