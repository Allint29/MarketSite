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
using MyWebSite.Models;
using MyWebSite.Services;
using MyWebSite.Tools;
using MyWebSite.ViewModels;

namespace MyWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MyDbContext _db;
        public IConfiguration _configuration { get; }

        public HomeController(ILogger<HomeController> logger, MyDbContext db, IConfiguration configuration)
        {
            _logger = logger;
            _db = db;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            await _db.CreateNewDatabase(_configuration.GetSection("ConnectionStringsToMyDbContext")["MyDbContext"]);

            _db.DataseseIsInitialized = true;

           

            return View();
        }




        /// <summary>
        /// Метод обработки запроса к финаму для получения файлов котировок
        /// Должен принимать Название инструмента, архивный или текущий, временные рамки
        /// Должен формировать запрос на один день котировок и отсылать его на finam.ru
        /// #адрес сайта           /название файла для сохранения   ? название бумаги                       &      число  месяц-1 год   с какого числа  число  месяц-1 год по какое число     название файла                расширение                   таймфрейм      время     информация биржи      &sep1 -если убрать то сепаратор ;   &sep2 - хз&направление&заголовок
        ///#http://export.finam.ru/SPFB.RTS-12.19_191029_191029.txt?market=14&em=490855&code=SPFB.RTS-12.19&apply=0&df=29&mf=9&yf=2019&from=29.10.2019&dt=29&mt=9&yt=2019&to=29.10.2019&p=1&f=SPFB.RTS-12.19_191029_191029&e=.txt&cn=SPFB.RTS-12.19&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        ///#http://export.finam.ru/SPFB.RTS-12.19_191028_191030.txt?market=14&em=490855&code=SPFB.RTS-12.19&apply=0&df=28&mf=9&yf=2019&from=28.10.2019&dt=30&mt=9&yt=2019&to=30.10.2019&p=1&f=SPFB.RTS-12.19_191028_191030&e=.txt&cn=SPFB.RTS-12.19&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        /// 
        /// http://export.finam.ru/SPFB.SBRF-12.19_191028_191028.txt?market=17&code=SPFB.SBRF-12.19&apply=0&df=28&mf=9&yf=2019&from=28.10.2019&dt=28&mt=9&yt=2019&to=28.10.2019&p=1&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        /// http://export.finam.ru/SPFB.Si-3.20_200302_200302.txt?market=17&em=493382&code=SPFB.Si-3.20&apply=0&df=2&mf=2&yf=2020&from=02.03.2020&dt=2&mt=2&yt=2020&to=02.03.2020&p=1&f=SPFB.Si-3.20_200302_200302&e=.txt&cn=SPFB.Si-3.20&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1
        /// http://export.finam.ru/SPFB.Si-6.20_200403_200403.txt?market=14&em=496222&code=SPFB.Si-6.20&apply=0&df=3&mf=3&yf=2020&from=03.04.2020&dt=3&mt=3&yt=2020&to=03.04.2020&p=1&f=SPFB.Si-6.20_200403_200403&e=.txt&cn=SPFB.Si-6.20&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=12&at=1

        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadSecurities(string? security_name, Dictionary<int, string> combo_securities)
        {
            //Название инструмента необходимо для запроса к сайту финам для заполнения комбобокса инструментов
            ViewBag.SecName = security_name ?? "";



            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DownloadSecurities(string security_name, string? testName)
        {
            var g = Request.Form.Keys;
            //RequestToFinam request_to_finam
            var dict_securities = await FinamArraySecuritiesNames.GetSecurityComboList(security_name);
            var dict_to_show = new Dictionary<int, BrokerRepositorySecurity>();
            ViewBag.SecName = security_name;
            ViewBag.SecuritiesDictionary = dict_securities;
            //FinamSecurityViewModel[] dic_sec = new FinamSecurityViewModel[dict_securities.Count()];
            //
            //for (int i = 0; i<dict_securities.Count(); i++)
            //{
            //    var t = (BrokerRepositorySecurity) dict_securities[i];
            //    int res;
            //    if (Int32.TryParse(t.IdString, out res))
            //    {
            //        dic_sec[i] = new FinamSecurityViewModel(){Id = res, Name = $"{t.Name} - {t.IdString}"};
            //        dict_to_show.Add(res, t);
            //    }
            //    
            //}

            //ViewBag.SecuritiesDictionary = new SelectList(dic_sec, "Id", "Name");

            return View(dict_to_show);
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
