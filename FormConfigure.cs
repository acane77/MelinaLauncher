using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static tbm_launcher.IniConfigReader;

namespace tbm_launcher
{
    public partial class FormConfigure : Form
    {
        public FormConfigure()
        {
            InitializeComponent();
        }

        public List<ServiceItemControlGroup> IniConfigureList = null;
        List<MetaInformation<LaunchInfoData>> settingItemConfigs = new List<MetaInformation<LaunchInfoData>>();
        public string SystemTitle = "";

        void InitializeConfigItem()
        {
            settingItemConfigs.Clear();
            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "name",
                FriendlyConfigName = "服务名称",
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_STRING,
                GetValueHandler = (LaunchInfoData p) => { return p.Name; },
                SetValueHandler = (LaunchInfoData p, string val) => { p.Name = val; listConfig.Invalidate(); }
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "command",
                FriendlyConfigName = "启动命令",
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_FILE,
                GetValueHandler = (LaunchInfoData p) => { return p.Command; },
                SetValueHandler = (LaunchInfoData p, string val) => { p.Command = val; }
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "stop_command",
                FriendlyConfigName = "停止命令",
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_FILE,
                GetValueHandler = (LaunchInfoData p) => { return p.StopCommand; },
                SetValueHandler = (LaunchInfoData p, string val) => { p.StopCommand = val; }
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "port",
                FriendlyConfigName = "端口",
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_INT,
                GetValueHandler = (LaunchInfoData p) => { return p.PortNumber.ToString(); },
                SetValueHandler = (LaunchInfoData p, string val) => {
                    try { p.PortNumber = Int32.Parse(val); }
                    catch { p.PortNumber = 0; }    
                }
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "requirement_command",
                FriendlyConfigName = "依赖检查命令",
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_FILE,
                ListItems = new List<MetaInformation<LaunchInfoData>.ListItem>
                {
                    new MetaInformation<LaunchInfoData>.ListItem{ Name = "检查文件是否存在", Value = "CHECK_EXISTANCE" },
                    new MetaInformation<LaunchInfoData>.ListItem{ Name = "不检查依赖", Value = "DO_NOT_CHECK" },
                },
                GetValueHandler = (LaunchInfoData p) => { return p.RequirementCommand; },
                SetValueHandler = (LaunchInfoData p, string val) => { p.RequirementCommand = val; }
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "status_check_method",
                FriendlyConfigName = "运行状态检查",
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_LIST,
                ListItems = new List<MetaInformation<LaunchInfoData>.ListItem>
                {
                    new MetaInformation<LaunchInfoData>.ListItem{ Name = "检查端口占用", Value = StatusCheckMethodEnum.CHECK_PORT_USAGE },
                    new MetaInformation<LaunchInfoData>.ListItem{ Name = "检查进程是否存在", Value = StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE },
                    new MetaInformation<LaunchInfoData>.ListItem{ Name = "不检查运行状态", Value = StatusCheckMethodEnum.NO_CHECK }
                },
                GetValueHandler = (LaunchInfoData p) => { return p.StatusCheckMethod; },
                SetValueHandler = (LaunchInfoData p, string val) => { p.StatusCheckMethod = val; }
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "workdir",
                FriendlyConfigName = "工作目录",
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_DIRECTORY,
                GetValueHandler = (LaunchInfoData p) => { return p.WorkingDirectory; },
                SetValueHandler = (LaunchInfoData p, string val) => { p.WorkingDirectory = val; }
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "executable_name",
                FriendlyConfigName = "可执行文件名称",
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_STRING,
                ListItems = new List<MetaInformation<LaunchInfoData>.ListItem>
                {
                    new MetaInformation<LaunchInfoData>.ListItem{ Name = "填写可执行文件名称", Value = "" },
                    new MetaInformation<LaunchInfoData>.ListItem{ Name = "使用正则表达式(以/开头和结尾)", Value = "//" },
                },
                GetValueHandler = (LaunchInfoData p) => { return p.ExecutableName; },
                SetValueHandler = (LaunchInfoData p, string val) => { p.ExecutableName = val; }
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "run_background",
                FriendlyConfigName = "在后台运行",
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_BOOL,
                GetValueHandler = (LaunchInfoData p) => { return p.RunBackground ? "1" : "0"; },
                SetValueHandler = (LaunchInfoData p, string val) => { p.RunBackground = val == "1"; }
            });

