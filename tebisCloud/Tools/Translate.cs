using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFLocalizeExtension.Engine;

namespace tebisCloud.Tools {
    static class Translate {
        public static string TranslateControl(string id) {
            return LocalizeDictionary.Instance.GetLocalizedObject("tebisCloud", "Controls", id, Thread.CurrentThread.CurrentUICulture) as string ?? "";
        }

        public static string TranslateMessage(string id) {
            return LocalizeDictionary.Instance.GetLocalizedObject("tebisCloud", "Messages", id, Thread.CurrentThread.CurrentUICulture) as string ?? "";
        }
    }
}