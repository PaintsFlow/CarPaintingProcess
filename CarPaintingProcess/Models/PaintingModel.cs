using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPaintingProcess.Models
{
    public class PaintingModel
    {
        public DateTime time { get; set; }
        public double paintamount { get; set; }
        public double pressure { get; set; }

        //읽기 전용 속성
        public string TimeFormatted => time.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
