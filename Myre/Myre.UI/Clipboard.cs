using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.UI
{
    static class Clipboard
    {
#if !WINDOWS
        public static string Text { get; set; }
#else
        public static string Text
        {
            get { return System.Windows.Clipboard.GetText(); }
            set { System.Windows.Clipboard.SetText(value); }
        }
#endif
    }
}
