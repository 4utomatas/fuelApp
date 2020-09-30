using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.ViewModels
{
    public class ReportCreateViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Date { get; set; }
        public int GasStation { get; set; }
        public int Gps { get; set; }
        public int Odo { get; set; }
        
    }
}
