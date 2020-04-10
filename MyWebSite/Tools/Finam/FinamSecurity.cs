﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebSite.Models
{

    /// <summary>
    /// security in Finam specification
    /// контракт в спецификации финам
    /// </summary>
    public class FinamSecurity
    {
        public int Id { get; set; }
        
        private int? _idInt;
        /// <summary>
        /// Идентификатор инструмента как инт
        /// </summary>
        public int? IdInt 
        {
            get { return _idInt; }
            private set
            {
                if (Int32.TryParse(value.ToString(), out var result))
                {
                    _idInt = result;
                    return;
                }
                _idInt = null;
            }
        }

        /// <summary>
        /// unique number
        /// уникальный номер в строковом представлении
        /// </summary>
        public string IdString { get; set; }

        /// <summary>
        /// name
        /// имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// код контракта
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// name of market
        /// название рынка 
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// name of market as a number
        /// название рынка в виде цифры
        /// </summary>
        public string MarketId { get; set; }

        /// <summary>
        /// хз
        /// </summary>
        public string Decp { get; set; }

        /// <summary>
        /// хз
        /// </summary>
        public string EmitentChild { get; set; }

        /// <summary>
        /// web-site adress
        /// адрес на сайте
        /// </summary>
        public string Url { get; set; }
    }
}
