using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tbm_launcher
{
    class MetaInformation<ValueType>
    {
        public class ListItem
        {
            public string Name;
            public string Value;
        }

        public class ValidateResult
        {
            public int Code;
            public string Message;

            public ValidateResult(int Code, string Message)
            {
                this.Code = Code;
                this.Message = Message;
            }

            public static readonly ValidateResult VALIDATE_PASSED = new ValidateResult(0, "Success");
            public static readonly ValidateResult VALIDATE_FAILED = new ValidateResult(1, "Failed");
        }


        public delegate ValidateResult Validator<T>(T value);
        public delegate string GetValueT(ValueType valueObj);
        public delegate void SetValueT(ValueType valueObj, string value);
        public delegate string ValueFillHandlerT(string configName, ValueType valueType);
        public Validator<string> ValidateInputHandler = null;
        public GetValueT GetValueHandler = null;
        public SetValueT SetValueHandler = null;
        public static ValueFillHandlerT ValueFillHandler = null;

        public const int CONFIG_TYPE_LIST = 1;
        public const int CONFIG_TYPE_STRING = 2;
        public const int CONFIG_TYPE_FILE = 3;
        public const int CONFIG_TYPE_INT = 4;
        public const int CONFIG_TYPE_DECIMAL = 5;
        public const int CONFIG_TYPE_BOOL = 6;
        public const int CONFIG_TYPE_DIRECTORY = 7;
        public string ConfigName;
        public string FriendlyConfigName;
        public int ConfigType;
        public List<ListItem> ListItems;

        private void RenderControl(int index, string value, ValueType info, Control container)
        {
            int baseHeight = index * 30 + 25;
            int baseTitleLeft = 20;
            int baseValueLeft = 120;
            int baseValueRight = 20;
            int baseValueHeightOffset = 0;
            MetaInformation<ValueType> settingItem = this;
            Label labelConfigName = new Label();
            labelConfigName.Text = settingItem.FriendlyConfigName;
            labelConfigName.FlatStyle = FlatStyle.System;
            labelConfigName.Location = new Point(baseTitleLeft, baseHeight + 3);
            labelConfigName.Size = new Size(baseValueLeft - baseTitleLeft, 22);
            container.Controls.Add(labelConfigName);

            Control valueControl = null;
            if (settingItem.ConfigType == CONFIG_TYPE_STRING
                || settingItem.ConfigType == CONFIG_TYPE_FILE
                || settingItem.ConfigType == CONFIG_TYPE_INT
                || settingItem.ConfigType == CONFIG_TYPE_DECIMAL
                || settingItem.ConfigType == CONFIG_TYPE_DIRECTORY)
            {
                if (settingItem.ListItems != null)
                {
                    ComboBox comboBox = new ComboBox();
                    baseValueHeightOffset = -1;
                    foreach (ListItem item in settingItem.ListItems)
                        comboBox.Items.Add(item.Name ?? item.Value);
                    valueControl = comboBox;
                    comboBox.SelectedIndexChanged += (object s, EventArgs ev) => {
                        ComboBox cb = s as ComboBox;
                        int selected_index = cb.SelectedIndex;
                        if (selected_index < 0)
                            return;
                        string text = settingItem.ListItems[selected_index].Value;
                        cb.BeginInvoke(new Action(() => cb.Text = text));
                    };
                }
                else
                {
                    TextBox valueCtrl = new TextBox();
                    valueControl = valueCtrl;
                }
                valueControl.TextChanged += (object s_, EventArgs e_) => {
                    settingItem.SetValueHandler(info, (s_ as Control).Text);
                };
                valueControl.Text = value;
            }
            else if (settingItem.ConfigType == CONFIG_TYPE_LIST)
            {
                ComboBox dropDownList = new ComboBox();
                valueControl = dropDownList;
                dropDownList.DropDownStyle = ComboBoxStyle.DropDownList;
                if (settingItem.ListItems != null)
                {
                    int selectedIndex = 0; // 默认检查端口占用，所以选0
                    int _currentIndex = 0;
                    foreach (ListItem item in settingItem.ListItems)
                    {
                        dropDownList.Items.Add(item.Name ?? item.Value);
                        dropDownList.SelectedIndexChanged += (object sender_, EventArgs e_) => {
                            settingItem.SetValueHandler(info, settingItem.ListItems[(sender_ as ComboBox).SelectedIndex].Value);
                        };
                        if (item.Value == value)
                            selectedIndex = _currentIndex;
                        _currentIndex++;
                    }
                    dropDownList.Tag = settingItem.ListItems;
                    if (selectedIndex >= 0)
                        dropDownList.SelectedIndex = selectedIndex;
                }
            }
            else if (settingItem.ConfigType == CONFIG_TYPE_BOOL)
            {
                CheckBox checkbox = new CheckBox();
                valueControl = checkbox;
                checkbox.FlatStyle = FlatStyle.System;
                checkbox.Checked = value == "1";
                checkbox.CheckedChanged += (object sender_, EventArgs e_) => {
                    settingItem.SetValueHandler(info, (sender_ as CheckBox).Checked ? "1" : "0");
                };
                
                checkbox.Text = labelConfigName.Text;
                labelConfigName.Text = "";
            }
            valueControl.Location = new Point(baseValueLeft, baseHeight + baseValueHeightOffset);
            valueControl.Size = new Size(container.Width - baseValueLeft - baseValueRight, 22);
            Control additionalButton = null;
            // add special buttons for certain ConfigTypes
            if (settingItem.ConfigType == CONFIG_TYPE_FILE ||
                settingItem.ConfigType == CONFIG_TYPE_DIRECTORY)
            {
                int additionalButtonWidth = 50;
                int baseAdditionalButtonRight = baseValueRight;
                
                valueControl.Size = new Size(container.Width - baseValueLeft - baseValueRight - additionalButtonWidth, 22);
                baseValueRight = container.Width - additionalButtonWidth;
                Button button = new Button();
                button.Text = "浏览...";
                button.Location = new Point(container.Width - baseAdditionalButtonRight - 50, baseHeight - 3);
                button.FlatStyle = FlatStyle.System;
                button.Size = new Size(additionalButtonWidth, 28);
                button.FlatStyle = FlatStyle.System;
                if (settingItem.ConfigType == CONFIG_TYPE_FILE)
                {
                    button.Click += (object _s, EventArgs _e) =>
                    {
                        OpenFileDialog dialog = new OpenFileDialog();
                        dialog.InitialDirectory = Application.StartupPath;
                        dialog.CheckFileExists = true;
                        var res = dialog.ShowDialog();
                        if (res == DialogResult.OK)
                        {
                            valueControl.Text = dialog.FileName;
                        }
                    };
                }
                else {
                    button.Click += (object _s, EventArgs _e) =>
                    {
                        FolderBrowserDialog dialog = new FolderBrowserDialog();
                        dialog.SelectedPath = Application.StartupPath;
                        var res = dialog.ShowDialog();
                        if (res == DialogResult.OK)
                        {
                            valueControl.Text = dialog.SelectedPath;
                        }
                    };
                }
                additionalButton = button;
            }
            if (additionalButton != null) {
                container.Controls.Add(additionalButton);
            }
            container.Controls.Add(valueControl);
        }

        public static void RenderControlGroup(List<MetaInformation<ValueType>> items, ValueType info, Control container)
        {
            container.Controls.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                string value = ValueFillHandler?.Invoke(items[i].ConfigName, info) ?? "";
                items[i].RenderControl(i, value, info, container);
            }
        }

        public static string GenerateIniString(List<MetaInformation<ValueType>> items, ValueType p, string group_name = "config_item")
        {
            string str = "[" + group_name.Replace(" ","") + "]\r\n";
            foreach (MetaInformation<ValueType> conf in items)
                str += conf.ConfigName + "=" + conf.GetValueHandler(p).Replace("\r", "").Replace("\n", "") + "\r\n";
            return str + "\r\n";
        }

    }

}
