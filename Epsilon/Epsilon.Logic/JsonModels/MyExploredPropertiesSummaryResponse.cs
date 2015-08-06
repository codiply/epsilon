using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.JsonModels
{
    public class MyExploredPropertiesSummaryResponse
    {
        public IList<ExploredPropertyInfo> items { get; set; }

        public bool moreItemsExist { get; set; }
    }
}
