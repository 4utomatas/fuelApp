using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kuras.Models;

namespace kuras.ViewModels
{
    public class AxaptaChangesViewModel
    {
        //Fetched from kurasContext
        public Car FCar { get; set; }
        //Fetched from Axapta
        public Car ACar { get; set; }
        //Fetched from kurasContext
        public Card FCard { get; set; }
        //Fetched from Axapta
        public Card ACard { get; set; }
        //Fetched from kurasContext
        public Employee FEmp { get; set; }
        //Fetched from Axapta
        public Employee AEmp { get; set; }

        public AxaptaChangesViewModel(Car fcar, Car acar, Card fcard, Card acard)
        {
            this.FCar = fcar;
            this.ACar = acar;
            this.FCard = fcard;
            this.ACard = acard;
        }
        public AxaptaChangesViewModel(Car fcar, Car acar, Card fcard, Card acard, Employee femp, Employee aemp)
        {
            this.FCar = fcar;
            this.ACar = acar;
            this.FCard = fcard;
            this.ACard = acard;
            this.FEmp = femp;
            this.AEmp = aemp;
        }
        public AxaptaChangesViewModel() { }
    }
}
