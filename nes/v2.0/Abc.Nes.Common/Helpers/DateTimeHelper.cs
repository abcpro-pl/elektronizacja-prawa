using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Abc.Nes.Common.Helpers {
    public class DateTimeHelper
    {
        /// <summary>
        /// Return formated date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="datePrefix"></param>
        /// <param name="datePostfix"></param>
        /// <param name="targetCultureInfoName"></param>
        /// <param name="format">0 - day, 1 - month name, 2 - year, 3 - prefix, 4 - postfix to </param>
        /// <returns></returns>
        public static string FormatDateForSignature(DateTime date,
                                                    string datePrefix = "dnia",
                                                    string datePostfix = "r.",
                                                    string targetCultureInfoName = "pl-PL",
                                                    string format = "{3} {0} {1} {2} {4}") {

            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var plPl = new CultureInfo(targetCultureInfoName);
            if (plPl.Name != currentCulture.Name) {
                System.Threading.Thread.CurrentThread.CurrentCulture = plPl;
            }
            string monthName = DateTimeFormatInfo.CurrentInfo.MonthGenitiveNames[date.Month - 1];

            return String.Format(format, date.Day, monthName, date.Year, datePrefix, datePostfix);
        }

    }
}
