using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.Models
{
    public class KilometersFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public bool Gps { get; set; }
        public bool Uploaded { get; set; }
    }
}
