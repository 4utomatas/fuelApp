using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.Models
{
    public class RemainingFuel
    {
        public int Id { get; set; }
        public int Car { get; set; }
        public string NumberPlate { get; set; }
        public float StartFuel { get; set; }
        public float EndFuel { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
