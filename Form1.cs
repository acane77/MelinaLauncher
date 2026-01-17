using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tbm_launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<ServiceItemControlGroup> LI = new List<ServiceItemControlGroup>();

        private void Form1_Load(object sender, EventArgs e)
        {
            const string CONFIG_FILENAME = ProgramGlobalConfig.CONFIG_FILENAME;
            bool has_error = false;
            string err_msg = "配置文件存在以下错误：";
            if (!File.Exists(CONFIG_FILENAME))
            {
                using (File.Create(CONFIG_FILENAME)) { }
            }
            try
            {
                IniConfigReader configReader = new IniConfigReader(CONFIG_FILENAME);
                configReader.Container = panel1;
                configReader.onLoadConfigError = (ServiceItemControlGroup L, IniConfigReader.ParseError err) =>
                {
                    LI.Add(L);
                    has_error = true;
                    err_msg += "\r\nLine " + err.Line + ": " + err.What;
                };
                configReader.OnReadConfigItem = (ServiceItemControlGroup L) =>
                {
                    LI.Add(L);
                };
                configReader.LoadConfig();
                timerRefresh.Enabled = true;
                if (configReader.ProgramTitle != "")
                    label1.Text = this.Text = configReader.ProgramTitle;
                if (has_error)
                    MessageBox.Show(err_msg);
            }
            catch (Exception ee)
            {
                MessageBox.Show("加载配置文件失败:" + ee.Message);
            }

            if (ProgramGlobalConfig.StartWithConfigureFlag)
            {
                button2_Click(null, null);
            }
            //buttonConfig.Show(); // todo remove it
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void btn_launch_Click(object sender, EventArgs e)
        {
            btn_launch.Enabled = false;
            foreach (ServiceItemControlGroup l in LI)
                l.StartService();
            btn_launch.Enabled = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            btn_stop.Enabled = false;
            foreach (ServiceItemControlGroup l in LI)
                l.StopService();
            btn_stop.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(() =>
            {
                foreach (ServiceItemControlGroup l in LI)
                    l.RetriveRunningInformation(true);
            });
            th.IsBackground = true;
            th.Start();
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var form = new FormConfigure();
            form.IniConfigureList = LI;
            form.SystemTitle = Text;
            Hide();
            form.ShowDialog();
            Show();
        }
    }
}
