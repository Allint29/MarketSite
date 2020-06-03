using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebSite.Interfaces
{
    /// <summary>
    /// Интерфейс нужен для использования его в формировании выпадающих списков или просто списков
    /// </summary>
    public interface ISelectable<out T>
    {
        public string IdString { get; set; }
        public string Name { get; set; }

    }
}
