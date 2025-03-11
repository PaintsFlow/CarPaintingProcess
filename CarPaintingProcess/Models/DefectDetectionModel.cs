using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace CarPaintingProcess.Models
{
    public class DefectDetectionModel
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public string colorName { get; set; }
        public int colorH { get; set; }
        public int colorS { get; set; }
        public int colorV { get; set; }

    }
}
