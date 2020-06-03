using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyWebSite.Interfaces;
using MyWebSite.Models.Entities;

namespace MyWebSite.TagHelpers
{
    public class SelectSourceSecuritiesTagHelper: TagHelper
    {
        public List<SourceSecurity> Elements { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            string listContent = "<option disabled>Выберите источник данных</option>";

            if (Elements != null)
                foreach (var item in Elements)
                {
                    if (item.Name.Contains("finam", StringComparison.CurrentCultureIgnoreCase))
                    {
                        listContent += "<option selected " + "value=\"" + item.IdString + "\">" + item.Name + "</option>";
                        continue;
                    }
                    listContent += "<option " + "value=\"" + item.IdString + "\">" + item.Name + "</option>";
                }
                    


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
