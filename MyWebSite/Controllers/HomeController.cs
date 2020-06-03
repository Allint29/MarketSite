#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyWebSite.Interfaces;
using MyWebSite.Models;
using MyWebSite.Models.Entities;
using MyWebSite.Services;
using MyWebSite.Tools;
using MyWebSite.ViewModels;

namespace MyWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDbContext _db;
        public IConfiguration Configuration { get; }
        

        public HomeController(ILogger<HomeController> logger, MyDbContext db, IConfiguration configuration)
        {
            _logger = logger;
            _db = db;
            Configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            await _db.CreateNewDatabase(Configuration.GetSection("ConnectionStringsToMyDbContext")["MyDbContext"]);

            _db.DatabaseIsInitialized = true;

            return View();
        }


        /// <summary>
        /// Метод обработки запроса к финаму для получения файлов котировок
        /// Должен принимать Название инструмента, архивный или текущий, временные рамки
        /// Должен формировать запрос на один день котировок и отсылать его на finam.ru
        /// #адрес сайта/название файла для сохранения   ? название бумаги                       &      число  месяц-1 год   с какого числа  число  месяц-1 год по какое число     название файла                расширение                   таймфрейм      время     информация биржи      &sep1 -если убрать то сепаратор ;   &sep2 - хз&направление&заголовок
        ///#http://export.finam.ru/SPFB.RTS-12.19_191029_191029.txt?market=14&em=490855&code=SPFB.RTS-12.19&apply=0&df=29&mf=9&yf=2019&from=29.10.2019&dt=29&mt=9&yt=2019&to=29.10.2019&p=1&f=SPFB.RTS-12.19_191029_191029&e=.txt&cn=SPFB.RTS-12.19&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        ///#http://export.finam.ru/SPFB.RTS-12.19_191028_191030.txt?market=14&em=490855&code=SPFB.RTS-12.19&apply=0&df=28&mf=9&yf=2019&from=28.10.2019&dt=30&mt=9&yt=2019&to=30.10.2019&p=1&f=SPFB.RTS-12.19_191028_191030&e=.txt&cn=SPFB.RTS-12.19&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        /// 
        /// http://export.finam.ru/SPFB.SBRF-12.19_191028_191028.txt?market=17&code=SPFB.SBRF-12.19&apply=0&df=28&mf=9&yf=2019&from=28.10.2019&dt=28&mt=9&yt=2019&to=28.10.2019&p=1&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        /// http://export.finam.ru/SPFB.Si-3.20_200302_200302.txt?market=17&em=493382&code=SPFB.Si-3.20&apply=0&df=2&mf=2&yf=2020&from=02.03.2020&dt=2&mt=2&yt=2020&to=02.03.2020&p=1&f=SPFB.Si-3.20_200302_200302&e=.txt&cn=SPFB.Si-3.20&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        /// http://export.finam.ru/SPFB.Si-6.20_200403_200403.txt?market=14&em=496222&code=SPFB.Si-6.20&apply=0&df=3&mf=3&yf=2020&from=03.04.2020&dt=3&mt=3&yt=2020&to=03.04.2020&p=1&f=SPFB.Si-6.20_200403_200403&e=.txt&cn=SPFB.Si-6.20&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        /// http://export.finam.ru/export9.out?market=17&em=496222&token=03AGdBq24TWv3xEpwJgauNVQpUInHX2dKUht0HMCagbdRO7wRLJHYCknzeqw8jxMHzz6yFXqFPEY-vSC7L1tB8W5BYAXP4wPwHbE3pEMe7G7yNRh1ySC6WojEoIRSuzNHpaU90zdzrbwQaB5w0LWpxOwgXaNvxv14Mvbd260ucowqBYYzAO6PySWCBu9B051rRD8Nlbw6ZHvg2VtAEH_EC2rs4bBGPhhgswnIDdf4kkUOtkrXk0-MGGERh5ewNNA6muHAjS0nbNxJ6REHIDoVthxLZ0pUWUOaRUxtmqy6eDWOLgUyo5SUIg9onjmOfVD4pLgzCDoBeEAGF37mZKqeL-af2y2YKRJrezNmret4Yo27hi1wPDtzkYArMTT-49gv2OO15ofs1D0Jh&code=SPFB.Si-6.20&apply=0&df=28&mf=4&yf=2020&from=28.05.2020&dt=28&mt=4&yt=2020&to=28.05.2020&p=1&f=SPFB.Si-6.20_200528_200528&e=.txt&cn=SPFB.Si-6.20&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        #region Вкладка внести новый инструмент
        /// http://export.finam.ru/export9.out?market=17&em=496222&token=03AGdBq24TWv3xEpwJgauNVQpUInHX2dKUht0HMCagbdRO7wRLJHYCknzeqw8jxMHzz6yFXqFPEY-vSC7L1tB8W5BYAXP4wPwHbE3pEMe7G7yNRh1ySC6WojEoIRSuzNHpaU90zdzrbwQaB5w0LWpxOwgXaNvxv14Mvbd260ucowqBYYzAO6PySWCBu9B051rRD8Nlbw6ZHvg2VtAEH_EC2rs4bBGPhhgswnIDdf4kkUOtkrXk0-MGGERh5ewNNA6muHAjS0nbNxJ6REHIDoVthxLZ0pUWUOaRUxtmqy6eDWOLgUyo5SUIg9onjmOfVD4pLgzCDoBeEAGF37mZKqeL-af2y2YKRJrezNmret4Yo27hi1wPDtzkYArMTT-49gv2OO15ofs1D0Jh&code=SPFB.Si-6.20&apply=0&df=27&mf=4&yf=2020&from=27.05.2020&dt=27&mt=4&yt=2020&to=27.05.2020&p=1&f=SPFB.Si-6.20_200527_200527&e=.txt&cn=SPFB.Si-6.20&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        /// <summary>
        /// Метод генерации страницы с источниками ресурсов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SourcesSecurity()
        {
            var list_of_sources = await _db.GetAllSourceSecurity();
            //В начале есть возможность откуда брать информацию об инструментах которые нужно качать
            ViewBag.SourceSecurityList = list_of_sources;

            return View();
        }

        [HttpPost]
        [ActionName("SourcesSecurity")]
        public async Task<IActionResult> SourcesSecurityAddNewSecurity(BrokerRepositorySecurity security)
        {
            security.Decp = "-1";
            //Тип вводимого инструмента "вручную"
            security.SourceSecurityId = 1;

             var g = await _db.UpdateOneBrokerRepositorySecurity(security);

            return View("SourcesSecurity");
        }

        [HttpPost]
        public async Task<IActionResult> InspectSecurity(string source_security_id)
        {

            var list_of_sources = await _db.GetAllSourceSecurity();
            ViewBag.SourceSecurityList = list_of_sources;

            //пытаюсь распарсить источник инструмента из комбобокс
            bool parsed = Int32.TryParse(source_security_id, out int intIdSource);

            if (!parsed || intIdSource < 0)
                return NotFound("Не удалось привести к int идентификатор, выбранного в комбобоксе инструмента.");
            
            var securities_list = await FinamArraySecuritiesNames.InspectSecuritiesInDb(_db, intIdSource);
            await _db.UpdateAllSecuritiesFromSource(securities_list, intIdSource);

            return View("SourcesSecurity");
        }

        #endregion

        /// <summary>
        /// На форме можно ввести название инструмента и найти его в БД
        /// </summary>
        /// <param name="security_name"></param>
        /// <param name="combo_securities"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("DownloadSecurities")]
        public async Task<IActionResult> GetSecuritiesInDb(string? security_name, Dictionary<int, string> combo_securities)
        {
            //Название инструмента необходимо для уменьшения количества найденных инструментов в комбобоксе
            //Чем точнее введен инструмент - тем легче искать в комбобоксе нужный
            ViewBag.SecName = security_name ?? "";

            var security_list = await _db.GetAllSourceSecurity();

            //В начале есть возможность откуда брать информацию об инструментах которые нужно качать
            ViewBag.SourceSecurityList = security_list;

            return View("DownloadSecurities");
        }

        /// <summary>
        /// Метод выбора инструмента из локальной базы данных
        /// </summary>
        /// <param name="security_name">часть имени инструмента</param>
        /// <param name="source_security_id">ид источника инструмента</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("DownloadSecurities")]
        public async Task<IActionResult> GetSecuritiesInDb(string security_name, int source_security_id)
        {
            var g = Request.Form.Keys;

            var security_list = await _db.GetAllSourceSecurity();
           
            //RequestToFinam request_to_finam
            List<BrokerRepositorySecurity> listSecurities = await FinamArraySecuritiesNames.GetSecurityComboList(_db, security_name, source_security_id);

            //if(listSecurities)

            //В начале есть возможность откуда брать информацию об инструментах которые нужно качать
            ViewBag.SourceSecurityList = security_list;
            ViewBag.SecName = security_name;
            ViewBag.BrokerRepositorySecurities = listSecurities;
            ViewBag.SecCount = listSecurities.Count();

            return View("DownloadSecurities");
        }
        
        [HttpPost]
        public async Task<IActionResult> DownloadFileFromFinam(int combo_security_id, DateTime start_date, DateTime end_date)
        {
            StringBuilder sb = new StringBuilder();

            var security = await _db.GetSecurityById(combo_security_id);

            


            var nowDate = start_date;
            StringBuilder sb2 = new StringBuilder();

            WebClient wb = new WebClient();

            while (nowDate <= end_date)
            {
                string url = FinamArraySecuritiesNames.GetTrades(security, start_date);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

                await using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException()))
                    {
                        Dictionary<string, int> dict_headers = new Dictionary<string, int>();

                        string line = "";
                        //получил массив за день
                        while ((line = reader.ReadLine()) != null)
                        {
                            //разбил строку по запятым
                            //<DATE>,<TIME>,<LAST>,<VOL>,<ID>,<OPER>
                            var mass = line.Split(',');
                            //проверяю - заголовочная это строка или нет
                            if(line.Contains("<"))
                            { 
                                if (mass.Contains("<DATE>"))
                                    dict_headers["<DATE>"] = Array.IndexOf(mass, "<DATE>");
                                if (mass.Contains("<TIME>"))
                                    dict_headers["<TIME>"] = Array.IndexOf(mass, "<TIME>");
                                if (mass.Contains("<LAST>"))
                                    dict_headers["<LAST>"] = Array.IndexOf(mass, "<LAST>");
                                if (mass.Contains("<VOL>"))
                                    dict_headers["<VOL>"] = Array.IndexOf(mass, "<VOL>"); 
                                if (mass.Contains("<ID>"))
                                    dict_headers["<ID>"] = Array.IndexOf(mass, "<ID>");
                                if (mass.Contains("<OPER>"))
                                    dict_headers["<OPER>"] = Array.IndexOf(mass, "<OPER>");

                                //после заполнения заголовочного файла переходу на следующую строку
                                continue;
                            }

                            //если нет заголовка то дальше ничего не делаю и сообщаю, что невозможно идентифицировать
                            if (!dict_headers.ContainsKey("<DATE>")
                                || !dict_headers.ContainsKey("<TIME>")
                                || !dict_headers.ContainsKey("<LAST>")
                                || !dict_headers.ContainsKey("<VOL>")
                                || !dict_headers.ContainsKey("<ID>")
                                || !dict_headers.ContainsKey("<OPER>")
                            )
                                break;

                            //формируем данные для БД и проверяем если тики для данной даты и данного инструмента присутствуют
                            //то обрываем цикл

                            //TODO сделать процедуру для проверки тика к определенной дате, и если такой тик уже есть обрывать цикл
                            //DateTime date_in_db = new DateTime();
                            if (!DateTime.TryParse(mass[dict_headers["<DATE>"]], out DateTime date_in_db))
                                return null;

                            var f = date_in_db;

                            sb2.Append(line);
                        }
                    }
                }
                response.Close();

               // await wb.DownloadStringTaskAsync(sb.ToString());


                nowDate = nowDate.AddDays(1);
            }

            //
            //    WebClient wb = new WebClient();
            //
            //    try
            //    {
            //        string res = await wb.DownloadStringTaskAsync(sb.ToString());
            //    }
            //    catch (Exception)
            //    {
            //        wb.Dispose();
            //        return "Ошибка при скачивании";
            //    }
            //
            //    //прибавляю один день
            //    nowDate = nowDate.AddDays(1);

            return View("DownloadSecurities");
        }

 

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
