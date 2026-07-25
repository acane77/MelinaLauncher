using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tbm_launcher
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            DpiHelper.InitializeProcessDpiAwareness();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            foreach (string arg in args)
            {
                if (arg == "-c" || arg == "--config")
                    ProgramGlobalConfig.StartWithConfigureFlag = true;
                else if (arg == "-h")
                {
                    MessageBox.Show(
                        "Usage: tbm_launcher [-c]\r\n" +
                        "\r\n" +
                        "Options:\r\n" +
                        "  -c, --config     Open configure window\r\n" +
                        "  -h               Print this help message" +
                        "");
                    return;
                }
            }
            Application.Run(new Form1());
        }
    }
}
