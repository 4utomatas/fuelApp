using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.Models
{
    public class Kilometers
    {
        public int Id { get; set; }
        public int Car { get; set; }
        public string CarNumber { get; set; }
        public float StartOdometer { get; set; }
        public float EndOdometer { get; set; }
        public float KmGps { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

    }
}
