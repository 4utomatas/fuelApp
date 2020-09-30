using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.Models
{
    public class Card
    {
        public int Id { get; set; }
        public int Car { get; set; }
        public int Emp { get; set; }
        public string Number { get; set; }
        public string NumberD { get; set; }
        public float Limit { get; set; }

        public void UpdateValues(Card other)
        {
            if (this.Emp != 0 && this.Emp != other.Emp)
                this.Emp = other.Emp;
            if (this.Car != 0 && this.Car != other.Car)
                this.Car = other.Car;
        }
        //Checks if the card numbers are not empty and are equal
        public bool CompareNumber(Card other)
        {
            if (string.IsNullOrWhiteSpace(this.Number) &&
                string.IsNullOrWhiteSpace(other.Number) &&
                string.IsNullOrWhiteSpace(this.NumberD) &&
                string.IsNullOrWhiteSpace(other.NumberD) &&
                this.Number == other.Number &&
                this.NumberD == other.NumberD)
                return true;
            else return false;

        }
        public bool Compare(Card other)
        {
            if (this.Emp != 0 && this.Emp == other.Emp &&
                this.Car != 0 && this.Car == other.Car &&
                this.Limit == other.Limit)
                return true;
            else return false;               
        }
    }
}
