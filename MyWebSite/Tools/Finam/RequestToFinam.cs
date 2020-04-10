using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebSite.Models
{
    public class RequestToFinam
    {
        public string SecurityName { get; set; }
        public bool CurrentMArket { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndTime { get; set; }
                
    }
}
