using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace tbm_launcher
{
    public class FormStringMapEditor : DpiAwareForm
    {
        private readonly DataGridView gridItems;
        private readonly Button buttonAdd;
        private readonly Button buttonRemove;
        private readonly Button buttonOk;
        private readonly Button buttonCancel;
        private Dictionary<string, string> items = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public FormStringMapEditor()
        {
            gridItems = new DataGridView();
            buttonAdd = new Button();
            buttonRemove = new Button();
            buttonOk = new Button();
            buttonCancel = new Button();

            InitializeComponent();
            InitializeDpiAwareness();
        }

        public void SetItems(IDictionary<string, string> values)
        {
            gridItems.Rows.Clear();
            foreach (KeyValuePair<string, string> item in values ?? new Dictionary<string, string>())
            {
                gridItems.Rows.Add(item.Key ?? string.Empty, item.Value ?? string.Empty);
            }
        }

        public Dictionary<string, string> GetItems()
        {
            return new Dictionary<string, string>(items, StringComparer.OrdinalIgnoreCase);
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            gridItems.AllowUserToAddRows = false;
            gridItems.AllowUserToDeleteRows = false;
            gridItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridItems.BackgroundColor = Color.White;
            gridItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = AppLocalization.Get("StringMapEditor.Key"), Name = "columnKey", FillWeight = 35F });
            gridItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = AppLocalization.Get("StringMapEditor.Value"), Name = "columnValue", FillWeight = 65F });
            gridItems.EditMode = DataGridViewEditMode.EditOnEnter;
            gridItems.Location = new Point(12, 12);
            gridItems.MultiSelect = false;
            gridItems.Name = "gridItems";
            gridItems.RowHeadersVisible = false;
            gridItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridItems.Size = new Size(560, 292);
            gridItems.TabIndex = 0;

            buttonAdd.FlatStyle = FlatStyle.System;
            buttonAdd.Location = new Point(578, 12);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(94, 28);
            buttonAdd.TabIndex = 1;
            buttonAdd.Text = AppLocalization.Get("Common.Add");
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;

            buttonRemove.FlatStyle = FlatStyle.System;
            buttonRemove.Location = new Point(578, 46);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(94, 28);
            buttonRemove.TabIndex = 2;
            buttonRemove.Text = AppLocalization.Get("Common.Delete");
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += buttonRemove_Click;

            buttonOk.FlatStyle = FlatStyle.System;
            buttonOk.Location = new Point(470, 314);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(98, 28);
            buttonOk.TabIndex = 3;
            buttonOk.Text = AppLocalization.Get("Common.Save");
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;

            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.FlatStyle = FlatStyle.System;
            buttonCancel.Location = new Point(574, 314);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(98, 28);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = AppLocalization.Get("Common.Cancel");
            buttonCancel.UseVisualStyleBackColor = true;

            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = buttonCancel;
            ClientSize = new Size(684, 354);
            Font = new Font("Microsoft Yahei", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
            Controls.Add(buttonCancel);
            Controls.Add(buttonOk);
            Controls.Add(buttonRemove);
            Controls.Add(buttonAdd);
            Controls.Add(gridItems);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormStringMapEditor";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Edit Variables";

            ResumeLayout(false);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            int rowIndex = gridItems.Rows.Add(string.Empty, string.Empty);
            gridItems.ClearSelection();
            gridItems.Rows[rowIndex].Selected = true;
            gridItems.CurrentCell = gridItems.Rows[rowIndex].Cells[0];
            gridItems.BeginEdit(true);
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (gridItems.CurrentRow != null)
            {
                gridItems.Rows.Remove(gridItems.CurrentRow);
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            gridItems.EndEdit();
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataGridViewRow row in gridItems.Rows)
            {
                string key = Convert.ToString(row.Cells[0].Value) ?? string.Empty;
                string value = Convert.ToString(row.Cells[1].Value) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(key))
                {
                    MessageBox.Show("A variable key is required.", AppLocalization.Get("Common.ErrorTitle"));
                    return;
                }

                if (result.ContainsKey(key))
                {
                    MessageBox.Show("Variable keys must be unique.", AppLocalization.Get("Common.ErrorTitle"));
                    return;
                }

                result.Add(key, value);
            }

            items = result;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
