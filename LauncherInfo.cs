using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace tbm_launcher
{
    public class InvalidConfigureValueError : Exception
    {
        public InvalidConfigureValueError()
            : base(AppLocalization.Get("LaunchInfo.AnalysisError"))
        {
        }

        public InvalidConfigureValueError(string msg)
            : base(msg)
        {
        }
    }

    public class StatusCheckMethodEnum
    {
        public static string CHECK_PORT_USAGE = "PORT_USAGE";
        public static string CHECK_EXECUTABLE_EXISTANCE = "EXECUTABLE_EXISTANCE";
        public static string NO_CHECK = "NO_CHECK";
    }

    [Serializable]
    public class LaunchInfoData
    {
        public bool depend_resolvable;
        public string SectionName = string.Empty;
        public string Name = string.Empty;
        public bool DisplayBold = false;
        public string RawDisplayBoldValue;
        public bool Disabled = false;
        public string RawDisabledValue;
        public string Command = string.Empty;
        public string StopCommand = string.Empty;
        public string PortClickCommand = string.Empty;
        public int PortNumber = 0;
        public string RawPortValue;
        public List<string> Environs = new List<string>();
        public string RequirementCommand = string.Empty;
        public string StatusCheckMethod = StatusCheckMethodEnum.CHECK_PORT_USAGE;
        public string RawStatusCheckMethodValue;
        public bool RunBackground = false;
        public string RawRunBackgroundValue;
        public string WorkingDirectory = string.Empty;
        public string ExecutableName = string.Empty;

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }

            if (!string.IsNullOrWhiteSpace(SectionName))
            {
                return SectionName;
            }

            return AppLocalization.Get("LaunchInfo.Unnamed");
        }

        public void SetFieldsFromString(string setting_name, string value)
        {
            switch (setting_name)
            {
                case "comment":
                    break;
                case "name":
                    Name = value ?? string.Empty;
                    break;
                case "display_bold":
                    RawDisplayBoldValue = value;
                    DisplayBold = string.Equals(value, "1", StringComparison.Ordinal)
                        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                    break;
                case "disabled":
                    RawDisabledValue = value;
                    Disabled = string.Equals(value, "1", StringComparison.Ordinal)
                        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                    break;
                case "command":
                    Command = value ?? string.Empty;
                    break;
                case "stop_command":
                    StopCommand = value ?? string.Empty;
                    break;
                case "port_click_command":
                    PortClickCommand = value ?? string.Empty;
                    break;
                case "port":
                    try
                    {
                        RawPortValue = value;
                        int portNumber;
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            portNumber = 0;
                        }
                        else
                        {
                            portNumber = Int32.Parse(value);
                        }

                        if (portNumber < 0 || portNumber > 65535)
                        {
                            throw new InvalidConfigureValueError(AppLocalization.Get("LaunchInfo.PortRange"));
                        }

                        PortNumber = portNumber;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidConfigureValueError(ex.Message);
                    }
                    break;
                case "requirement_command":
                    RequirementCommand = value ?? string.Empty;
                    break;
                case "status_check_method":
                    RawStatusCheckMethodValue = value;
                    if (value != StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE
                        && value != StatusCheckMethodEnum.CHECK_PORT_USAGE
                        && value != StatusCheckMethodEnum.NO_CHECK)
                    {
                        throw new InvalidConfigureValueError(
                            AppLocalization.Format(
                                "LaunchInfo.InvalidStatusCheckMethod",
                                value,
                                StatusCheckMethodEnum.CHECK_PORT_USAGE,
                                StatusCheckMethodEnum.NO_CHECK,
                                StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE));
                    }
                    StatusCheckMethod = value;
                    break;
                case "run_background":
                    RawRunBackgroundValue = value;
                    RunBackground = string.Equals(value, "1", StringComparison.Ordinal)
                        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                    break;
                case "workdir":
                    WorkingDirectory = value ?? string.Empty;
                    break;
                case "executable_name":
                    ExecutableName = value ?? string.Empty;
                    break;
                default:
                    throw new InvalidConfigureValueError(AppLocalization.Format("LaunchInfo.NoSuchField", setting_name));
            }
        }

        public void SetFieldsFromRawString(string setting_name, string value)
        {
            switch (setting_name)
            {
                case "comment":
                    break;
                case "name":
                    Name = value ?? string.Empty;
                    break;
                case "display_bold":
                    RawDisplayBoldValue = value;
                    DisplayBold = string.Equals(value, "1", StringComparison.Ordinal)
                        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                    break;
                case "disabled":
                    RawDisabledValue = value;
                    Disabled = string.Equals(value, "1", StringComparison.Ordinal)
                        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                    break;
                case "command":
                    Command = value ?? string.Empty;
                    break;
                case "stop_command":
                    StopCommand = value ?? string.Empty;
                    break;
                case "port_click_command":
                    PortClickCommand = value ?? string.Empty;
                    break;
                case "port":
                    RawPortValue = value;
                    int parsedPort;
                    if (Int32.TryParse(value, out parsedPort) && parsedPort >= 0 && parsedPort <= 65535)
                    {
                        PortNumber = parsedPort;
                    }
                    else
                    {
                        PortNumber = 0;
                    }
                    break;
                case "requirement_command":
                    RequirementCommand = value ?? string.Empty;
                    break;
                case "status_check_method":
                    RawStatusCheckMethodValue = value;
                    StatusCheckMethod = value ?? string.Empty;
                    break;
                case "run_background":
                    RawRunBackgroundValue = value;
                    RunBackground = string.Equals(value, "1", StringComparison.Ordinal)
                        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                    break;
                case "workdir":
                    WorkingDirectory = value ?? string.Empty;
                    break;
                case "executable_name":
                    ExecutableName = value ?? string.Empty;
                    break;
                default:
                    throw new InvalidConfigureValueError(AppLocalization.Format("LaunchInfo.NoSuchField", setting_name));
            }
        }

        public string GetFieldValueString(string nameInConfig)
        {
            switch (nameInConfig)
            {
                case "section_name":
                    return SectionName;
                case "name":
                    return Name;
                case "display_bold":
                    return RawDisplayBoldValue ?? (DisplayBold ? "1" : "0");
                case "disabled":
                    return RawDisabledValue ?? (Disabled ? "1" : "0");
                case "command":
                    return Command;
                case "stop_command":
                    return StopCommand;
                case "port_click_command":
                    return PortClickCommand;
                case "port":
                    return RawPortValue ?? PortNumber.ToString();
                case "requirement_command":
                    return RequirementCommand;
                case "environs":
                    return string.Join(";", Environs ?? new List<string>());
                case "status_check_method":
                    return RawStatusCheckMethodValue ?? StatusCheckMethod;
                case "run_background":
                    return RawRunBackgroundValue ?? (RunBackground ? "1" : "0");
                case "workdir":
                    return WorkingDirectory;
                case "executable_name":
                    return ExecutableName;
            }
            return AppLocalization.Get("LaunchInfo.ErrorValue");
        }

        public static string GetFieldValueString(string n, LaunchInfoData _this)
        {
            return _this.GetFieldValueString(n);
        }

        public List<string> GetEnvironsCopy()
        {
            return new List<string>(Environs ?? new List<string>());
        }

        public void SetEnvirons(IEnumerable<string> values)
        {
            Environs = values == null
                ? new List<string>()
                : values.Where(value => value != null).ToList();
        }

        public LaunchInfoData Clone()
        {
            return DeepCopy.Copy(this);
        }
    }

    public class ServiceItemControlGroup
    {
        private const int DesignRowHeight = 30;
        private const int DesignTextOffset = 3;
        private const int DesignNameLeft = 0;
        private const int DesignNameWidth = 195;
        private const int DesignStatusLeft = 200;
        private const int DesignStatusWidth = 100;
        private const int DesignPortLeft = 300;
        private const int DesignPortWidth = 50;
        private const int DesignInstallLeft = 260;
        private const int DesignLaunchLeft = 360;
        private const int DesignChangePortLeft = 420;
        private const int DesignButtonWidth = 75;
        private const int DesignButtonHeight = 23;
        private const int DesignIconSize = 16;
        private const int DesignIconGap = 4;

        public class RunningStatus
        {
            public const int RETERVING_RUNNING_STATUS = 0;
            public const int RUNNING = 1;
            public const int STOPPED = 2;
            public const int LACK_OF_DEPENDENCIES = 3;
            public const int PROCESSING = 4;
            public const int FAILED = 6;
            public const int CONFIG_ERROR = 7;
        }

        private Label Ctrl_Status;
        private Label Ctrl_Name;
        private Label Ctrl_Port;
        private PictureBox Ctrl_Icon;
        private Button Ctrl_Launch;
        private Button Ctrl_ChangePort;
        private Button Ctrl_InstallRequirement;
        private Panel Ctrl_ParentControl;
        private readonly int controlIndex;
        public LaunchInfoData Data = new LaunchInfoData();
        private int status;
        private bool manual_terminate;

        public ServiceItemControlGroup(LaunchInfoData data, int ctrl_index, Panel container)
        {
            Ctrl_ParentControl = container;
            controlIndex = ctrl_index;
            Data = data ?? new LaunchInfoData();

            Ctrl_Name = new Label();
            Ctrl_Name.FlatStyle = FlatStyle.System;
            Name = Data.ToString();
            container.Controls.Add(Ctrl_Name);

            Ctrl_Status = new Label();
            Ctrl_Status.FlatStyle = FlatStyle.System;
            Ctrl_Status.Text = AppLocalization.Get("Service.StatusRetrieving");
            Ctrl_Status.ForeColor = Color.Gray;
            if (Data.StatusCheckMethod != StatusCheckMethodEnum.NO_CHECK)
            {
                container.Controls.Add(Ctrl_Status);
            }

            Ctrl_Port = new Label();
            Ctrl_Port.FlatStyle = FlatStyle.System;
            Port = Data.PortNumber;
            Ctrl_Port.Click += (object s, EventArgs e) =>
            {
                if (Status == RunningStatus.RUNNING)
                {
                    ExecutePortClickAction();
                }
            };
            Ctrl_Port.MouseEnter += (object s, EventArgs e) =>
            {
                if (Status == RunningStatus.RUNNING)
                {
                    Ctrl_Port.ForeColor = Color.Purple;
                }
            };
            Ctrl_Port.MouseLeave += (object sender, EventArgs e) =>
            {
                if (Status == RunningStatus.RUNNING)
                {
                    Ctrl_Port.ForeColor = Color.Blue;
                }
            };
            if (Port > 0)
            {
                container.Controls.Add(Ctrl_Port);
            }

            Ctrl_Launch = new Button();
            Ctrl_Launch.Text = AppLocalization.Get("Service.ButtonStart");
            Ctrl_Launch.FlatStyle = FlatStyle.System;
            Ctrl_Launch.Enabled = false;
            Ctrl_Launch.Click += (object sender, EventArgs e) =>
            {
                if (Status == RunningStatus.RUNNING)
                {
                    StopService();
                }
                else
                {
                    StartService();
                }
            };
            container.Controls.Add(Ctrl_Launch);

            Ctrl_ChangePort = new Button();
            Ctrl_ChangePort.Text = AppLocalization.Get("Service.ButtonChangePort");
            Ctrl_ChangePort.Visible = false;
            Ctrl_ChangePort.FlatStyle = FlatStyle.System;

            Ctrl_InstallRequirement = new Button();
            Ctrl_InstallRequirement.Text = AppLocalization.Get("Service.ButtonInstallDependencies");
            Ctrl_InstallRequirement.Visible = false;
            Ctrl_InstallRequirement.FlatStyle = FlatStyle.System;
            container.Controls.Add(Ctrl_InstallRequirement);

            Bitmap icon = LaunchItemIconCache.Resolve(Data.Command, Data.WorkingDirectory, Data.ExecutableName, Data.PortNumber);
            if (icon != null)
            {
                Ctrl_Icon = new PictureBox();
                Ctrl_Icon.Image = icon;
                Ctrl_Icon.SizeMode = PictureBoxSizeMode.Zoom;
                Ctrl_Icon.BackColor = container.BackColor;
                Ctrl_Icon.TabStop = false;
                Ctrl_Icon.Margin = new Padding(0);
                container.Controls.Add(Ctrl_Icon);
            }

            ApplyDpi(DpiHelper.GetDpiForControl(container));
            RetriveRunningInformation();
        }

        public string Command { get { return Data.Command; } }
        public string StopCommand { get { return Data.StopCommand; } }
        public string PortClickCommand { get { return Data.PortClickCommand; } }
        public int PortNumber { get { return Data.PortNumber; } }
        public bool DisplayBold { get { return Data.DisplayBold; } }
        public bool Disabled { get { return Data.Disabled; } }
        public string RequirementCommand { get { return Data.RequirementCommand; } }
        public string StatusCheckMethod { get { return Data.StatusCheckMethod; } }
        public bool RunBackground { get { return Data.RunBackground; } }
        public string WorkingDirectory { get { return Data.WorkingDirectory; } }
        public string ExecutableName { get { return Data.ExecutableName; } }
        public bool HasLaunchCommand { get { return !string.IsNullOrWhiteSpace(Command); } }
        public bool ShouldParticipateInBatchOperations
        {
            get
            {
                return !Disabled && HasLaunchCommand && StatusCheckMethod != StatusCheckMethodEnum.NO_CHECK;
            }
        }

        public string Name
        {
            get { return Data.Name; }
            set
            {
                Data.Name = value;
                if (Ctrl_Name != null)
                {
                    InvokeIfRequired(() =>
                    {
                        Ctrl_Name.Text = Data.ToString();
                        ApplyDpi(DpiHelper.GetDpiForControl(Ctrl_ParentControl));
                    });
                }
            }
        }

        public int Port
        {
            get { return Data.PortNumber; }
            set
            {
                Data.PortNumber = value;
                if (Ctrl_Port != null)
                {
                    InvokeIfRequired(() =>
                    {
                        Ctrl_Port.Text = value.ToString();
                        ApplyDpi(DpiHelper.GetDpiForControl(Ctrl_ParentControl));
                    });
                }
            }
        }

        public void ApplyDpi(int dpi)
        {
            if (Ctrl_ParentControl == null || Ctrl_ParentControl.IsDisposed)
            {
                return;
            }

            int effectiveDpi = dpi > 0 ? dpi : DpiHelper.DefaultDpi;
            int rowTop = DpiHelper.ScaleValue(DesignRowHeight * controlIndex, effectiveDpi);
            int rowHeight = DpiHelper.ScaleValue(DesignRowHeight, effectiveDpi);
            int textTop = rowTop + DpiHelper.ScaleValue(DesignTextOffset, effectiveDpi);
            int nameWidth = DpiHelper.ScaleValue(DesignNameWidth, effectiveDpi);
            if (Data.StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK)
            {
                nameWidth += DpiHelper.ScaleValue(DesignStatusWidth, effectiveDpi);
                if (Port == 0)
                {
                    nameWidth += DpiHelper.ScaleValue(DesignPortWidth, effectiveDpi);
                }
            }

            int labelHeight = Math.Max(DpiHelper.ScaleValue(22, effectiveDpi), Ctrl_ParentControl.Font.Height + DpiHelper.ScaleValue(6, effectiveDpi));
            Size buttonSize = new Size(
                DpiHelper.ScaleValue(DesignButtonWidth, effectiveDpi),
                DpiHelper.ScaleValue(DesignButtonHeight, effectiveDpi));

            int nameLeft = DpiHelper.ScaleValue(DesignNameLeft, effectiveDpi);
            if (Ctrl_Icon != null)
            {
                int iconSize = DpiHelper.ScaleValue(DesignIconSize, effectiveDpi);
                int iconGap = DpiHelper.ScaleValue(DesignIconGap, effectiveDpi);
                int iconTop = textTop + (Ctrl_ParentControl.Font.Height - iconSize) / 2;
                if (iconTop < rowTop)
                {
                    iconTop = rowTop;
                }
                Ctrl_Icon.Location = new Point(nameLeft, iconTop);
                Ctrl_Icon.Size = new Size(iconSize, iconSize);
                int indent = iconSize + iconGap;
                nameLeft += indent;
                nameWidth = Math.Max(0, nameWidth - indent);
            }

            if (Ctrl_Name != null)
            {
                Ctrl_Name.Location = new Point(nameLeft, textTop);
                Ctrl_Name.Size = new Size(nameWidth, labelHeight);
                Ctrl_Name.Font = DisplayBold
                    ? new Font(Ctrl_ParentControl.Font, Ctrl_ParentControl.Font.Style | FontStyle.Bold)
                    : Ctrl_ParentControl.Font;
                Ctrl_Name.Height = Math.Max(Ctrl_Name.Height, rowHeight);
            }
            
            if (Ctrl_Status != null)
            {
                Ctrl_Status.Location = new Point(DpiHelper.ScaleValue(DesignStatusLeft, effectiveDpi), textTop);
                Ctrl_Status.Size = new Size(DpiHelper.ScaleValue(DesignStatusWidth, effectiveDpi), labelHeight);
                Ctrl_Status.Font = Ctrl_ParentControl.Font;
            }

            if (Ctrl_Port != null)
            {
                Ctrl_Port.Location = new Point(DpiHelper.ScaleValue(DesignPortLeft, effectiveDpi), textTop);
                Ctrl_Port.Size = new Size(DpiHelper.ScaleValue(DesignPortWidth, effectiveDpi), labelHeight);
                Ctrl_Port.Font = Status == RunningStatus.RUNNING
                ? new Font(Ctrl_ParentControl.Font, FontStyle.Underline)
                : Ctrl_ParentControl.Font;
            }
           
            if (Ctrl_Launch != null)
            {
                Ctrl_Launch.Location = new Point(DpiHelper.ScaleValue(DesignLaunchLeft, effectiveDpi), rowTop);
                Ctrl_Launch.Size = buttonSize;
                Ctrl_Launch.Font = Ctrl_ParentControl.Font;
            }
            
            if (Ctrl_ChangePort != null)
            {
                Ctrl_ChangePort.Location = new Point(DpiHelper.ScaleValue(DesignChangePortLeft, effectiveDpi), rowTop);
                Ctrl_ChangePort.Size = buttonSize;
                Ctrl_ChangePort.Font = Ctrl_ParentControl.Font;
            }
            
            if (Ctrl_InstallRequirement != null)
            {
                Ctrl_InstallRequirement.Location = new Point(DpiHelper.ScaleValue(DesignInstallLeft, effectiveDpi), rowTop);
                Ctrl_InstallRequirement.Size = buttonSize;
                Ctrl_InstallRequirement.Font = Ctrl_ParentControl.Font;
            }

        }

        private int Status
        {
            get { return status; }
            set
            {
                status = value;
                InvokeIfRequired(() =>
                {
                    try
                    {
                        if (status == RunningStatus.RETERVING_RUNNING_STATUS)
                        {
                            Ctrl_Status.Text = AppLocalization.Get("Service.StatusRetrieving");
                            if (HasLaunchCommand)
                            {
                                Ctrl_Launch.Show();
                            }
                            else
                            {
                                Ctrl_Launch.Hide();
                            }
                            Ctrl_Status.ForeColor = Color.Gray;
                            Ctrl_Launch.Enabled = false;
                            Ctrl_Launch.Text = StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK
                                ? AppLocalization.Get("Service.ButtonOpen")
                                : AppLocalization.Get("Service.ButtonStart");
                        }
                        else if (status == RunningStatus.RUNNING)
                        {
                            Ctrl_Status.Text = AppLocalization.Get("Service.StatusRunning");
                            Ctrl_Status.ForeColor = Color.Green;
                            Ctrl_Launch.Enabled = true;
                            Ctrl_Launch.Text = AppLocalization.Get("Service.ButtonStop");
                        }
                        else if (status == RunningStatus.STOPPED)
                        {
                            Ctrl_Status.Text = AppLocalization.Get("Service.StatusStopped");
                            Ctrl_Status.ForeColor = Color.Gray;
                            Ctrl_Launch.Enabled = true;
                            Ctrl_Launch.Text = StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK
                                ? AppLocalization.Get("Service.ButtonOpen")
                                : AppLocalization.Get("Service.ButtonStart");
                            Ctrl_InstallRequirement.Hide();
                        }
                        else if (status == RunningStatus.LACK_OF_DEPENDENCIES)
                        {
                            Ctrl_Status.Text = AppLocalization.Get("Service.StatusMissingDependencies");
                            Ctrl_Status.ForeColor = Color.Red;
                            if (HasLaunchCommand)
                            {
                                Ctrl_Launch.Hide();
                                Ctrl_InstallRequirement.Show();
                            }
                            else
                            {
                                Ctrl_Launch.Hide();
                                Ctrl_InstallRequirement.Hide();
                            }
                        }
                        else if (status == RunningStatus.PROCESSING)
                        {
                            Ctrl_Status.Text = AppLocalization.Get("Service.StatusProcessing");
                            Ctrl_Status.ForeColor = Color.Black;
                            Ctrl_InstallRequirement.Hide();
                            Ctrl_Launch.Enabled = false;
                        }
                        else if (status == RunningStatus.FAILED)
                        {
                            Ctrl_Status.Text = AppLocalization.Get("Service.StatusFailed");
                            Ctrl_Launch.Text = StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK
                                ? AppLocalization.Get("Service.ButtonOpen")
                                : AppLocalization.Get("Service.ButtonStart");
                            Ctrl_Status.ForeColor = Color.Red;
                        }
                        else if (status == RunningStatus.CONFIG_ERROR)
                        {
                            Ctrl_Status.Text = AppLocalization.Get("Service.StatusConfigError");
                            Ctrl_Status.ForeColor = Color.Red;
                            if (HasLaunchCommand)
                            {
                                Ctrl_Launch.Hide();
                                Ctrl_InstallRequirement.Show();
                            }
                            else
                            {
                                Ctrl_Launch.Hide();
                                Ctrl_InstallRequirement.Hide();
                            }
                        }

                        if (!HasLaunchCommand)
                        {
                            Ctrl_Launch.Hide();
                        }

                        if (Ctrl_Port != null)
                        {
                            if (status == RunningStatus.RUNNING)
                            {
                                Ctrl_Port.ForeColor = Color.Blue;
                                Ctrl_Port.Cursor = Cursors.Hand;
                                Ctrl_Port.Font = new Font(Ctrl_ParentControl.Font, FontStyle.Underline);
                            }
                            else
                            {
                                Ctrl_Port.ForeColor = Color.Black;
                                Ctrl_Port.Cursor = Cursors.Default;
                                Ctrl_Port.Font = Ctrl_ParentControl.Font;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
            }
        }

        public void RetriveRunningInformation(bool slient = false)
        {
            if (StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK)
            {
                Status = RunningStatus.STOPPED;
                return;
            }

            if (!slient)
            {
                Status = RunningStatus.RETERVING_RUNNING_STATUS;
            }

            Thread th = new Thread(() =>
            {
                bool isOpened;
                if (StatusCheckMethod == StatusCheckMethodEnum.CHECK_PORT_USAGE)
                {
                    TcpHelperUtil tcpHelper = new TcpHelperUtil();
                    isOpened = tcpHelper.GetPortDetails(Port).Item1;
                }
                else if (StatusCheckMethod == StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE)
                {
                    isOpened = GetProcessRunningStatus();
                }
                else
                {
                    Name = AppLocalization.Format("Service.InvalidStatusCheckFormat", StatusCheckMethod);
                    Status = RunningStatus.CONFIG_ERROR;
                    return;
                }

                if (isOpened)
                {
                    Status = RunningStatus.RUNNING;
                    return;
                }

                bool reqSat = CheckRequirements();
                Status = reqSat ? RunningStatus.STOPPED : RunningStatus.LACK_OF_DEPENDENCIES;
            });
            th.IsBackground = true;
            th.Start();
        }

        public void StartService()
        {
            if (Status == RunningStatus.STOPPED || Status == RunningStatus.FAILED)
            {
                StartServiceInternal();
            }
        }

        public void StopService()
        {
            if (Status != RunningStatus.RUNNING)
            {
                return;
            }

            if (string.IsNullOrEmpty(StopCommand))
            {
                StopServiceInternal();
                return;
            }

            Thread th = new Thread(() =>
            {
                int retCode = ExecuteGetReturnCode(StopCommand);
                if (retCode != 0)
                {
                    if (Ctrl_ParentControl != null)
                    {
                        InvokeIfRequired(() =>
                        {
                            MessageBox.Show(
                                Ctrl_ParentControl.Parent,
                                AppLocalization.Format("Service.ErrorCommandFailed", retCode),
                                AppLocalization.Get("Common.ErrorTitle"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        });
                    }
                }
                else
                {
                    Status = RunningStatus.STOPPED;
                }
            });
            th.IsBackground = true;
            th.Start();
        }

        private void InvokeIfRequired(Action action)
        {
            if (Ctrl_ParentControl == null)
            {
                return;
            }

            if (Ctrl_ParentControl.InvokeRequired)
            {
                Ctrl_ParentControl.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private bool GetProcessRunningStatus()
        {
            string program_name = GetProcessLookupName();
            Process[] processes = Process.GetProcesses();
            bool is_regex = ExecutableName.StartsWith("/") && ExecutableName.EndsWith("/");

            if (is_regex)
            {
                try
                {
                    string regex_str = ExecutableName.TrimStart('/').TrimEnd('/').Trim().ToLowerInvariant();
                    if (regex_str.Length == 0)
                    {
                        return false;
                    }

                    Regex regex = new Regex(regex_str);
                    foreach (Process process in processes)
                    {
                        if (regex.IsMatch(process.ProcessName.ToLowerInvariant()))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }

                return false;
            }

            string loweredProgramName = program_name.ToLowerInvariant();
            foreach (Process process in processes)
            {
                string processName = process.ProcessName.ToLowerInvariant();
                if (loweredProgramName == processName || loweredProgramName == processName + ".exe")
                {
                    return true;
                }
            }

            return false;
        }

        private string GetProcessLookupName()
        {
            string program_name = (Command ?? string.Empty)
                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? string.Empty;
            program_name = program_name.Split("\\/".ToCharArray()).LastOrDefault() ?? string.Empty;
            program_name = program_name.Trim();

            if (!program_name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && program_name.Length > 0)
            {
                program_name += ".exe";
            }

            if (!string.IsNullOrWhiteSpace(ExecutableName))
            {
                program_name = ExecutableName.Trim();
            }

            return program_name;
        }

        private int ExecuteGetReturnCode(string command)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = "/c \"" + (command ?? string.Empty).Replace("\\", "\\\\") + "\"";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.Start();
                    process.WaitForExit();
                    return process.ExitCode;
                }
            }
            catch
            {
                return -1;
            }
        }

        private void ExecutePortClickAction()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(PortClickCommand))
                {
                    StartDetachedCommand(PortClickCommand.Replace("{this.port}", Port.ToString()));
                    return;
                }

                Process.Start("http://localhost:" + Port + "/");
            }
            catch (Exception ex)
            {
                if (Ctrl_ParentControl != null)
                {
                    InvokeIfRequired(() =>
                    {
                        MessageBox.Show(
                            Ctrl_ParentControl.Parent,
                            AppLocalization.Format("Service.ErrorPortClickFailed", ex.Message),
                            AppLocalization.Get("Common.ErrorTitle"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    });
                }
            }
        }

        private void StartDetachedCommand(string command)
        {
            using (Process process = new Process())
            {
                if (!string.IsNullOrWhiteSpace(WorkingDirectory))
                {
                    process.StartInfo.WorkingDirectory = WorkingDirectory;
                }

                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c \"" + (command ?? string.Empty) + "\"";
                process.StartInfo.CreateNoWindow = true;

                if (Data.Environs.Count > 0)
                {
                    // Keep click-command execution aligned with launch-command env injection.
                    process.StartInfo.UseShellExecute = false;
                    foreach (string environmentEntry in Data.Environs ?? new List<string>())
                    {
                        int separatorIndex = environmentEntry.IndexOf('=');
                        if (separatorIndex <= 0)
                        {
                            continue;
                        }

                        string name = environmentEntry.Substring(0, separatorIndex).Trim();
                        string value = environmentEntry.Substring(separatorIndex + 1);
                        if (name.Length == 0)
                        {
                            continue;
                        }

                        process.StartInfo.EnvironmentVariables[name] = value;
                    }
                }

                if (RunBackground)
                {
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }

                process.Start();
            }
        }

        private void StopServiceInternal()
        {
            manual_terminate = true;
            Status = RunningStatus.PROCESSING;
            Thread th = new Thread(() =>
            {
                string identifier = string.Empty;
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "taskkill.exe";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    if (StatusCheckMethod == StatusCheckMethodEnum.CHECK_PORT_USAGE)
                    {
                        TcpHelperUtil tcpHelper = new TcpHelperUtil();
                        var details = tcpHelper.GetPortDetails(PortNumber);
                        if (details.Item1)
                        {
                            process.StartInfo.Arguments = "/pid " + details.Item2.ProcessID + " /f";
                            identifier = details.Item2.ProcessID.ToString();
                        }
                        else
                        {
                            Status = RunningStatus.STOPPED;
                            return;
                        }
                    }
                    else if (StatusCheckMethod == StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE)
                    {
                        identifier = GetProcessLookupName();
                        process.StartInfo.Arguments = "/im " + identifier + " /f";
                    }

                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        Status = RunningStatus.STOPPED;
                    }
                    else
                    {
                        if (Ctrl_ParentControl != null)
                        {
                            InvokeIfRequired(() =>
                            {
                                MessageBox.Show(
                                    Ctrl_ParentControl.Parent,
                                    AppLocalization.Format("Service.ErrorStopFailed", process.ExitCode, identifier),
                                    AppLocalization.Get("Common.ErrorTitle"),
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            });
                        }
                        Status = RunningStatus.STOPPED;
                    }
                }
            });
            th.IsBackground = true;
            th.Start();
        }

        private void StartServiceInternal()
        {
            manual_terminate = false;
            Thread th = new Thread(() =>
            {
                Status = RunningStatus.PROCESSING;
                ExecuteDeteched(Command);
            });
            th.IsBackground = true;
            th.Start();
        }

        private bool CheckRequirements()
        {
            if (RequirementCommand == "CHECK_EXISTANCE")
            {
                if (ExecutableName != string.Empty)
                {
                    return File.Exists(ExecutableName);
                }

                string executable = (Command ?? string.Empty)
                    .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault() ?? string.Empty;
                return File.Exists(executable.Trim());
            }

            if (RequirementCommand == "DO_NOT_CHECK")
            {
                return true;
            }

            return ExecuteGetReturnCode(RequirementCommand) == 0;
        }

        private bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (success)
                    {
                        client.EndConnect(result);
                    }
                    return success;
                }
            }
            catch
            {
                return false;
            }
        }

        private void ExecuteDeteched(string command)
        {
            try
            {
                using (Process process = new Process())
                {
                    if (!string.IsNullOrWhiteSpace(WorkingDirectory))
                    {
                        process.StartInfo.WorkingDirectory = WorkingDirectory;
                    }

                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = "/c \"" + (command ?? string.Empty) + "\"";
                    process.StartInfo.CreateNoWindow = true;

                    if (Data.Environs.Count > 0)
                    {
                        // .NET Framework requires UseShellExecute=false when custom environment variables are supplied.
                        process.StartInfo.UseShellExecute = false;
                        foreach (string environmentEntry in Data.Environs ?? new List<string>())
                        {
                            int separatorIndex = environmentEntry.IndexOf('=');
                            if (separatorIndex <= 0)
                            {
                                continue;
                            }

                            string name = environmentEntry.Substring(0, separatorIndex).Trim();
                            string value = environmentEntry.Substring(separatorIndex + 1);
                            if (name.Length == 0)
                            {
                                continue;
                            }

                            process.StartInfo.EnvironmentVariables[name] = value;
                        }
                    }
                    
                    if (RunBackground)
                    {
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    }
                    process.Start();
                    Status = RunningStatus.RUNNING;

                    if (StatusCheckMethod != StatusCheckMethodEnum.NO_CHECK)
                    {
                        process.WaitForExit();
                    }

                    if (StatusCheckMethod != StatusCheckMethodEnum.NO_CHECK)
                    {
                        if (process.ExitCode == 0 || manual_terminate)
                        {
                            Status = RunningStatus.STOPPED;
                            manual_terminate = false;
                        }
                        else
                        {
                            Status = RunningStatus.FAILED;
                        }
                    }
                }
            }
            catch
            {
                Status = RunningStatus.FAILED;
            }
        }
    }
}
