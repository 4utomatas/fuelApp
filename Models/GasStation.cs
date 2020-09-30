using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.Models
{
    public class GasStation
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public string CardNumberD { get; set; }
        public string Date { get; set; }
        public string Location { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public double ItemPrice { get; set; }
        public double ItemAmount { get; set; }
        public double ItemTotalPrice { get; set; }
    }
}
