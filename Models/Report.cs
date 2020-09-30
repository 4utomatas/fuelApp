using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string NumberPlate { get; set; }
        public int Card { get; set; }
        public double Suma { get; set; }
        public double Km { get; set; }
        public double Km_gps { get; set; }
        public double L { get; set; }
        public double Start { get; set; }
        public double End { get; set; }
    }
}
