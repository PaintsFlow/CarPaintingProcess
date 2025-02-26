using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPaintingProcess.Models
{
    internal class ElectroDepositionModel
    {
        public DateTime time {  get; set; }
        public double waterlevel { get; set; }
        public double viscosity { get; set; }
        public double ph { get; set; }
        public double current {  get; set; }
        public double voltage { get; set; }
    }
}
