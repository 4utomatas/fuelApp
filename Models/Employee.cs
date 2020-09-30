using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kuras.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmplId { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Group { get; set; }
        public string City { get; set; }

        // Update Employee values 
        public void UpdateValues(Employee other)
        {
            if (this.Fname != other.Fname)
                this.Fname = other.Fname;
            if (this.Lname != other.Lname)
                this.Lname = other.Lname;
            if (this.Group != other.Group)
                this.Group = other.Group;
            if (this.City != other.City)
                this.City = other.City;
        }
        public bool Compare(Employee other)
        {
            if (this.Fname == other.Fname &&
                this.Lname == other.Lname &&
                this.Group == other.Group &&
                this.City == other.City)
                return true;
            else return false;
        }
        public bool CompareEmplId(Employee other)
        {
            if (this.EmplId == other.EmplId)
                return true;
            else return false;
        }
    }
}
