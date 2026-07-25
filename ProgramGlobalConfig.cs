using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tbm_launcher
{
    public class ProgramGlobalConfig
    {
        public static bool StartWithConfigureFlag = false;
        public const string DefaultApplicationTitle = "Melina Launcher";
        public const string SystemSectionName = "system";

        public static string ConfigFilePath
        {
            get
            {
                return Path.ChangeExtension(Application.ExecutablePath, ".toml");
            }
        }
    }
}
