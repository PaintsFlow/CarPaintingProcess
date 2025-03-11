using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPaintingProcess.Models
{
    public class ElectroDepositionModel
    {
        public DateTime time {  get; set; }
        public double waterlevel { get; set; }
        public double viscosity { get; set; }
        public double ph { get; set; }
        public double current {  get; set; }
        public double voltage { get; set; }
        public double temperature { get; set; }

        //읽기 전용 속성
        public string TimeFormatted => time.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
