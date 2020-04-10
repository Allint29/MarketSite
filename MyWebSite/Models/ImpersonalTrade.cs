using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebSite.Models
{
    /// <summary>
    /// Класс - минимальная единица, характеризующая один трейд
    /// </summary>
    public class ImpersonalTrade
    {
        public int Id { get; set; }
        /// <summary>
        /// Тикер инструмента в формате SPFB.Si-12.18
        /// </summary>
        public string Ticker { get; set; }  

        /// <summary>
        /// Тикер инструмента в формате SIM9
        /// </summary>
        public string Ticker_Short { get; set; }
        
        /// <summary>
        /// Дата трейда в формате 20190325
        /// </summary>
        public int Date { get; set; }       
        
        /// <summary>
        /// Время трейда в формате 105959
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Время и дата трейда, расчитывается из Date and Time
        /// </summary>
        public DateTime Date_Time { get; set; }

        /// <summary>
        /// Цена трейда
        /// </summary>
        public decimal Last { get; set; }

        /// <summary>
        /// Объем трейда
        /// </summary>
        public int Vol { get; set; }

        /// <summary>
        /// Направление трейда. Может быть S или B
        /// </summary>
        public string Oper { get; set; }

        /// <summary>
        /// Id трейда на бирже
        /// </summary>
        public long Exchange_Id { get; set; }

        /// <summary>
        /// Данные проверены
        /// </summary>
        public bool Verificated { get; set; }

    }
}
