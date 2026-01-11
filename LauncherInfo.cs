using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tbm_launcher
{
    public class InvalidConfigureValueError : Exception {
        public InvalidConfigureValueError() : 
            base("分析配置文件时出现了错误。") { }

        public InvalidConfigureValueError(string msg) : base(msg) { }
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
        public string Name = "";
        public string Command = "";
        public string StopCommand = "";
        public int PortNumber = 0;
        public string RequirementCommand = "";
        public string StatusCheckMethod = StatusCheckMethodEnum.CHECK_PORT_USAGE;
        public bool RunBackground = false;
        public string WorkingDirectory = "";
        public string ExecutableName = "";

        public override string ToString()
        {
            if (Name == "")
            {
                return "<Unnamed>";
            }
            return Name;
        }

        public void SetFieldsFromString(string setting_name, string value)
        {
            switch (setting_name)
            {
                case "comment":
                    break;
                case "name":
                    Name = value;
                    break;
                case "command":
                    Command = value;
                    break;
                case "stop_command":
                    StopCommand = value;
                    break;
                case "port":
                    try
                    {
                        PortNumber = Int32.Parse(value);
                        if (PortNumber < 0 || PortNumber > 65535)
                            throw new InvalidConfigureValueError("端口号取值范围是 0-65535.");
                    }
                    catch (Exception ee) { throw new InvalidConfigureValueError(ee.Message); }
                    break;
                case "requirement_command":
                    RequirementCommand = value;
                    break;
                case "status_check_method":
                    if (value != StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE && 
                        value != StatusCheckMethodEnum.CHECK_PORT_USAGE && 
                        value != StatusCheckMethodEnum.NO_CHECK)
                        throw new InvalidConfigureValueError("配置项：错误的启动状态检查方式：" + value +
                            ", 可选的值有：" + 
                            StatusCheckMethodEnum.CHECK_PORT_USAGE + "," + 
                            StatusCheckMethodEnum.NO_CHECK + "和" + 
                            StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE);
                    StatusCheckMethod = value;
                    break;
                case "run_background":
                    RunBackground = value == "1";
                    break;
                case "workdir":
                    WorkingDirectory = value;
                    break;
                case "executable_name":
                    ExecutableName = value;
                    break;
                default:
                    throw new InvalidConfigureValueError("没有此配置项：" + setting_name);
            }
        }

        public string GetFieldValueString(string nameInConfig)
        {
            switch (nameInConfig)
            {
                case "name":
                    return Name;
                case "command":
                    return Command;
                case "stop_command":
                    return StopCommand;
                case "port":
                    return PortNumber.ToString();
                case "requirement_command":
                    return RequirementCommand;
                case "status_check_method":
                    return StatusCheckMethod;
                case "run_background":
                    return RunBackground ? "1" : "0";
                case "workdir":
                    return WorkingDirectory;
                case "executable_name":
                    return ExecutableName;
            }
            return "<Error Value>";
        }

        public static string GetFieldValueString(string n, LaunchInfoData _this) {
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

        private Label Ctrl_Status = null;
        private Label Ctrl_Name = null;
        private Label Ctrl_Port = null;
        private Button Ctrl_Launch = null;
        private Button Ctrl_ChangePort = null;
        private Button Ctrl_InstallRequirement = null;
        private Panel Ctrl_ParentControl = null;
        public LaunchInfoData Data = new LaunchInfoData();
        private int status;

        public string Command { get { return Data.Command; } }
        public string StopCommand { get { return Data.StopCommand; } }
        public int PortNumber { get { return Data.PortNumber; } }
        public string RequirementCommand { get { return Data.RequirementCommand; } }
        public string StatusCheckMethod { get { return Data.StatusCheckMethod; } }
        public bool RunBackground { get { return Data.RunBackground; } }
        public string WorkingDirectory { get { return Data.WorkingDirectory; } }
        public string ExecutableName { get { return Data.ExecutableName; } }


        public string Name
        {
            get { return Data.Name; }
            set { Data.Name = value; Ctrl_Name.Text = Data.Name; }
        }

        public int Port
        {
            get { return Data.PortNumber; }
            set { Data.PortNumber = value; Ctrl_Port.Text = value.ToString(); }
        }

        private int Status
        {
            get { return status; }
            set
            {
                try
                {
                    status = value;
                    if (status == RunningStatus.RETERVING_RUNNING_STATUS)
                    {
                        Ctrl_Status.Text = "获取运行信息...";
                        Ctrl_Launch.Show();
                        Ctrl_Status.ForeColor = Color.Gray;
                        Ctrl_Launch.Enabled = false;
                        Ctrl_Launch.Text = "启动";
                        if (StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK)
                        {
                            Ctrl_Launch.Text = "打开";
                        }
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
                        Ctrl_Launch.Text = "启动";
                        Ctrl_InstallRequirement.Hide();
                        if (StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK)
                        {
                            Ctrl_Launch.Text = "打开";
                        }
                    }
                    else if (status == RunningStatus.LACK_OF_DEPENDENCIES)
                    {
                        Ctrl_Status.Text = "缺少依赖";
                        Ctrl_Status.ForeColor = Color.Red;
                        Ctrl_Launch.Hide();
                        Ctrl_InstallRequirement.Show();
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
                        Ctrl_Launch.Text = "启动";
                        Ctrl_Status.ForeColor = Color.Red;
                        if (StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK)
                        {
                            Ctrl_Launch.Text = "打开";
                        }
                    }
                    else if (status == RunningStatus.CONFIG_ERROR)
                    {
                        Ctrl_Status.Text = "配置文件错误";
                        Ctrl_Status.ForeColor = Color.Red;
                        Ctrl_Launch.Hide();
                        Ctrl_InstallRequirement.Show();
                    }

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
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }

        bool manual_terminate = false;

        public ServiceItemControlGroup(LaunchInfoData data, int ctrl_index, Panel container)
        {
            this.Ctrl_ParentControl = container;
            this.Data = data;

            const int height = 30; // TODO: make it DPI-aware
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
            Port = data.PortNumber;
            Ctrl_Port.Location = new Point(300, height * ctrl_index + text_offset);
            Ctrl_Port.Width = 50;
            Ctrl_Port.Click += (object s, EventArgs e) =>
            {
                if (Status == 1)
                {
                    Process.Start("http://127.0.0.1:" + Port + "/");
                }
            };
            Ctrl_Port.MouseEnter += (object s, EventArgs e) => {
                if (Status == 1)
                {
                    Ctrl_Port.ForeColor = Color.Purple;
                    //Cursor.Current = Cursors.Hand;
                }
            };
            Ctrl_Port.MouseLeave += (object sender, EventArgs e) =>
            {
                if (Status == 1)
                {
                    Ctrl_Port.ForeColor = Color.Blue;
                    //Cursor.Current = Cursors.Default;
                }
            };
            if (Port > 0)
                container.Controls.Add(Ctrl_Port);

            Ctrl_Launch = new Button();
            Ctrl_Launch.Text = "启动";
            Ctrl_Launch.Location = new Point(360, height * ctrl_index);
            Ctrl_Launch.FlatStyle = FlatStyle.System;
            Ctrl_Launch.Enabled = false;
            Ctrl_Launch.Click += (object sender, EventArgs e) =>
            {
                if (Status == 1) StopService();
                else StartService();
            };
            container.Controls.Add(Ctrl_Launch);

            Ctrl_ChangePort = new Button();
            Ctrl_ChangePort.Text = "Change Port";
            Ctrl_ChangePort.Location = new Point(420, height * ctrl_index);
            Ctrl_ChangePort.Visible = false;
            Ctrl_ChangePort.FlatStyle = FlatStyle.System;
            //container.Controls.Add(Ctrl_ChangePort);

            Ctrl_InstallRequirement = new Button();
            Ctrl_InstallRequirement.Text = "安装依赖";
            Ctrl_InstallRequirement.Visible = false;
            Ctrl_InstallRequirement.Location = new Point(260, height * ctrl_index);
            Ctrl_InstallRequirement.FlatStyle = FlatStyle.System;
            container.Controls.Add(Ctrl_InstallRequirement);

            RetriveRunningInformation();
        }

        bool GetProcessRunningStatus()
        {
            string program_name = Command.Split(" ".ToCharArray())[0].Split("\\/".ToCharArray()).Last().Trim();
            if (!program_name.EndsWith(".exe")) {
                program_name += ".exe";
            }
            if (ExecutableName.Trim() != "")
            {
                program_name = ExecutableName.Trim();
            }
            Process[] processes = Process.GetProcesses();
            bool is_regex = ExecutableName.StartsWith("/") && ExecutableName.EndsWith("/");
            if (is_regex)
            {
                try
                {
                    string regex_str = ExecutableName.TrimStart('/').TrimEnd('/').Trim().ToLower();
                    if (regex_str == "")
                    {
                        return false;
                    }
                    Regex regex = new Regex(regex_str);
                    foreach (Process p in processes)
                    {
                        if (regex.IsMatch(p.ProcessName.ToLower()))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                foreach (Process p in processes)
                {
                    if (program_name.ToLower() == p.ProcessName.ToLower() ||
                        program_name.ToLower() == p.ProcessName.ToLower() + ".exe")
                    {
                        return true;
                    }
                }
            }
            

            
            return false;
        }

        public void RetriveRunningInformation(bool slient = false)
        {
            if (StatusCheckMethod == StatusCheckMethodEnum.NO_CHECK)
            {
                Status = RunningStatus.STOPPED;
                return;
            }
            if (!slient)  Status = RunningStatus.RETERVING_RUNNING_STATUS;
            Thread th = new Thread(() =>
            {
                //bool isOpened = IsPortOpen("127.0.0.1", port, TimeSpan.FromMilliseconds(1000));

                bool isOpened = false;
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
                if (isOpened) {
                    Status = RunningStatus.RUNNING;
                    return;
                }
                bool reqSat = CheckRequirements();
                if (!reqSat) Status = RunningStatus.LACK_OF_DEPENDENCIES;
                else Status = RunningStatus.STOPPED;
            });
            th.Start();
        }


        private int ExecuteGetReturnCode(string command)
        {
            //string executable = command.Split(new char[] { ' ' })[0];
            //string args = executable.Length == command.Length ? "" : command.Substring(executable.Length);
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c \"" + command.Replace("\\", "\\\\") + "\"";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                p.WaitForExit();
                return p.ExitCode;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private void StopServiceInternal()
        {
            manual_terminate = true;
            Status = RunningStatus.PROCESSING;
            Thread th2 = new Thread(() =>
            {
                string identifier = "";
                var p = new Process();
                p.StartInfo.FileName = ("taskkill.exe");
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (StatusCheckMethod == StatusCheckMethodEnum.CHECK_PORT_USAGE)
                {
                    TcpHelperUtil tcpHelper = new TcpHelperUtil();
                    var details = tcpHelper.GetPortDetails(PortNumber);
                    if (details.Item1)
                        p.StartInfo.Arguments = "/pid " + details.Item2.ProcessID + " /f";
                    identifier = details.Item2.ProcessID.ToString();
                }
                else if (StatusCheckMethod == StatusCheckMethodEnum.CHECK_EXECUTABLE_EXISTANCE)
                {
                    string program_name = Command.Split(" ".ToCharArray())[0].Split("\\/".ToCharArray()).Last().Trim();
                    if (!program_name.EndsWith(".exe")) program_name += ".exe";
                    if (ExecutableName.Trim().Length > 0)
                    {
                        program_name = ExecutableName.Trim();
                    }
                    p.StartInfo.Arguments = "/im " + program_name + " /f";
                    identifier = program_name;
                }
                p.Start();
                p.WaitForExit();
                if (p.ExitCode == 0)
                    Status = RunningStatus.STOPPED;
                else
                {
                    MessageBox.Show(Ctrl_ParentControl.Parent, "执行该操作的时候发生了错误。\r\n可能是因为\r\n   (1)该程序已经停止运行\r\n   (2)该程序的启动命令并不包含实际执行的应用的名称\r\n\r\nreturn code: " + p.ExitCode + "\r\nidentifier: " + identifier, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Status = RunningStatus.STOPPED;
                }
            });
            th2.Start();
        }

        private void StartServiceInternal()
        {
            manual_terminate = false;
            Thread th1 = new Thread(() => {
                Status = RunningStatus.PROCESSING;
                ExecuteDeteched(Command.Replace("{port}", "" + PortNumber));
            });
            th1.Start();
        }

        public void StartService()
        {
            if (Status == RunningStatus.STOPPED || Status == RunningStatus.FAILED)
                StartServiceInternal();
        }

        public void StopService()
        {
            if (Status == RunningStatus.RUNNING)
            {
                // MessageBox.Show(Ctrl_ParentControl.Parent, StopCommand);
                if (StopCommand == null || StopCommand.Length == 0)
                {
                    StopServiceInternal();
                }
                else
                {
                    Thread th2 = new Thread(() =>
                    {
                        int ret_code = ExecuteGetReturnCode(StopCommand);
                        if (ret_code != 0)
                        {
                            MessageBox.Show(Ctrl_ParentControl.Parent,
                                "执行该操作指定的命令时发生了错误。\r\n\r\nreturn code: " + ret_code, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            Status = RunningStatus.STOPPED;
                        }
                    });
                    th2.Start();
                }
            }
        }

        private bool CheckRequirements()
        {
            if (RequirementCommand == "CHECK_EXISTANCE")
            {
                if (ExecutableName != "")
                    return File.Exists(ExecutableName);
                return File.Exists(Command.Split(" ".ToCharArray())[0].Trim());
            }
            if (RequirementCommand == "DO_NOT_CHECK") 
            {
                return true;    
            }
            return ExecuteGetReturnCode(RequirementCommand) == 0;
        }

        bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    client.EndConnect(result);
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
            //string executable = command.Split(new char[] { ' ' })[0];
            //string args = executable.Length == command.Length ? "" : command.Substring(executable.Length);
            try
            {
                Process p = new Process();
                if (WorkingDirectory != null && WorkingDirectory.Length > 0)
                {
                    p.StartInfo.WorkingDirectory = WorkingDirectory;
                }
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c \"" + command + "\"";
                //MessageBox.Show(p.StartInfo.Arguments);
                p.StartInfo.CreateNoWindow = true;
                if (RunBackground)
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                Status = RunningStatus.RUNNING;

                if (StatusCheckMethod != StatusCheckMethodEnum.NO_CHECK)
                    p.WaitForExit();
                if (p.ExitCode == 0 || manual_terminate)
                {
                    Status = RunningStatus.STOPPED;
                    manual_terminate = false;
                }
                else
                    Status = RunningStatus.FAILED;
            }
            catch
            {
                Status = RunningStatus.FAILED;
            }
        }
    }

}
