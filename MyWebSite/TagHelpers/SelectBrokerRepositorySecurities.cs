using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyWebSite.Models;

namespace MyWebSite.TagHelpers
{
    public class SelectBrokerRepositorySecuritiesTagHelper : TagHelper
    {
        public Dictionary<int, BrokerRepositorySecurity> Elements { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            string listContent = "<option disabled>Выберите инструмент</option>";

            if (Elements != null)
                foreach (var item in Elements)
                    listContent += "<option " + "value=\""+ item.Value.IdString + "\">" + item.Value.Name + "</option>";

            //<select size="3" multiple name="hero[]">
            //<option disabled>Выберите героя</option>
            //<option value="Чебурашка">Чебурашка</option>
            //<option selected value="Крокодил Гена">Крокодил Гена</option>
            //<option value="Шапокляк">Шапокляк</option>
            //<option value="Крыса Лариса">Крыса Лариса</option>
            //</select>


            output.Content.SetHtmlContent(listContent);
        }
    }
}
