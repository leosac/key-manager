using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leosac.KeyManager
{
    public class LangHelper
    {
        public static void ChangeLanguage(string lang)
        {
            var culture = CultureInfo.GetCultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
