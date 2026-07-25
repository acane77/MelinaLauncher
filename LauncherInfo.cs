using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
            : base("分析配置文件时出现了错误。")
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
        public string Command = string.Empty;
        public string StopCommand = string.Empty;
        public int PortNumber = 0;
        public string RawPortValue;
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

            return "<Unnamed>";
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
                case "command":
                    Command = value ?? string.Empty;
                    break;
                case "stop_command":
                    StopCommand = value ?? string.Empty;
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
                            throw new InvalidConfigureValueError("端口号取值范围是 0-65535.");
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
                            "配置项：错误的启动状态检查方式：" + value +
                            ", 可选的值有：" +
                            StatusCheckMethodEnum.CHECK_PORT_USAGE + "," +
                            StatusCheckMethodEnum.NO_CHECK + "和" +
                            StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE);
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
                    throw new InvalidConfigureValueError("没有此配置项：" + setting_name);
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
                case "command":
                    Command = value ?? string.Empty;
                    break;
                case "stop_command":
                    StopCommand = value ?? string.Empty;
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
                    throw new InvalidConfigureValueError("没有此配置项：" + setting_name);
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
                case "command":
                    return Command;
                case "stop_command":
                    return StopCommand;
                case "port":
                    return RawPortValue ?? PortNumber.ToString();
                case "requirement_command":
                    return RequirementCommand;
                case "status_check_method":
                    return RawStatusCheckMethodValue ?? StatusCheckMethod;
                case "run_background":
                    return RawRunBackgroundValue ?? (RunBackground ? "1" : "0");
                case "workdir":
                    return WorkingDirectory;
                case "executable_name":
                    return ExecutableName;
            }
            return "<Error Value>";
        }

        public static string GetFieldValueString(string n, LaunchInfoData _this)
        {
            return _this.GetFieldValueString(n);
        }

        public LaunchInfoData Clone()
        {
            return DeepCopy.Copy(this);
        }
    }

    public class ServiceItemControlGroup
    {
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
        private Button Ctrl_Launch;
        private Button Ctrl_ChangePort;
        private Button Ctrl_InstallRequirement;
        private Panel Ctrl_ParentControl;
        public LaunchInfoData Data = new LaunchInfoData();
        private int status;
        private bool manual_terminate;

        public ServiceItemControlGroup(LaunchInfoData data, int ctrl_index, Panel container)
        {
            Ctrl_ParentControl = container;
            Data = data ?? new LaunchInfoData();

            const int height = 30;
            const int text_offset = 3;

            Ctrl_Name = new Label();
            Ctrl_Name.FlatStyle = FlatStyle.System;
            Name = Data.ToString();
            Ctrl_Name.Location = new Point(0, height * ctrl_index + text_offset);
            Ctrl_Name.Width = 195;
            container.Controls.Add(Ctrl_Name);

            Ctrl_Status = new Label();
            Ctrl_Status.FlatStyle = FlatStyle.System;
            Ctrl_Status.Text = "获取运行信息...";
            Ctrl_Status.ForeColor = Color.Gray;
            Ctrl_Status.Location = new Point(200, height * ctrl_index + text_offset);
            if (Data.StatusCheckMethod != StatusCheckMethodEnum.NO_CHECK)
            {
                container.Controls.Add(Ctrl_Status);
            }
            else
            {
                Ctrl_Name.Width += 100;
                if (Port == 0)
                {
                    Ctrl_Name.Width += 50;
                }
            }

            Ctrl_Port = new Label();
            Ctrl_Port.FlatStyle = FlatStyle.System;
            Port = Data.PortNumber;
            Ctrl_Port.Location = new Point(300, height * ctrl_index + text_offset);
            Ctrl_Port.Width = 50;
            Ctrl_Port.Click += (object s, EventArgs e) =>
            {
                if (Status == RunningStatus.RUNNING)
                {
                    Process.Start("http://127.0.0.1:" + Port + "/");
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
            Ctrl_Launch.Text = "启动";
            Ctrl_Launch.Location = new Point(360, height * ctrl_index);
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
            Ctrl_ChangePort.Text = "Change Port";
            Ctrl_ChangePort.Location = new Point(420, height * ctrl_index);
            Ctrl_ChangePort.Visible = false;
            Ctrl_ChangePort.FlatStyle = FlatStyle.System;

            Ctrl_InstallRequirement = new Button();
            Ctrl_InstallRequirement.Text = "安装依赖";
            Ctrl_InstallRequirement.Visible = false;
            Ctrl_InstallRequirement.Location = new Point(260, height * ctrl_index);
            Ctrl_InstallRequirement.FlatStyle = FlatStyle.System;
            container.Controls.Add(Ctrl_InstallRequirement);

            RetriveRunningInformation();
        }

        public string Command { get { return Data.Command; } }
        public string StopCommand { get { return Data.StopCommand; } }
        public int PortNumber { get { return Data.PortNumber; } }
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
                return HasLaunchCommand && StatusCheckMethod != StatusCheckMethodEnum.NO_CHECK;
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
                    InvokeIfRequired(() => { Ctrl_Name.Text = Data.ToString(); });
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
                    InvokeIfRequired(() => { Ctrl_Port.Text = value.ToString(); });
                }
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
                            Ctrl_Status.Text = "获取运行信息...";
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
                            Ctrl_Launch.Text = StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK ? "打开" : "启动";
                        }
                        else if (status == RunningStatus.RUNNING)
                        {
                            Ctrl_Status.Text = "正在运行";
                            Ctrl_Status.ForeColor = Color.Green;
                            Ctrl_Launch.Enabled = true;
                            Ctrl_Launch.Text = "停止";
                        }
                        else if (status == RunningStatus.STOPPED)
                        {
                            Ctrl_Status.Text = "已停止";
                            Ctrl_Status.ForeColor = Color.Gray;
                            Ctrl_Launch.Enabled = true;
                            Ctrl_Launch.Text = StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK ? "打开" : "启动";
                            Ctrl_InstallRequirement.Hide();
                        }
                        else if (status == RunningStatus.LACK_OF_DEPENDENCIES)
                        {
                            Ctrl_Status.Text = "缺少依赖";
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
                            Ctrl_Status.Text = "正在处理";
                            Ctrl_Status.ForeColor = Color.Black;
                            Ctrl_InstallRequirement.Hide();
                            Ctrl_Launch.Enabled = false;
                        }
                        else if (status == RunningStatus.FAILED)
                        {
                            Ctrl_Status.Text = "运行失败";
                            Ctrl_Launch.Text = StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK ? "打开" : "启动";
                            Ctrl_Status.ForeColor = Color.Red;
                        }
                        else if (status == RunningStatus.CONFIG_ERROR)
                        {
                            Ctrl_Status.Text = "配置文件错误";
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
                    Name = "InvalidStatusCheck [" + StatusCheckMethod + "]";
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
                                "执行该操作指定的命令时发生了错误。\r\n\r\nreturn code: " + retCode,
                                "Error",
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
                                    "执行该操作的时候发生了错误。\r\n可能是因为\r\n   (1)该程序已经停止运行\r\n   (2)该程序的启动命令并不包含实际执行的应用的名称\r\n\r\nreturn code: " + process.ExitCode + "\r\nidentifier: " + identifier,
                                    "Error",
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
