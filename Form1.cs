using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace tbm_launcher
{
    public partial class Form1 : DpiAwareForm
    {
        private readonly List<ServiceItemControlGroup> LI = new List<ServiceItemControlGroup>();

        public Form1()
        {
            InitializeComponent();
            InitializeDpiAwareness();
            ApplyLocalizedText();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfiguration();

            if (ProgramGlobalConfig.StartWithConfigureFlag)
            {
                button2_Click(null, null);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btn_launch_Click(object sender, EventArgs e)
        {
            btn_launch.Enabled = false;
            foreach (ServiceItemControlGroup serviceItem in LI)
            {
                if (serviceItem.ShouldParticipateInBatchOperations)
                {
                    serviceItem.StartService();
                }
            }
            btn_launch.Enabled = true;
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            btn_stop.Enabled = false;
            foreach (ServiceItemControlGroup serviceItem in LI)
            {
                if (serviceItem.ShouldParticipateInBatchOperations)
                {
                    serviceItem.StopService();
                }
            }
            btn_stop.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(() =>
            {
                foreach (ServiceItemControlGroup serviceItem in LI)
                {
                    serviceItem.RetriveRunningInformation(true);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timerRefresh.Enabled = false;
            try
            {
                using (var form = new FormConfigure())
                {
                    form.IniConfigureList = LI;
                    form.SystemTitle = Text;
                    form.ShowDialog(this);
                }
            }
            finally
            {
                LoadConfiguration();
            }
        }

        private void LoadConfiguration()
        {
            timerRefresh.Enabled = false;
            panel1.Controls.Clear();
            LI.Clear();

            try
            {
                EnsureConfigFileExists();
                LauncherAppConfig config = LauncherConfigFile.Load(ProgramGlobalConfig.ConfigFilePath);
                AppLocalization.SetLanguage(config.Language);
                ApplyLocalizedText();

                int visibleIndex = 0;
                for (int index = 0; index < config.Items.Count; index++)
                {
                    LaunchInfoData item = config.Items[index];
                    if (item == null || item.Disabled)
                    {
                        continue;
                    }

                    ServiceItemControlGroup group = new ServiceItemControlGroup(item, visibleIndex, panel1);
                    LI.Add(group);
                    visibleIndex++;
                }

                string title = string.IsNullOrWhiteSpace(config.Title)
                    ? ProgramGlobalConfig.DefaultApplicationTitle
                    : config.Title;
                label1.Text = Text = title;
                btn_launch.Visible = !config.HideOneKey;
                btn_stop.Visible = !config.HideOneKey;
                timerRefresh.Interval = Math.Max(1, config.CheckIntervalSeconds) * 1000;
                ApplyServiceItemDpi();
                timerRefresh.Enabled = true;
            }
            catch (LauncherConfigException ex)
            {
                string errorText = ex.LineNumber > 0
                    ? AppLocalization.Format("Main.LoadConfigFailedLine", ex.LineNumber, ex.Message)
                    : AppLocalization.Format("Main.LoadConfigFailed", ex.Message);
                MessageBox.Show(errorText);
                label1.Text = Text = ProgramGlobalConfig.DefaultApplicationTitle;
                btn_launch.Visible = true;
                btn_stop.Visible = true;
                timerRefresh.Interval = 10000;
            }
            catch (Exception ex)
            {
                MessageBox.Show(AppLocalization.Format("Main.LoadConfigFailed", ex.Message));
                label1.Text = Text = ProgramGlobalConfig.DefaultApplicationTitle;
                btn_launch.Visible = true;
                btn_stop.Visible = true;
                timerRefresh.Interval = 10000;
            }
        }

        private static void EnsureConfigFileExists()
        {
            string configFilePath = ProgramGlobalConfig.ConfigFilePath;
            if (!File.Exists(configFilePath))
            {
                LauncherConfigFile.Save(configFilePath, new LauncherAppConfig());
            }
        }

        protected override void OnAfterDpiChanged(int oldDpi, int newDpi)
        {
            base.OnAfterDpiChanged(oldDpi, newDpi);
            ApplyServiceItemDpi();
        }

        private void ApplyServiceItemDpi()
        {
            foreach (ServiceItemControlGroup serviceItem in LI)
            {
                serviceItem.ApplyDpi(CurrentDpi);
            }
        }

        private void ApplyLocalizedText()
        {
            btn_launch.Text = AppLocalization.Get("Main.BatchStart");
            btn_stop.Text = AppLocalization.Get("Main.BatchStop");
            button1.Text = AppLocalization.Get("Main.Refresh");
        }
    }
}
