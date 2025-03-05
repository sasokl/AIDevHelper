using System;
using System.Windows.Forms;
using System.Globalization;

namespace AIDevHelper
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CultureInfo culture = CultureInfo.CurrentCulture; // Можно изменить на нужный язык, например, new CultureInfo("ru")
            Application.Run(new MainForm(culture));
        }
    }
}
