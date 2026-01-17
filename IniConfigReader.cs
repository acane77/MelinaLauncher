using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace tbm_launcher
{

    

    class IniConfigReader
    {
        string content;

        public Panel Container = null;

        public string ProgramTitle = ProgramGlobalConfig.DefaultApplicationTitle;

        public delegate void OnReadConfigItemDelegate(ServiceItemControlGroup LI);
        public delegate void OnLoadConfigError(ServiceItemControlGroup LI, ParseError err);

        public OnReadConfigItemDelegate OnReadConfigItem = null;
        public OnLoadConfigError onLoadConfigError = null;

        public IniConfigReader(string filename)
        {
            if (!File.Exists(filename))
            {
                content = "";
            }
            else
            {
                content = File.ReadAllText(filename);
            }
        }

        public class ParseError : Exception
        {
            public int Line;
            public string What;
            public ParseError(int line, string what)
            {
                Line = line; What = what;
            }

            public override string Message => What;
        }

        delegate void EmitParseErrorFuncTy(string what);

        public void LoadConfig()
        {
            string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            LaunchInfoData data = new LaunchInfoData();

            bool initialized = false;
            int line_no = 0;
            int count = 0;
            bool on_error = false;
            bool is_system_config = false;

            EmitParseErrorFuncTy EmitParseError = (string what) =>
            {
                if (is_system_config) return;
                ServiceItemControlGroup LI = new ServiceItemControlGroup(data, count, Container);
                ParseError err = new ParseError(line_no, what);
                onLoadConfigError?.Invoke(LI, err);
                on_error = true;
            };
            foreach (string line in lines)
            {
                line_no++;
                string trimmedLine = line.Trim();
                if (trimmedLine == "")
                    continue;
                if (trimmedLine[0] == '[')
                {
                    if (initialized)
                    {
                        if (!on_error && !is_system_config)
                        {
                            ServiceItemControlGroup LI = new ServiceItemControlGroup(data, count, Container);
                            data = new LaunchInfoData();
                            OnReadConfigItem?.Invoke(LI);
                            count++;
                        }
                    }
                    else
                        initialized = true;
                    if (trimmedLine == "[SYSTEM]")
                    {
                        is_system_config = true;
                    }
                    else is_system_config = false;
                    data.Name = trimmedLine.Replace("[", "").Replace("]", "");
                    data.Command = "";
                    data.PortNumber = 0;
                    data.RequirementCommand = "";
                    on_error = false;
                    continue;
                }
                if (!initialized)
                {
                    EmitParseError("配置文件格式错误，需要分组头");
                }
                if (is_system_config)
                {
                    string[] partsS = line.Split(new[] { '=' }, 2);
                    if (partsS.Length < 2)
                        continue;
                    string settingNameS = partsS[0].Trim().ToLower();
                    string valueS = partsS[1].Trim();
                    switch (settingNameS)
                    {
                        case "title":
                            ProgramTitle = valueS;
                            break;
                        case "comment":
                            break;
                    }
                    continue;
                }
                // if (on_error) continue;
                string[] parts = line.Split(new[] { '=' }, 2);
                if (parts.Length < 2)
                {
                    EmitParseError("配置项格式错误");
                    continue;
                }
                string settingName = parts[0].Trim().ToLower();
                string value = parts[1].Trim();
                try
                {
                    data.SetFieldsFromString(settingName, value);
                }
                catch (InvalidConfigureValueError e)
                {
                    EmitParseError(e.Message);
                }
                
            }

            if (!on_error && !is_system_config)
            {
                ServiceItemControlGroup LI = new ServiceItemControlGroup(data, count, Container);
                data = new LaunchInfoData();
                OnReadConfigItem?.Invoke(LI);
            }
        }
    }
}
