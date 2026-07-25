using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
        public delegate IList<string> GetStringArrayT(ValueType valueObj);
        public delegate void SetStringArrayT(ValueType valueObj, IList<string> value);
        public delegate string GetReferenceSectionNameT(ValueType valueObj);
        public delegate string ValueFillHandlerT(string configName, ValueType valueType);
        public Validator<string> ValidateInputHandler = null;
        public GetValueT GetValueHandler = null;
        public SetValueT SetValueHandler = null;
        public GetStringArrayT GetStringArrayHandler = null;
        public SetStringArrayT SetStringArrayHandler = null;
        public GetReferenceSectionNameT GetReferenceSectionNameHandler = null;
        public static ValueFillHandlerT ValueFillHandler = null;

        public const int CONFIG_TYPE_LIST = 1;
        public const int CONFIG_TYPE_STRING = 2;
        public const int CONFIG_TYPE_FILE = 3;
        public const int CONFIG_TYPE_INT = 4;
        public const int CONFIG_TYPE_DECIMAL = 5;
        public const int CONFIG_TYPE_BOOL = 6;
        public const int CONFIG_TYPE_DIRECTORY = 7;
        public const int CONFIG_TYPE_STRING_ARRAY = 8;
        public string ConfigName;
        public string FriendlyConfigName;
        public int ConfigType;
        public List<ListItem> ListItems;

        private static bool TryParseNumericValue(string rawValue, bool allowDecimal, out decimal parsedValue)
        {
            parsedValue = 0m;
            string normalized = (rawValue ?? string.Empty).Trim();
            if (normalized.Length == 0)
            {
                return false;
            }

            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
            {
                return false;
            }

            if (!allowDecimal && decimal.Truncate(parsedValue) != parsedValue)
            {
                return false;
            }

            return true;
        }

        private static int ResolveDecimalPlaces(string rawValue)
        {
            string normalized = (rawValue ?? string.Empty).Trim();
            int pointIndex = normalized.IndexOf('.');
            if (pointIndex < 0 || pointIndex >= normalized.Length - 1)
            {
                return 2;
            }

            return Math.Max(1, Math.Min(6, normalized.Length - pointIndex - 1));
        }

        private static decimal ResolveDecimalIncrement(int decimalPlaces)
        {
            if (decimalPlaces <= 0)
            {
                return 1m;
            }

            decimal increment = 1m;
            for (int index = 0; index < decimalPlaces; index++)
            {
                increment /= 10m;
            }

            return increment;
        }

        private void TryCopyReferenceToClipboard(ValueType info)
        {
            string sectionName = GetReferenceSectionNameHandler == null
                ? string.Empty
                : GetReferenceSectionNameHandler(info);
            if (string.IsNullOrWhiteSpace(sectionName)
                || string.IsNullOrWhiteSpace(ConfigName)
                || string.Equals(ConfigName, "section_name", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                Clipboard.SetText("{" + sectionName + "." + ConfigName + "}");
            }
            catch
            {
            }
        }

        private void BindReferenceCopy(Control control, ValueType info)
        {
            if (control == null)
            {
                return;
            }

            control.DoubleClick += (object _s, EventArgs _e) =>
            {
                TryCopyReferenceToClipboard(info);
            };
        }

        private void RenderControl(int index, string value, ValueType info, Control container)
        {
            int currentDpi = DpiHelper.GetDpiForControl(container);
            int baseHeight = DpiHelper.ScaleValue(25, currentDpi) + index * DpiHelper.ScaleValue(30, currentDpi);
            int baseTitleLeft = DpiHelper.ScaleValue(20, currentDpi);
            int designLabelWidth = DpiHelper.ScaleValue(100, currentDpi);
            int effectiveLabelWidth = AppLocalization.CurrentLanguageCode == AppLanguageCodes.English
                ? DpiHelper.ScaleValue(150, currentDpi)
                : designLabelWidth;
            int baseValueLeft = baseTitleLeft + effectiveLabelWidth;
            int baseValueRight = DpiHelper.ScaleValue(20, currentDpi);
            int baseValueHeightOffset = 0;
            MetaInformation<ValueType> settingItem = this;
            Label labelConfigName = new Label();
            labelConfigName.Text = settingItem.FriendlyConfigName;
            labelConfigName.FlatStyle = FlatStyle.System;
            labelConfigName.Location = new Point(baseTitleLeft, baseHeight + DpiHelper.ScaleValue(3, currentDpi));
            labelConfigName.Size = new Size(effectiveLabelWidth, DpiHelper.ScaleValue(22, currentDpi));
            BindReferenceCopy(labelConfigName, info);
            container.Controls.Add(labelConfigName);

            Control valueControl = null;
            Control additionalButton = null;
            if (settingItem.ConfigType == CONFIG_TYPE_STRING
                || settingItem.ConfigType == CONFIG_TYPE_FILE
                || settingItem.ConfigType == CONFIG_TYPE_DIRECTORY)
            {
                if (settingItem.ListItems != null)
                {
                    ComboBox comboBox = new ComboBox();
                    baseValueHeightOffset = -DpiHelper.ScaleValue(1, currentDpi);
                    int selectedIndex = -1;
                    int currentIndex = 0;
                    foreach (ListItem item in settingItem.ListItems)
                    {
                        comboBox.Items.Add(item.Name ?? item.Value);
                        if (string.Equals(item.Value, value, StringComparison.Ordinal))
                        {
                            selectedIndex = currentIndex;
                        }
                        currentIndex++;
                    }
                    valueControl = comboBox;
                    comboBox.SelectedIndexChanged += (object s, EventArgs ev) => {
                        ComboBox cb = s as ComboBox;
                        int selected_index = cb.SelectedIndex;
                        if (selected_index < 0)
                            return;
                        settingItem.SetValueHandler(info, settingItem.ListItems[selected_index].Value);
                    };
                    comboBox.TextChanged += (object s, EventArgs ev) =>
                    {
                        ComboBox cb = s as ComboBox;
                        if (cb.DropDownStyle == ComboBoxStyle.DropDown && cb.SelectedIndex < 0)
                        {
                            settingItem.SetValueHandler(info, cb.Text);
                        }
                    };
                    if (selectedIndex >= 0)
                    {
                        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                        comboBox.SelectedIndex = selectedIndex;
                    }
                    else
                    {
                        comboBox.DropDownStyle = ComboBoxStyle.DropDown;
                        comboBox.Text = value;
                    }
                }
                else
                {
                    TextBox valueCtrl = new TextBox();
                    valueControl = valueCtrl;
                    valueControl.TextChanged += (object s_, EventArgs e_) => {
                        settingItem.SetValueHandler(info, (s_ as Control).Text);
                    };
                    valueControl.Text = value;
                }
            }
            else if (settingItem.ConfigType == CONFIG_TYPE_INT
                || settingItem.ConfigType == CONFIG_TYPE_DECIMAL)
            {
                bool allowDecimal = settingItem.ConfigType == CONFIG_TYPE_DECIMAL;
                NumericUpDown numericControl = new NumericUpDown();
                bool isUpdatingNumericValue = true;
                numericControl.Minimum = decimal.MinValue;
                numericControl.Maximum = decimal.MaxValue;
                numericControl.DecimalPlaces = allowDecimal ? ResolveDecimalPlaces(value) : 0;
                numericControl.Increment = ResolveDecimalIncrement(numericControl.DecimalPlaces);
                numericControl.ThousandsSeparator = false;
                numericControl.TextAlign = HorizontalAlignment.Left;

                decimal parsedValue;
                numericControl.Value = TryParseNumericValue(value, allowDecimal, out parsedValue)
                    ? parsedValue
                    : 0m;

                isUpdatingNumericValue = false;
                numericControl.ValueChanged += (object sender_, EventArgs e_) =>
                {
                    if (isUpdatingNumericValue)
                    {
                        return;
                    }

                    NumericUpDown control = sender_ as NumericUpDown;
                    string rawNumericValue = allowDecimal
                        ? control.Value.ToString(CultureInfo.InvariantCulture)
                        : decimal.Truncate(control.Value).ToString(CultureInfo.InvariantCulture);
                    settingItem.SetValueHandler(info, rawNumericValue);
                };

                valueControl = numericControl;
            }
            else if (settingItem.ConfigType == CONFIG_TYPE_LIST)
            {
                ComboBox dropDownList = new ComboBox();
                valueControl = dropDownList;
                if (settingItem.ListItems != null)
                {
                    int selectedIndex = -1;
                    int _currentIndex = 0;
                    foreach (ListItem item in settingItem.ListItems)
                    {
                        dropDownList.Items.Add(item.Name ?? item.Value);
                        if (item.Value == value)
                            selectedIndex = _currentIndex;
                        _currentIndex++;
                    }
                    dropDownList.SelectedIndexChanged += (object sender_, EventArgs e_) => {
                        ComboBox combo = sender_ as ComboBox;
                        if (combo.SelectedIndex >= 0)
                        {
                            string selectedValue = settingItem.ListItems[combo.SelectedIndex].Value;
                            settingItem.SetValueHandler(info, selectedValue);
                        }
                    };
                    dropDownList.TextChanged += (object sender_, EventArgs e_) =>
                    {
                        ComboBox combo = sender_ as ComboBox;
                        if (combo.DropDownStyle == ComboBoxStyle.DropDown && combo.SelectedIndex < 0)
                        {
                            settingItem.SetValueHandler(info, combo.Text);
                        }
                    };
                    dropDownList.Tag = settingItem.ListItems;
                    if (selectedIndex >= 0)
                    {
                        dropDownList.DropDownStyle = ComboBoxStyle.DropDownList;
                        dropDownList.SelectedIndex = selectedIndex;
                    }
                    else
                    {
                        dropDownList.DropDownStyle = ComboBoxStyle.DropDown;
                        dropDownList.Text = value;
                    }
                }
            }
            else if (settingItem.ConfigType == CONFIG_TYPE_BOOL)
            {
                if (value == "1" || value == "0" ||
                    string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    CheckBox checkbox = new CheckBox();
                    valueControl = checkbox;
                    checkbox.FlatStyle = FlatStyle.System;
                    checkbox.Checked = value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                    checkbox.CheckedChanged += (object sender_, EventArgs e_) => {
                        settingItem.SetValueHandler(info, (sender_ as CheckBox).Checked ? "1" : "0");
                    };

                    checkbox.Text = labelConfigName.Text;
                    labelConfigName.Text = "";
                    BindReferenceCopy(checkbox, info);
                }
                else
                {
                    TextBox valueCtrl = new TextBox();
                    valueControl = valueCtrl;
                    valueControl.TextChanged += (object s_, EventArgs e_) => {
                        settingItem.SetValueHandler(info, (s_ as Control).Text);
                    };
                    valueControl.Text = value;
                }
            }
            else if (settingItem.ConfigType == CONFIG_TYPE_STRING_ARRAY)
            {
                TextBox valueCtrl = new TextBox();
                valueCtrl.ReadOnly = true;
                valueCtrl.Text = value;
                valueControl = valueCtrl;

                int additionalButtonWidth = 50;
                int scaledAdditionalButtonWidth = DpiHelper.ScaleValue(additionalButtonWidth, currentDpi);
                int additionalButtonHeight = DpiHelper.ScaleValue(28, currentDpi);
                int baseAdditionalButtonRight = baseValueRight;
                baseValueRight += scaledAdditionalButtonWidth;

                Button button = new Button();
                button.Text = AppLocalization.Get("Common.Edit");
                button.Location = new Point(
                    container.ClientSize.Width - baseAdditionalButtonRight - scaledAdditionalButtonWidth,
                    baseHeight - DpiHelper.ScaleValue(3, currentDpi));
                button.FlatStyle = FlatStyle.System;
                button.Size = new Size(scaledAdditionalButtonWidth, additionalButtonHeight);
                button.Click += (object _s, EventArgs _e) =>
                {
                    var dialog = new FormStringArrayEditor();
                    dialog.Text = settingItem.FriendlyConfigName;
                    dialog.SetItems(settingItem.GetStringArrayHandler == null
                        ? new List<string>()
                        : settingItem.GetStringArrayHandler(info));
                    if (dialog.ShowDialog(container.FindForm()) == DialogResult.OK)
                    {
                        var items = dialog.GetItems();
                        settingItem.SetStringArrayHandler?.Invoke(info, items);
                        valueCtrl.Text = string.Join(";", items);
                    }
                };
                additionalButton = button;
            }
            valueControl.Location = new Point(baseValueLeft, baseHeight + baseValueHeightOffset);
            valueControl.Size = new Size(
                container.ClientSize.Width - baseValueLeft - baseValueRight,
                DpiHelper.ScaleValue(22, currentDpi));
            // add special buttons for certain ConfigTypes
            if (settingItem.ConfigType == CONFIG_TYPE_FILE ||
                settingItem.ConfigType == CONFIG_TYPE_DIRECTORY)
            {
                int additionalButtonWidth = 50;
                int scaledAdditionalButtonWidth = DpiHelper.ScaleValue(additionalButtonWidth, currentDpi);
                int additionalButtonHeight = DpiHelper.ScaleValue(28, currentDpi);
                int baseAdditionalButtonRight = baseValueRight;
                
                valueControl.Size = new Size(
                    container.ClientSize.Width - baseValueLeft - baseValueRight - scaledAdditionalButtonWidth,
                    DpiHelper.ScaleValue(22, currentDpi));
                Button button = new Button();
                button.Text = AppLocalization.Get("Common.Browse");
                button.Location = new Point(
                    container.ClientSize.Width - baseAdditionalButtonRight - scaledAdditionalButtonWidth,
                    baseHeight - DpiHelper.ScaleValue(3, currentDpi));
                button.FlatStyle = FlatStyle.System;
                button.Size = new Size(scaledAdditionalButtonWidth, additionalButtonHeight);
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
