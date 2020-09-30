using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.ViewModels
{
    public class KilometersFileViewModel
    {
        public List<KilometersGpsFileViewModel> Gps { get; set; }
        public List<KilometersOdoFileViewModel> Odo { get; set; }
    }
}
