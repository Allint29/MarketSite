using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyWebSite.Models;

namespace MyWebSite.TagHelpers
{
    public class ListBrokerRepositorySecurities : TagHelper
    {
        public Dictionary<int, BrokerRepositorySecurity> Elements { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "ul";
            string listContent = "";



            if (Elements != null)
            {
                // элемент перед тегом
                output.PreElement.SetHtmlContent($"<p>Найдено {Elements.Count()} элементов</p>");
                //output.Attributes.RemoveAll("class");
                output.Attributes.SetAttribute("class", $"myclass");
                //output.Attributes.SetAttribute("style", $"font-family:{font};font-size:18px;");
                foreach (var item in Elements)
                    listContent += "<li>" + item.Value.Name + "</li>";

                // элемент после тега
                output.PostElement.SetHtmlContent($"<p>----------------------------</p>");
            }


            output.Content.SetHtmlContent(listContent);
        }
    }
}