            MetaInformation<LaunchInfoData>.ValueFillHandler = LaunchInfoData.GetFieldValueString;
        }

        string GenerateIniString()
        {
            string str = "[SYSTEM]" + CRLF;
            str += "title=" + textBox1.Text + CRLF + CRLF;
            foreach (object o in listConfig.Items)
            {
                LaunchInfoData p = o as LaunchInfoData;
                str += MetaInformation<LaunchInfoData>.GenerateIniString(settingItemConfigs, p);
            }
            return str;
        }

        void RenderConfigItemList()
        {
            listConfig.Items.Clear();
            if (IniConfigureList != null)
            {
                foreach (ServiceItemControlGroup info in IniConfigureList)
                {
                    listConfig.Items.Add(info.Data);
                }
            }
            if (listConfig.Items.Count > 0)
                listConfig.SelectedIndex = 0;
        }

        private void FormConfigure_Load(object sender, EventArgs e)
        {
            InitializeConfigItem();
            RenderConfigItemList();
            textBox1.Text = SystemTitle;
            Text = "Configure " + SystemTitle;
        }

        private void listConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listConfig.SelectedIndex == -1)
            {
                panel_config.Controls.Clear();
                return;
            }
            LastSelectedConfigItem = listConfig.SelectedIndex;
            MetaInformation<LaunchInfoData>.RenderControlGroup(settingItemConfigs, 
                listConfig.SelectedItem as LaunchInfoData, panel_config);
        }

        private void FormConfigure_FormClosed(object sender, FormClosedEventArgs e)
        {
            Process.Start(Application.ExecutablePath);
            Application.Exit();
        }

        private void btn_launch_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str = GenerateIniString();
            File.WriteAllText(ProgramGlobalConfig.CONFIG_FILENAME, str);
            Close();
        }

        int FindMaxNewConfig()
        {
            int max_epoch = 0;
            Regex pattern = new Regex(@"新建配置项 \((\d+)\)");

            foreach (object conf in listConfig.Items)
            {
                Match match = pattern.Match(conf.ToString());
                if (match.Success)
                {
                    try
                    {
                        int epoch = int.Parse(match.Groups[1].Value);
                        if (epoch > max_epoch)
                        {
                            max_epoch = epoch;
                        }
                    }
                    catch { }
                }
            }
            return max_epoch;
        }

        
        private void button2_Click(object sender, EventArgs e)
        {
            int new_config_count = 0;
            try { new_config_count += FindMaxNewConfig() + 1; }
            catch { new_config_count = 0; }
            string newConfigName = "新建配置项 (" + new_config_count + ")";
            LaunchInfoData L = new LaunchInfoData();
            L.Name = newConfigName;
            int current_idx = listConfig.SelectedIndex;
            int insert_index = current_idx >= 0 ? current_idx + 1 : 0;
            listConfig.Items.Insert(insert_index, L);
            listConfig.SelectedIndex = insert_index;
            listConfig.Focus();
        }

        const string CRLF = "\r\n";

        int LastSelectedConfigItem = -1;

        private void button3_Click(object sender, EventArgs e)
        {
            if (listConfig.SelectedIndex == -1)
                return;
            int sel = listConfig.SelectedIndex;
            listConfig.Items.RemoveAt(sel);
            if (listConfig.Items.Count > 0)
            {
                listConfig.SelectedIndex = Math.Max(0, Math.Min(sel - 1, listConfig.Items.Count - 1));
            }
            listConfig.Focus();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string str = GenerateIniString();
            MessageBox.Show(str);
        }

        private void btn_moveup_Click(object sender, EventArgs e)
        {
            listConfig.Focus();
            if (listConfig.SelectedIndex <= 0)
                return;
            int sel = listConfig.SelectedIndex;
            var tmp = listConfig.Items[sel];
            listConfig.Items[sel] = listConfig.Items[sel - 1];
            listConfig.Items[sel - 1] = tmp;
            listConfig.SelectedIndex = sel - 1;
        }

        private void btn_movedown_Click(object sender, EventArgs e)
        {
            listConfig.Focus();
            if (listConfig.SelectedIndex < 0 || listConfig.SelectedIndex >= listConfig.Items.Count - 1)
                return;
            int sel = listConfig.SelectedIndex;
            var tmp = listConfig.Items[sel];
            listConfig.Items[sel] = listConfig.Items[sel + 1];
            listConfig.Items[sel + 1] = tmp;
            listConfig.SelectedIndex = sel + 1;
        }

        private void move_copy_Click(object sender, EventArgs e)
        {
            listConfig.Focus();
            if (listConfig.SelectedIndex == -1)
                return;
            int sel = listConfig.SelectedIndex;
            LaunchInfoData obj = (listConfig.Items[sel] as LaunchInfoData).Clone();
            obj.Name = obj.Name + " (Copy)";
            listConfig.Items.Insert(sel + 1, obj);
            listConfig.SelectedIndex = sel + 1;
        }
    }
}
