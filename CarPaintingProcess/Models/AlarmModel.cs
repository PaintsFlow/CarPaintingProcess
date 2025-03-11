using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPaintingProcess.Models
{
    public class AlarmModel
    {
        public int alarmid { get; set; }
        public DateTime time { get; set; }
        public string sensor { get; set; }
        public double data { get; set; }
        public string message { get; set; }

        //읽기 전용 속성
        public string TimeFormatted => time.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
