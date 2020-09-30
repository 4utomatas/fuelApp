using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kuras.Models;
namespace kuras.ViewModels
{
    public class CardFullViewModel
    {
        public List<Employee> AllEmployees { get; set; }
        public List<Card> AllCards { get; set; }
        public List<Car> AllCars { get; set; }
    }
}
