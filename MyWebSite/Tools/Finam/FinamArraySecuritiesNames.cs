using MyWebSite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;

namespace MyWebSite.Tools
{
    public static class FinamArraySecuritiesNames
    {
        public async static Task<Dictionary<int, BrokerRepositorySecurity>> GetSecurityComboList(string reg_str)
        {
            Dictionary<int, BrokerRepositorySecurity> dict = new Dictionary<int, BrokerRepositorySecurity>();
            Regex regex = new Regex(@$"^{reg_str}(\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            int max_combo_list_count = 30;
            int added_item_to_list = 0;

            var response = await GetPage("https://www.finam.ru/cache/icharts/icharts.js");
            List<BrokerRepositorySecurity> _finamSecurities = new List<BrokerRepositorySecurity>();
            string[] arraySets = response.Split('=');
            string[] arrayIds = arraySets[1].Split('[')[1].Split(']')[0].Split(',');

            string names = arraySets[2].Split('[')[1].Split(']')[0];

            List<string> arrayNames = new List<string>();

            string name = "";

            for (int i = 1; i < names.Length; i++)
            {
                if ((names[i] == '\'' && i + 1 == names.Length)
                    ||
                    (names[i] == '\'' && names[i + 1] == ',' && names[i + 2] == '\''))
                {
                    arrayNames.Add(name);
                    name = "";
                    i += 2;
                }
                else
                {
                    name += names[i];
                }
            }
            string[] arrayCodes = arraySets[3].Split('[')[1].Split(']')[0].Split(',');
            string[] arrayMarkets = arraySets[4].Split('[')[1].Split(']')[0].Split(',');
            string[] arrayDecp = arraySets[5].Split('{')[1].Split('}')[0].Split(',');
            string[] arrayFormatStrs = arraySets[6].Split('[')[1].Split(']')[0].Split(',');
            string[] arrayEmitentChild = arraySets[7].Split('[')[1].Split(']')[0].Split(',');
            string[] arrayEmitentUrls = arraySets[8].Split('{')[1].Split('}')[0].Split(',');

            _finamSecurities = new List<BrokerRepositorySecurity>();

            for (int i = 0; i < arrayIds.Length; i++)
            {
                var tt = arrayEmitentUrls[i].Split(':')[1];

                BrokerRepositorySecurity sec2 = new BrokerRepositorySecurity()
                {
                    Code = arrayCodes[i],
                    Decp = arrayDecp[i].Split(':')[1],
                    EmitentChild = arrayEmitentChild[i],
                    IdString = arrayIds[i],
                    Name = arrayNames[i],
                    Url = arrayEmitentUrls[i].Split(':')[1],
                    MarketId = arrayMarkets[i],
                    Market = FinamSecurityDecoder.FinamCodeToString(arrayMarkets[i])
                };

                MatchCollection matches = regex.Matches(sec2.Name);

                if (matches.Count > 0)
                {
                    dict.Add(added_item_to_list, sec2);
                    added_item_to_list++;
                    max_combo_list_count--;
                }

                if (max_combo_list_count <= 0)
                {
                    dict.Clear();
                    break;
                }
                   
            }

            return dict;
        }

        public async static Task<string> GetPage(string uri)
        {
            if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);


            var response = await request.GetResponseAsync();

            //HttpWebResponse response = (HttpWebResponse)request .GetResponse();

            string resultPage = "";

            using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default, true))
            {
                resultPage = sr.ReadToEnd();
                sr.Close();
            }
            return resultPage;
        }
    }
}
