using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace tbm_launcher
{
    public partial class FormConfigure : DpiAwareForm
    {
        private const string PortCommandPortPlaceholder = "{this.port}";
        private readonly List<MetaInformation<LaunchInfoData>> settingItemConfigs = new List<MetaInformation<LaunchInfoData>>();
        private readonly List<MetaInformation<SystemConfigData>> systemSettingConfigs = new List<MetaInformation<SystemConfigData>>();
        private LauncherAppConfig loadedConfig = new LauncherAppConfig();
        private readonly SystemConfigData systemConfig = new SystemConfigData();
        private int LastSelectedConfigItem = -1;
        private const string CRLF = "\r\n";

        private enum ConfigEditorEntryKind
        {
            System,
            Service
        }

        private sealed class SystemConfigData
        {
            public string Title = string.Empty;
            public bool HideOneKey = false;
            public string RawHideOneKeyValue;
            public int CheckIntervalSeconds = 10;
            public string RawCheckIntervalValue;
            public string Language = AppLocalization.DefaultLanguageCode;
            public string RawLanguageValue;
            public Dictionary<string, string> Variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            public string GetFieldValueString(string configName)
            {
                if (string.Equals(configName, "title", StringComparison.OrdinalIgnoreCase))
                {
                    return Title;
                }

                if (string.Equals(configName, "hide_one_key", StringComparison.OrdinalIgnoreCase))
                {
                    return RawHideOneKeyValue ?? (HideOneKey ? "1" : "0");
                }

                if (string.Equals(configName, "chk_interval", StringComparison.OrdinalIgnoreCase))
                {
                    return RawCheckIntervalValue ?? CheckIntervalSeconds.ToString();
                }

                if (string.Equals(configName, "language", StringComparison.OrdinalIgnoreCase))
                {
                    return RawLanguageValue ?? Language;
                }

                if (string.Equals(configName, "var", StringComparison.OrdinalIgnoreCase))
                {
                    return LauncherConfigFile.FormatStringMap(Variables);
                }

                return string.Empty;
            }
        }

        private sealed class ConfigEditorEntry
        {
            public ConfigEditorEntryKind Kind;
            public SystemConfigData SystemConfig;
            public LaunchInfoData ServiceConfig;

            public override string ToString()
            {
                if (Kind == ConfigEditorEntryKind.System)
                {
                    return AppLocalization.Get("Config.SystemEntry");
                }

                return ServiceConfig == null ? AppLocalization.Get("LaunchInfo.Unnamed") : ServiceConfig.ToString();
            }
        }

        private bool isLoaded;

        public FormConfigure()
        {
            InitializeComponent();
            InitializeDpiAwareness();
            ApplyLocalizedText();
        }

        public List<ServiceItemControlGroup> IniConfigureList = null;
        public string SystemTitle = string.Empty;

        private void FormConfigure_Load(object sender, EventArgs e)
        {
            LoadRawConfiguration();
            AppLocalization.SetLanguage(systemConfig.Language);
            InitializeConfigItem();
            ApplyLocalizedText();
            textBox1.Visible = false;
            textBox1.TabStop = false;
            label3.Visible = false;
            RenderConfigItemList();
            UpdateWindowTitle();
            isLoaded = true;
        }

        private void FormConfigure_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void listConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConfigEditorEntry entry = GetSelectedEditorEntry();
            if (entry == null)
            {
                panel_config.Controls.Clear();
                return;
            }

            LastSelectedConfigItem = listConfig.SelectedIndex;
            if (entry.Kind == ConfigEditorEntryKind.System)
            {
                MetaInformation<SystemConfigData>.RenderControlGroup(
                    systemSettingConfigs,
                    entry.SystemConfig,
                    panel_config);
            }
            else
            {
                MetaInformation<LaunchInfoData>.RenderControlGroup(
                    settingItemConfigs,
                    entry.ServiceConfig,
                    panel_config);
            }
        }

        private void btn_launch_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                LauncherAppConfig config = BuildConfiguration();
                LauncherConfigFile.Save(ProgramGlobalConfig.ConfigFilePath, config);
                Close();
            }
            catch (LauncherConfigException ex)
            {
                string errorText = ex.LineNumber > 0
                    ? AppLocalization.Format("Config.SaveFailedLine", ex.LineNumber, ex.Message)
                    : AppLocalization.Format("Config.SaveFailed", ex.Message);
                MessageBox.Show(errorText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(AppLocalization.Format("Config.SaveFailed", ex.Message));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int newConfigCount;
            try
            {
                newConfigCount = FindMaxNewConfig() + 1;
            }
            catch
            {
                newConfigCount = 0;
            }

            string newConfigName = AppLocalization.Format("Config.NewItemFormat", newConfigCount);
            LaunchInfoData item = new LaunchInfoData();
            item.Name = newConfigName;
            item.SectionName = LauncherConfigFile.CreateRandomSectionName(GetExistingSectionNames());
            int currentIndex = listConfig.SelectedIndex;
            int insertIndex = currentIndex >= 0 ? currentIndex + 1 : 0;
            listConfig.Items.Insert(insertIndex, CreateServiceEntry(item));
            listConfig.SelectedIndex = insertIndex;
            listConfig.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ConfigEditorEntry entry = GetSelectedEditorEntry();
            if (entry == null)
            {
                return;
            }

            if (entry.Kind == ConfigEditorEntryKind.System)
            {
                MessageBox.Show(AppLocalization.Get("Config.SystemItemCannotDelete"));
                return;
            }

            int selectedIndex = listConfig.SelectedIndex;
            listConfig.Items.RemoveAt(selectedIndex);
            if (listConfig.Items.Count > 0)
            {
                listConfig.SelectedIndex = Math.Max(0, Math.Min(selectedIndex - 1, listConfig.Items.Count - 1));
            }
            listConfig.Focus();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show(GenerateTomlString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(AppLocalization.Format("Config.PreviewFailed", ex.Message));
            }
        }

        private void btn_moveup_Click(object sender, EventArgs e)
        {
            listConfig.Focus();
            if (listConfig.SelectedIndex <= 0)
            {
                return;
            }

            int selectedIndex = listConfig.SelectedIndex;
            object temp = listConfig.Items[selectedIndex];
            listConfig.Items[selectedIndex] = listConfig.Items[selectedIndex - 1];
            listConfig.Items[selectedIndex - 1] = temp;
            listConfig.SelectedIndex = selectedIndex - 1;
        }

        private void btn_movedown_Click(object sender, EventArgs e)
        {
            listConfig.Focus();
            if (listConfig.SelectedIndex < 0 || listConfig.SelectedIndex >= listConfig.Items.Count - 1)
            {
                return;
            }

            int selectedIndex = listConfig.SelectedIndex;
            object temp = listConfig.Items[selectedIndex];
            listConfig.Items[selectedIndex] = listConfig.Items[selectedIndex + 1];
            listConfig.Items[selectedIndex + 1] = temp;
            listConfig.SelectedIndex = selectedIndex + 1;
        }

        private void move_copy_Click(object sender, EventArgs e)
        {
            listConfig.Focus();
            ConfigEditorEntry entry = GetSelectedEditorEntry();
            if (entry == null || entry.Kind == ConfigEditorEntryKind.System)
            {
                return;
            }

            int selectedIndex = listConfig.SelectedIndex;
            LaunchInfoData source = entry.ServiceConfig;
            LaunchInfoData copy = source.Clone();
            copy.Name = copy.Name + " (" + AppLocalization.Get("Config.CopySuffix") + ")";
            copy.SectionName = LauncherConfigFile.CreateRandomSectionName(GetExistingSectionNames());
            listConfig.Items.Insert(selectedIndex + 1, CreateServiceEntry(copy));
            listConfig.SelectedIndex = selectedIndex + 1;
        }

        private void InitializeConfigItem()
        {
            systemSettingConfigs.Clear();
            systemSettingConfigs.Add(new MetaInformation<SystemConfigData>
            {
                ConfigName = "title",
                FriendlyConfigName = AppLocalization.Get("Config.Field.Title"),
                ConfigType = MetaInformation<SystemConfigData>.CONFIG_TYPE_STRING,
                GetValueHandler = p => p.Title,
                SetValueHandler = (p, val) =>
                {
                    p.Title = val;
                    UpdateWindowTitle();
                },
                GetReferenceSectionNameHandler = p => ProgramGlobalConfig.SystemSectionName
            });
            systemSettingConfigs.Add(new MetaInformation<SystemConfigData>
            {
                ConfigName = "chk_interval",
                FriendlyConfigName = AppLocalization.Get("Config.Field.CheckInterval"),
                ConfigType = MetaInformation<SystemConfigData>.CONFIG_TYPE_INT,
                GetValueHandler = p => p.CheckIntervalSeconds.ToString(),
                SetValueHandler = (p, val) =>
                {
                    p.RawCheckIntervalValue = val;
                    int parsedSeconds;
                    if (Int32.TryParse(val, out parsedSeconds) && parsedSeconds > 0)
                    {
                        p.CheckIntervalSeconds = parsedSeconds;
                    }
                    else
                    {
                        p.CheckIntervalSeconds = 10;
                    }
                },
                GetReferenceSectionNameHandler = p => ProgramGlobalConfig.SystemSectionName
            });
            systemSettingConfigs.Add(new MetaInformation<SystemConfigData>
            {
                ConfigName = "hide_one_key",
                FriendlyConfigName = AppLocalization.Get("Config.Field.HideOneKey"),
                ConfigType = MetaInformation<SystemConfigData>.CONFIG_TYPE_BOOL,
                GetValueHandler = p => p.HideOneKey ? "1" : "0",
                SetValueHandler = (p, val) =>
                {
                    p.RawHideOneKeyValue = val;
                    p.HideOneKey = string.Equals(val, "1", StringComparison.Ordinal)
                        || string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                },
                GetReferenceSectionNameHandler = p => ProgramGlobalConfig.SystemSectionName
            });
            systemSettingConfigs.Add(new MetaInformation<SystemConfigData>
            {
                ConfigName = "var",
                FriendlyConfigName = AppLocalization.Get("Config.Field.Variables"),
                ConfigType = MetaInformation<SystemConfigData>.CONFIG_TYPE_STRING_MAP,
                GetValueHandler = p => LauncherConfigFile.FormatStringMap(p.Variables),
                GetStringMapHandler = p => new Dictionary<string, string>(p.Variables, StringComparer.OrdinalIgnoreCase),
                SetStringMapHandler = (p, val) =>
                {
                    p.Variables = new Dictionary<string, string>(val ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
                },
                GetReferenceSectionNameHandler = p => ProgramGlobalConfig.SystemSectionName
            });
            systemSettingConfigs.Add(new MetaInformation<SystemConfigData>
            {
                ConfigName = "language",
                FriendlyConfigName = AppLocalization.Get("Config.Field.Language"),
                ConfigType = MetaInformation<SystemConfigData>.CONFIG_TYPE_LIST,
                ListItems = new List<MetaInformation<SystemConfigData>.ListItem>
                {
                    new MetaInformation<SystemConfigData>.ListItem { Name = AppLocalization.Get("Language.English"), Value = AppLanguageCodes.English },
                    new MetaInformation<SystemConfigData>.ListItem { Name = AppLocalization.Get("Language.SimplifiedChinese"), Value = AppLanguageCodes.SimplifiedChinese },
                    new MetaInformation<SystemConfigData>.ListItem { Name = AppLocalization.Get("Language.TraditionalChinese"), Value = AppLanguageCodes.TraditionalChinese },
                    new MetaInformation<SystemConfigData>.ListItem { Name = AppLocalization.Get("Language.Japanese"), Value = AppLanguageCodes.Japanese }
                },
                GetValueHandler = p => p.Language,
                SetValueHandler = (p, val) =>
                {
                    string normalizedLanguage;
                    if (!AppLocalization.TryNormalizeLanguage(val, out normalizedLanguage))
                    {
                        return;
                    }

                    p.RawLanguageValue = val;
                    p.Language = normalizedLanguage;
                },
                GetReferenceSectionNameHandler = p => ProgramGlobalConfig.SystemSectionName
            });
            
            MetaInformation<SystemConfigData>.ValueFillHandler = (configName, info) => info.GetFieldValueString(configName);

            settingItemConfigs.Clear();
            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "section_name",
                FriendlyConfigName = AppLocalization.Get("Config.Field.SectionName"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_STRING,
                GetValueHandler = p => p.SectionName,
                SetValueHandler = (p, val) => { p.SectionName = val; },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "name",
                FriendlyConfigName = AppLocalization.Get("Config.Field.Name"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_STRING,
                GetValueHandler = p => p.Name,
                SetValueHandler = (p, val) => { p.Name = val; listConfig.Invalidate(); },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "command",
                FriendlyConfigName = AppLocalization.Get("Config.Field.Command"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_FILE,
                GetValueHandler = p => p.Command,
                SetValueHandler = (p, val) => { p.Command = val; },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "stop_command",
                FriendlyConfigName = AppLocalization.Get("Config.Field.StopCommand"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_FILE,
                GetValueHandler = p => p.StopCommand,
                SetValueHandler = (p, val) => { p.StopCommand = val; },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "port",
                FriendlyConfigName = AppLocalization.Get("Config.Field.Port"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_INT,
                GetValueHandler = p => p.PortNumber.ToString(),
                SetValueHandler = (p, val) =>
                {
                    p.SetFieldsFromRawString("port", val);
                },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "requirement_command",
                FriendlyConfigName = AppLocalization.Get("Config.Field.RequirementCommand"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_FILE,
                ListItems = new List<MetaInformation<LaunchInfoData>.ListItem>
                {
                    new MetaInformation<LaunchInfoData>.ListItem { Name = AppLocalization.Get("Config.Option.RequirementCheckExistence"), Value = "CHECK_EXISTANCE" },
                    new MetaInformation<LaunchInfoData>.ListItem { Name = AppLocalization.Get("Config.Option.RequirementDoNotCheck"), Value = "DO_NOT_CHECK" },
                },
                GetValueHandler = p => p.RequirementCommand,
                SetValueHandler = (p, val) => { p.RequirementCommand = val; },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "status_check_method",
                FriendlyConfigName = AppLocalization.Get("Config.Field.StatusCheckMethod"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_LIST,
                ListItems = new List<MetaInformation<LaunchInfoData>.ListItem>
                {
                    new MetaInformation<LaunchInfoData>.ListItem { Name = AppLocalization.Get("Config.Option.StatusPortUsage"), Value = StatusCheckMethodEnum.CHECK_PORT_USAGE },
                    new MetaInformation<LaunchInfoData>.ListItem { Name = AppLocalization.Get("Config.Option.StatusExecutableExistence"), Value = StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE },
                    new MetaInformation<LaunchInfoData>.ListItem { Name = AppLocalization.Get("Config.Option.StatusNoCheck"), Value = StatusCheckMethodEnum.NO_CHECK }
                },
                GetValueHandler = p => p.StatusCheckMethod,
                SetValueHandler = (p, val) => { p.SetFieldsFromRawString("status_check_method", val); },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "workdir",
                FriendlyConfigName = AppLocalization.Get("Config.Field.Workdir"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_DIRECTORY,
                GetValueHandler = p => p.WorkingDirectory,
                SetValueHandler = (p, val) => { p.WorkingDirectory = val; },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "environs",
                FriendlyConfigName = AppLocalization.Get("Config.Field.Environs"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_STRING_ARRAY,
                GetValueHandler = p => string.Join(";", p.GetEnvironsCopy()),
                GetStringArrayHandler = p => p.GetEnvironsCopy(),
                SetStringArrayHandler = (p, val) => { p.SetEnvirons(val); },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "executable_name",
                FriendlyConfigName = AppLocalization.Get("Config.Field.ExecutableName"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_STRING,
                ListItems = new List<MetaInformation<LaunchInfoData>.ListItem>
                {
                    new MetaInformation<LaunchInfoData>.ListItem { Name = AppLocalization.Get("Config.Option.ExecutableNamePrompt"), Value = string.Empty },
                    new MetaInformation<LaunchInfoData>.ListItem { Name = AppLocalization.Get("Config.Option.ExecutableRegex"), Value = "//" },
                },
                GetValueHandler = p => p.ExecutableName,
                SetValueHandler = (p, val) => { p.ExecutableName = val; },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "run_background",
                FriendlyConfigName = AppLocalization.Get("Config.Field.RunBackground"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_BOOL,
                GetValueHandler = p => p.RunBackground ? "1" : "0",
                SetValueHandler = (p, val) => { p.SetFieldsFromRawString("run_background", val); },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "port_click_command",
                FriendlyConfigName = AppLocalization.Get("Config.Field.PortClickCommand"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_FILE,
                IsListEditable = true,
                ListItems = GetPortClickCommandOptions(),
                GetValueHandler = p => p.PortClickCommand,
                SetValueHandler = (p, val) => { p.PortClickCommand = val; },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "display_bold",
                FriendlyConfigName = AppLocalization.Get("Config.Field.DisplayBold"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_BOOL,
                GetValueHandler = p => p.GetFieldValueString("display_bold"),
                SetValueHandler = (p, val) => { p.SetFieldsFromRawString("display_bold", val); },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            settingItemConfigs.Add(new MetaInformation<LaunchInfoData>
            {
                ConfigName = "disabled",
                FriendlyConfigName = AppLocalization.Get("Config.Field.Disabled"),
                ConfigType = MetaInformation<LaunchInfoData>.CONFIG_TYPE_BOOL,
                GetValueHandler = p => p.GetFieldValueString("disabled"),
                SetValueHandler = (p, val) => { p.SetFieldsFromRawString("disabled", val); },
                GetReferenceSectionNameHandler = p => p.SectionName
            });

            MetaInformation<LaunchInfoData>.ValueFillHandler = LaunchInfoData.GetFieldValueString;
        }

        private static List<MetaInformation<LaunchInfoData>.ListItem> GetPortClickCommandOptions()
        {
            try
            {
                var options = new List<MetaInformation<LaunchInfoData>.ListItem>();
                var seenAddresses = new HashSet<IPAddress>();
                bool hasLoopbackAddress = false;

                foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.OperationalStatus != OperationalStatus.Up
                        && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }

                    IPInterfaceProperties properties = networkInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
                    {
                        IPAddress address = unicastAddress.Address;
                        if (address.AddressFamily != AddressFamily.InterNetwork || !seenAddresses.Add(address))
                        {
                            continue;
                        }

                        bool isLoopback = IPAddress.Loopback.Equals(address);
                        string interfaceName = string.IsNullOrWhiteSpace(networkInterface.Description)
                            ? networkInterface.Name
                            : networkInterface.Description;
                        string addressText = address.ToString();
                        string displayName = AppLocalization.Format(
                            "Config.Option.PortClickOpenBrowser",
                            addressText,
                            interfaceName);
                        if (isLoopback)
                        {
                            displayName += " (" + AppLocalization.Get("Config.Option.Default") + ")";
                        }

                        var option = new MetaInformation<LaunchInfoData>.ListItem
                        {
                            Name = displayName,
                            Value = "explorer http://" + addressText + ":" + PortCommandPortPlaceholder
                        };
                        if (isLoopback)
                        {
                            hasLoopbackAddress = true;
                            options.Insert(0, option);
                        }
                        else
                        {
                            options.Add(option);
                        }
                    }
                }

                if (hasLoopbackAddress)
                {
                    return options;
                }
            }
            catch
            {
            }

            return new List<MetaInformation<LaunchInfoData>.ListItem>
            {
                new MetaInformation<LaunchInfoData>.ListItem
                {
                    Name = AppLocalization.Format(
                        "Config.Option.PortClickOpenBrowserDefault",
                        AppLocalization.Get("Config.Option.Default")),
                    Value = "explorer http://" + IPAddress.Loopback + ":" + PortCommandPortPlaceholder
                }
            };
        }

        private void RenderConfigItemList()
        {
            listConfig.Items.Clear();
            listConfig.Items.Add(CreateSystemEntry());
            if (loadedConfig != null)
            {
                foreach (LaunchInfoData item in loadedConfig.Items)
                {
                    listConfig.Items.Add(CreateServiceEntry(item));
                }
            }

            if (listConfig.Items.Count > 0)
            {
                listConfig.SelectedIndex = Math.Max(0, Math.Min(LastSelectedConfigItem, listConfig.Items.Count - 1));
            }
        }

        private string GenerateTomlString()
        {
            return LauncherConfigFile.Serialize(BuildConfiguration());
        }

        private LauncherAppConfig BuildConfiguration()
        {
            var config = new LauncherAppConfig();

            foreach (object item in listConfig.Items)
            {
                ConfigEditorEntry entry = item as ConfigEditorEntry;
                if (entry == null)
                {
                    continue;
                }

                if (entry.Kind == ConfigEditorEntryKind.System)
                {
                    config.Title = entry.SystemConfig == null ? string.Empty : entry.SystemConfig.Title;
                    config.HideOneKey = entry.SystemConfig != null && entry.SystemConfig.HideOneKey;
                    config.RawHideOneKeyValue = entry.SystemConfig == null ? null : entry.SystemConfig.RawHideOneKeyValue;
                    config.CheckIntervalSeconds = entry.SystemConfig == null ? 10 : entry.SystemConfig.CheckIntervalSeconds;
                    config.RawCheckIntervalValue = entry.SystemConfig == null ? null : entry.SystemConfig.RawCheckIntervalValue;
                    config.Language = entry.SystemConfig == null ? AppLocalization.DefaultLanguageCode : entry.SystemConfig.Language;
                    config.RawLanguageValue = entry.SystemConfig == null ? null : entry.SystemConfig.RawLanguageValue;
                    config.Variables = entry.SystemConfig == null
                        ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, string>(entry.SystemConfig.Variables, StringComparer.OrdinalIgnoreCase);
                }
                else if (entry.ServiceConfig != null)
                {
                    config.Items.Add(entry.ServiceConfig.Clone());
                }
            }

            LauncherConfigFile.ValidateForSave(config);
            return config;
        }

        private IEnumerable<string> GetExistingSectionNames()
        {
            foreach (object item in listConfig.Items)
            {
                ConfigEditorEntry entry = item as ConfigEditorEntry;
                LaunchInfoData info = entry == null ? null : entry.ServiceConfig;
                if (entry != null
                    && entry.Kind == ConfigEditorEntryKind.Service
                    && info != null
                    && !string.IsNullOrWhiteSpace(info.SectionName))
                {
                    yield return info.SectionName;
                }
            }
        }

        private int FindMaxNewConfig()
        {
            int maxEpoch = 0;
            Regex pattern = new Regex(AppLocalization.Get("Config.NewItemPattern"));

            foreach (object conf in listConfig.Items)
            {
                Match match = pattern.Match(conf.ToString());
                if (!match.Success)
                {
                    continue;
                }

                try
                {
                    int epoch = Int32.Parse(match.Groups[1].Value);
                    if (epoch > maxEpoch)
                    {
                        maxEpoch = epoch;
                    }
                }
                catch
                {
                }
            }

            return maxEpoch;
        }

        private void LoadRawConfiguration()
        {
            try
            {
                loadedConfig = LauncherConfigFile.LoadRaw(ProgramGlobalConfig.ConfigFilePath);
                systemConfig.Title = loadedConfig.Title;
                systemConfig.HideOneKey = loadedConfig.HideOneKey;
                systemConfig.RawHideOneKeyValue = loadedConfig.RawHideOneKeyValue;
                systemConfig.CheckIntervalSeconds = loadedConfig.CheckIntervalSeconds;
                systemConfig.RawCheckIntervalValue = loadedConfig.RawCheckIntervalValue;
                systemConfig.Language = loadedConfig.Language;
                systemConfig.RawLanguageValue = loadedConfig.RawLanguageValue;
                systemConfig.Variables = new Dictionary<string, string>(loadedConfig.Variables, StringComparer.OrdinalIgnoreCase);
            }
            catch (LauncherConfigException ex)
            {
                string errorText = ex.LineNumber > 0
                    ? AppLocalization.Format("Config.LoadRawFailedLine", ex.LineNumber, ex.Message)
                    : AppLocalization.Format("Config.LoadRawFailed", ex.Message);
                MessageBox.Show(errorText);
                loadedConfig = new LauncherAppConfig();
                systemConfig.Title = loadedConfig.Title;
                systemConfig.HideOneKey = loadedConfig.HideOneKey;
                systemConfig.RawHideOneKeyValue = loadedConfig.RawHideOneKeyValue;
                systemConfig.CheckIntervalSeconds = loadedConfig.CheckIntervalSeconds;
                systemConfig.RawCheckIntervalValue = loadedConfig.RawCheckIntervalValue;
                systemConfig.Language = loadedConfig.Language;
                systemConfig.RawLanguageValue = loadedConfig.RawLanguageValue;
                systemConfig.Variables = new Dictionary<string, string>(loadedConfig.Variables, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                MessageBox.Show(AppLocalization.Format("Config.LoadRawFailed", ex.Message));
                loadedConfig = new LauncherAppConfig();
                systemConfig.Title = loadedConfig.Title;
                systemConfig.HideOneKey = loadedConfig.HideOneKey;
                systemConfig.RawHideOneKeyValue = loadedConfig.RawHideOneKeyValue;
                systemConfig.CheckIntervalSeconds = loadedConfig.CheckIntervalSeconds;
                systemConfig.RawCheckIntervalValue = loadedConfig.RawCheckIntervalValue;
                systemConfig.Language = loadedConfig.Language;
                systemConfig.RawLanguageValue = loadedConfig.RawLanguageValue;
                systemConfig.Variables = new Dictionary<string, string>(loadedConfig.Variables, StringComparer.OrdinalIgnoreCase);
            }
        }

        private ConfigEditorEntry GetSelectedEditorEntry()
        {
            return listConfig.SelectedItem as ConfigEditorEntry;
        }

        private ConfigEditorEntry CreateSystemEntry()
        {
            return new ConfigEditorEntry
            {
                Kind = ConfigEditorEntryKind.System,
                SystemConfig = systemConfig
            };
        }

        private ConfigEditorEntry CreateServiceEntry(LaunchInfoData item)
        {
            return new ConfigEditorEntry
            {
                Kind = ConfigEditorEntryKind.Service,
                ServiceConfig = item
            };
        }

        private void UpdateWindowTitle()
        {
            Text = AppLocalization.Format("Config.WindowTitle", systemConfig.Title ?? string.Empty);
        }

        private void ApplyLocalizedText()
        {
            button1.Text = AppLocalization.Get("Common.Save");
            btn_launch.Text = AppLocalization.Get("Common.Cancel");
            button4.Text = AppLocalization.Get("Common.Preview");
            label3.Text = AppLocalization.Get("Config.Field.Title");
            panel_config.Text = AppLocalization.Get("Config.PanelTitle");
            UpdateWindowTitle();
            listConfig.Invalidate();
        }

        protected override void OnAfterDpiChanged(int oldDpi, int newDpi)
        {
            base.OnAfterDpiChanged(oldDpi, newDpi);
            listConfig.ApplyDpiScaling(newDpi);
            listConfig_SelectedIndexChanged(this, EventArgs.Empty);
        }
    }
}
