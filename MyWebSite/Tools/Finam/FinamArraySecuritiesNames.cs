using MyWebSite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using MyWebSite.Models.Entities;
using MyWebSite.Services;

namespace MyWebSite.Tools
{
    public static class FinamArraySecuritiesNames
    {
        /// <summary>
        /// Метод запрашивает страницу у сервера, по переданному адресу
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static async Task<string> GetPage(string uri)
        {
            if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            var ruRu = new CultureInfo("ru-RU");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            //request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            var response = await request.GetResponseAsync();

            //HttpWebResponse response = (HttpWebResponse)request .GetResponse();

            string resultPage = "";

            using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(ruRu.TextInfo.ANSICodePage), true))
            {
                resultPage = sr.ReadToEnd();
                sr.Close();
            }
            return resultPage;
        }

        /// <summary>
        ///  Метод ищет в локальной БД выбранный в комбобоксе инструмент
        /// </summary>
        /// <param name="reg_str">паттерн для поиска инструмента по первым символам - для уменьшения количества выбора</param>
        /// <param name="refresh_data_db">Обновить данные в БД</param>
        /// <returns></returns>
        public static async Task<List<BrokerRepositorySecurity>> GetSecurityComboList(MyDbContext db, string reg_str, int? sourceSecurityId)
        {
            //Словарь - как результат поиска нужного инструмента
            List<BrokerRepositorySecurity> result = new List<BrokerRepositorySecurity>();
            //паттерн для начального поиска по базе нужного инструмента
            Regex regex = new Regex(@$"^{reg_str}(\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            //лимит по найденным инструментам - если больше нужно уточнять поиск
            int max_combo_list_count = 30;
            int added_item_to_list = 0;

            if (string.IsNullOrEmpty(reg_str) || sourceSecurityId == null)
                return result;

            var securities_in_db = await db.GetListBrokerRepositorySecurity(name: reg_str);

            var list_securities_in_db = securities_in_db.Where(s =>
            {
                MatchCollection matches = regex.Matches(s.Name);

                if (matches.Count > 0 && s.SourceSecurityId == sourceSecurityId)
                    return true;

                return false;
            }).ToList();

            if (list_securities_in_db.Count() > 30)
                return result;

            return list_securities_in_db;
        }


        /// <summary>
        /// Метод получает все записи об инструментах из репозитория Финам
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="source_id"></param>
        /// <returns></returns>
        public static async Task<List<BrokerRepositorySecurity>> InspectSecuritiesInDb(MyDbContext db, int source_id)
        {
            //Словарь - как результат поиска нужного инструмента
            List<BrokerRepositorySecurity> result = new List<BrokerRepositorySecurity>();

            //если не определен ресурс скачивания или он не финам
            if (source_id != 2)
                return result;

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

                bool parseInId = Int32.TryParse(arrayIds[i], out int secIntId);

                BrokerRepositorySecurity sec2 = new BrokerRepositorySecurity()
                {
                    Code = arrayCodes[i],
                    Decp = arrayDecp[i].Split(':')[1],
                    EmitentChild = arrayEmitentChild[i],
                    SourceSecurityId = source_id,
                    IdString = arrayIds[i],
                    IdInt = parseInId ? secIntId : -1,
                    Name = arrayNames[i],
                    Url = arrayEmitentUrls[i].Split(':')[1],
                    MarketId = arrayMarkets[i],
                    Market = FinamSecurityDecoder.FinamCodeToString(arrayMarkets[i])
                };

                result.Add(sec2);
            }

            return result;
        }


        /// <summary>
        /// take trade for period
        /// взять трейды за период
        /// </summary>
        /// <param name="timeStart"></param>
        /// <param name="timeEnd"></param>
        /// <returns></returns>
        public static string GetTrades(BrokerRepositorySecurity security, DateTime nowDate)
        {
            /*
                market, em, code – об этих параметрах, упоминал ранее, при обращении к функции их значения будут приниматься из файла.
                df, mf, yf, from, dt, mt, yt, to – это параметры времени.
                p — период котировок (тики, 1 мин., 5 мин., 10 мин., 15 мин., 30 мин., 1 час, 1 день, 1 неделя, 1 месяц)
                e – расширение получаемого файла; возможны варианты — .txt либо .csv
                dtf — формат даты (1 — ггггммдд, 2 — ггммдд, 3 — ддммгг, 4 — дд/мм/гг, 5 — мм/дд/гг)
                tmf — формат времени (1 — ччммсс, 2 — ччмм, 3 — чч: мм: сс, 4 — чч: мм)
                MSOR — выдавать время (0 — начала свечи, 1 — окончания свечи)
                mstimever — выдавать время (НЕ московское — mstimever=0; московское — mstime='on', mstimever='1')
                sep — параметр разделитель полей (1 — запятая (,), 2 — точка (.), 3 — точка с запятой (;), 4 — табуляция (»), 5 — пробел ( ))
                sep2 — параметр разделитель разрядов (1 — нет, 2 — точка (.), 3 — запятая (,), 4 — пробел ( ), 5 — кавычка ('))
                datf — Перечень получаемых данных (#1 — TICKER, PER, DATE, TIME, OPEN, HIGH, LOW, CLOSE, VOL; #2 — TICKER, PER, DATE, TIME, OPEN, HIGH, LOW, CLOSE; #3 — TICKER, PER, DATE, TIME, CLOSE, VOL; #4 — TICKER, PER, DATE, TIME, CLOSE; #5 — DATE, TIME, OPEN, HIGH, LOW, CLOSE, VOL; #6 — DATE, TIME, LAST, VOL, ID, OPER).
                at — добавлять заголовок в файл (0 — нет, 1 — да)
             */
             
            if (security == null || nowDate == null )
                return "Ошибка определения инструмента";

            //формат даты год - последние 2 цифры (1998 -> 98)
            string year_short = nowDate.ToString("yyyy").Substring(2);

            //формат даты год - последние 2 цифры (1998 -> 98)
            string year_full = nowDate.ToString("yyyy");

            //месяц начиная с 1, то есть если май, то 5, если декабрь то 12
            string month_original = nowDate.Month.ToString();

            //месяц начиная с 0, то есть если май, то 4, если декабрь то 11
            string month_minus_one = (nowDate.Month - 1).ToString();

            //день месяца
            string day = nowDate.Day.ToString();

            StringBuilder sb = new StringBuilder();

            //дата запроса котировок 27.05.2020
            //http://export.finam.ru/export9.out
            sb.Append("http://export.finam.ru/export9.out");
            //?market=17
            sb.Append($"?market={security.MarketId}");
            //&em=496222
            sb.Append($"&em={security.IdString}");
            //&token=03AGdBq24TWv3xEpwJgauNVQpUInHX2dKUht0HMCagbdRO7wRLJHYCknzeqw8jxMHzz6yFXqFPEY-vSC7L1tB8W5BYAXP4wPwHbE3pEMe7G7yNRh1ySC6WojEoIRSuzNHpaU90zdzrbwQaB5w0LWpxOwgXaNvxv14Mvbd260ucowqBYYzAO6PySWCBu9B051rRD8Nlbw6ZHvg2VtAEH_EC2rs4bBGPhhgswnIDdf4kkUOtkrXk0-MGGERh5ewNNA6muHAjS0nbNxJ6REHIDoVthxLZ0pUWUOaRUxtmqy6eDWOLgUyo5SUIg9onjmOfVD4pLgzCDoBeEAGF37mZKqeL-af2y2YKRJrezNmret4Yo27hi1wPDtzkYArMTT-49gv2OO15ofs1D0Jh
            sb.Append($"&token=03AGdBq24TWv3xEpwJgauNVQpUInHX2dKUht0HMCagbdRO7wRLJHYCknzeqw8jxMHzz6yFXqFPEY-vSC7L1tB8W5BYAXP4wPwHbE3pEMe7G7yNRh1ySC6WojEoIRSuzNHpaU90zdzrbwQaB5w0LWpxOwgXaNvxv14Mvbd260ucowqBYYzAO6PySWCBu9B051rRD8Nlbw6ZHvg2VtAEH_EC2rs4bBGPhhgswnIDdf4kkUOtkrXk0-MGGERh5ewNNA6muHAjS0nbNxJ6REHIDoVthxLZ0pUWUOaRUxtmqy6eDWOLgUyo5SUIg9onjmOfVD4pLgzCDoBeEAGF37mZKqeL-af2y2YKRJrezNmret4Yo27hi1wPDtzkYArMTT-49gv2OO15ofs1D0Jh");
            //&code=SPFB.Si-6.20
            sb.Append($"&code={security.Code}");
            //&apply=0
            sb.Append($"&apply=0");
            //&df=27
            sb.Append($"&df={day}");
            //&mf=4
            sb.Append($"&mf={month_minus_one}");
            //&yf=2020
            sb.Append($"&yf={year_full}");
            //&from=27.05.2020
            sb.Append($"&from={day}.{month_original}.{year_full}");
            //&dt=27
            sb.Append($"&dt={day}");
            //&mt=4
            sb.Append($"&mt={month_minus_one}");
            //&yt=2020
            sb.Append($"&yt={year_full}");
            //&to=27.05.2020
            sb.Append($"&to={day}.{month_original}.{year_full}");
            //&p=1            --  p — период котировок (тики, 1 мин., 5 мин., 10 мин., 15 мин., 30 мин., 1 час, 1 день, 1 неделя, 1 месяц)
            sb.Append($"&p=1");
            //&f=SPFB.Si-6.20_200527_200527
            sb.Append($"&f={security.Code}_{year_short}{nowDate.Month:D2}{nowDate.Day:D2}_{year_short}{nowDate.Month:D2}{nowDate.Day:D2}");
            //&e=.txt
            sb.Append($"&e=.txt");
            //&cn=SPFB.Si-6.20
            sb.Append($"&cn={security.Code}");
            //&dtf=1          --  dtf — формат даты (1 — ггггммдд, 2 — ггммдд, 3 — ддммгг, 4 — дд/мм/гг, 5 — мм/дд/гг)
            sb.Append($"&dtf=4");
            //&tmf=1          -- tmf — формат времени (1 — ччммсс, 2 — ччмм, 3 — чч: мм: сс, 4 — чч: мм)
            sb.Append($"&tmf=3");
            //&MSOR=1&        -- MSOR — выдавать время (0 — начала свечи, 1 — окончания свечи)
            sb.Append($"&MSOR=1");
            //mstime=on
            sb.Append($"&mstime=on");
            //&mstimever=1    -- mstimever — выдавать время (НЕ московское — mstimever=0; московское — mstime='on', mstimever='1')
            sb.Append($"&mstimever=1");
            //&sep=1          -- sep — параметр разделитель полей (1 — запятая (,), 2 — точка (.), 3 — точка с запятой (;), 4 — табуляция (»), 5 — пробел ( ))
            sb.Append($"&sep=1");
            //&sep2=1         -- sep2 — параметр разделитель разрядов (1 — нет, 2 — точка (.), 3 — запятая (,), 4 — пробел ( ), 5 — кавычка ('))
            sb.Append($"&sep2=1");
            //&datf=12        -- datf — Перечень получаемых данных (#1 — TICKER, PER, DATE, TIME, OPEN, HIGH, LOW, CLOSE, VOL; #2 — TICKER, PER, DATE, TIME, OPEN, HIGH, LOW, CLOSE; #3 — TICKER, PER, DATE, TIME, CLOSE, VOL; #4 — TICKER, PER, DATE, TIME, CLOSE; #5 — DATE, TIME, OPEN, HIGH, LOW, CLOSE, VOL; #6 — DATE, TIME, LAST, VOL, ID, OPER).
            sb.Append($"&datf=12");
            //&at=1 - добавлять заголовок
            sb.Append($"&at=1");

            return sb.ToString();



        }



    }
}
