using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.Models
{
    public class Axapta
    {
        public int Id { get; set; }
        public string EmpIdAutoTable { get; set; }
        public string EmpIdEmplTable { get; set; }
        public string EmpName { get; set; }
        public string CardNumber { get; set; }
        public string CardNumberD { get; set; }
        public string NumberPlate { get; set; }
        public string MakeModel { get; set; }
        public int Type { get; set; }    
        public string EmpGroup { get; set; }
        public string CityCode { get; set; }
        public string Occupation { get; set; }
        public int Blocked { get; set; }
    }
}
