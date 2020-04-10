using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebSite.Models
{
    /// <summary>
    /// Класс информации об инструменте
    /// </summary>
    public class SecurityInfo
    {
        public int Id { get; set; }

        /// <summary>
        /// краткое описание SIM0
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// краткое описание по бирже Si-6.20
        /// </summary>
        public string ShortExchangeDescription { get; set; }

        public DateTime BeginTradingDate { get; set; }

        public DateTime EndTradingDate { get; set; }
    }
}
