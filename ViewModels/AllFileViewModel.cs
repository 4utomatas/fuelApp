using System.Collections.Generic;
using kuras.Models;

namespace kuras.ViewModels
{
    // This is used for Selecting an input from three different tables
    // For formating a REPORT
    public class AllFileViewModel
    {
        public List<GasStationFile> GasStationFileAll { get; set; }
        public List<KilometersFile> KilometersFileAll { get; set; }
    }
}
