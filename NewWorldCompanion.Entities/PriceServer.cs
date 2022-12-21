using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWorldCompanion.Entities
{
    public class PriceServer
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; } = 1;
        public DateTime Updated { get; set; } = DateTime.MinValue;
    }
}
