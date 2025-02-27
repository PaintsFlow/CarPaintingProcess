using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPaintingProcess.Models
{
    public class DryModel
    {
        public DateTime time { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }

        //읽기 전용 속성
        public string TimeFormatted => time.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
}
