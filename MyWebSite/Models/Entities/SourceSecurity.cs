using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyWebSite.Interfaces;

namespace MyWebSite.Models.Entities
{
    /// <summary>
    /// класс представляет источник получения инструмента
    /// </summary>
    public class SourceSecurity : ISelectable<SourceSecurity>
    {
        public int Id { get; set; }
        public string IdString { get => Id.ToString(); set{} }
        public string Name { get; set; }
        public List<BrokerRepositorySecurity> BrokerRepositorySecurities { get; set; } = new List<BrokerRepositorySecurity>();
    }
}
