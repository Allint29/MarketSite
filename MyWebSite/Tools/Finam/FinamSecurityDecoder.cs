using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebSite.Tools
{
    public static class FinamSecurityDecoder
    {
        public static string FinamCodeToString(string str)
        {
            string result = "";
            switch (Convert.ToInt32(str))
            {
                case 200:
                    result = "МосБиржа топ";
                    break;
                case 1:
                    result = "МосБиржа акции";
                    break;
                case 14:
                    result = "МосБиржа фьючерсы";
                    break;
                case 41:
                    result = "Курс рубля";
                    break;
                case 45:
                    result = "МосБиржа валютный рынок";
                    break;
                case 2:
                    result = "МосБиржа облигации";
                    break;
                case 12:
                    result = "МосБиржа внесписочные облигации";
                    break;
                case 29:
                    result = "МосБиржа пифы";
                    break;
                case 515:
                    result = "Мосбиржа ETF";
                    break;
                case 8:
                    result = "Расписки";
                    break;
                case 519:
                    result = "Еврооблигации";
                    break;
                case 517:
                    result = "Санкт-Петербургская биржа";
                    break;
                case 6:
                    result = "Мировые Индексы";
                    break;
                case 24:
                    result = "Товары";
                    break;
                case 5:
                    result = "Мировые валюты";
                    break;
                case 25:
                    result = "Акции США(BATS)";
                    break;
                case 7:
                    result = "Фьючерсы США";
                    break;
                case 27:
                    result = "Отрасли экономики США";
                    break;
                case 26:
                    result = "Гособлигации США";
                    break;
                case 28:
                    result = "ETF";
                    break;
                case 30:
                    result = "Индексы мировой экономики";
                    break;
                case 91:
                    result = "Российские индексы";
                    break;
                case 3:
                    result = "РТС";
                    break;
                case 20:
                    result = "RTS Board";
                    break;
                case 10:
                    result = "РТС-GAZ";
                    break;
                case 17:
                    result = "ФОРТС Архив";
                    break;
                case 31:
                    result = "Сырье Архив";
                    break;
                case 38:
                    result = "RTS Standard Архив";
                    break;
                case 16:
                    result = "ММВБ Архив";
                    break;
                case 18:
                    result = "РТС Архив";
                    break;
                case 9:
                    result = "СПФБ Архив";
                    break;
                case 32:
                    result = "РТС-BOARD Архив";
                    break;
                case 39:
                    result = "Расписки Архив";
                    break;
                case -1:
                    result = "Отрасли";
                    break;
                default:
                    result = "неизвестно";
                    break;
            };

            return result;
        }
    }
}
