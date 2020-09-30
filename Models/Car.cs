using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace kuras.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; } 
        public string NumberPlate { get; set; } //Numberplate
        // Update Car Make and Model
        public void UpdateValues( Car other )
        {
            if (this.Make != other.Make)
                this.Make = other.Make;
            if (this.Model != other.Model)
                this.Model = other.Model;
        }
        public bool Compare( Car other )
        {
            if (this.Model == other.Model &&
                this.Make == other.Make)
                return true;
            else return false;

        }
        public bool CompareNP(Car other)
        {
            if (this.NumberPlate == other.NumberPlate)
                return true;
            else return false;

        }
    }
}
