using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace tbm_launcher
{
    public class FormStringArrayEditor : DpiAwareForm
    {
        private readonly CusomizedListBox listItems;
        private readonly Button buttonAdd;
        private readonly Button buttonRemove;
        private readonly Button buttonMoveUp;
        private readonly Button buttonMoveDown;
        private readonly Button buttonOk;
        private readonly Button buttonCancel;

        public FormStringArrayEditor()
        {
            listItems = new CusomizedListBox();
            buttonAdd = new Button();
            buttonRemove = new Button();
            buttonMoveUp = new Button();
            buttonMoveDown = new Button();
            buttonOk = new Button();
            buttonCancel = new Button();

            InitializeComponent();
            InitializeDpiAwareness();
            ApplyLocalizedText();
        }

        public void SetItems(IEnumerable<string> items)
        {
            listItems.Items.Clear();
            foreach (string item in items ?? Enumerable.Empty<string>())
            {
                listItems.Items.Add(item ?? string.Empty);
            }

            if (listItems.Items.Count > 0)
            {
                listItems.SelectedIndex = 0;
            }
        }

        public List<string> GetItems()
        {
            var items = new List<string>();
            foreach (object item in listItems.Items)
            {
                items.Add(item == null ? string.Empty : item.ToString());
            }
            return items;
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            listItems.DrawMode = DrawMode.OwnerDrawVariable;
            listItems.FormattingEnabled = true;
            listItems.EnableInlineStringEditing = true;
            listItems.ItemHeight = 24;
            listItems.Location = new Point(12, 12);
            listItems.Name = "listItems";
            listItems.Size = new Size(460, 292);
            listItems.TabIndex = 0;

            buttonAdd.FlatStyle = FlatStyle.System;
            buttonAdd.Location = new Point(478, 12);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(94, 28);
            buttonAdd.TabIndex = 1;
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;

            buttonRemove.FlatStyle = FlatStyle.System;
            buttonRemove.Location = new Point(478, 46);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(94, 28);
            buttonRemove.TabIndex = 2;
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += buttonRemove_Click;

            buttonMoveUp.FlatStyle = FlatStyle.System;
            buttonMoveUp.Location = new Point(478, 80);
            buttonMoveUp.Name = "buttonMoveUp";
            buttonMoveUp.Size = new Size(94, 28);
            buttonMoveUp.TabIndex = 3;
            buttonMoveUp.UseVisualStyleBackColor = true;
            buttonMoveUp.Click += buttonMoveUp_Click;

            buttonMoveDown.FlatStyle = FlatStyle.System;
            buttonMoveDown.Location = new Point(478, 114);
            buttonMoveDown.Name = "buttonMoveDown";
            buttonMoveDown.Size = new Size(94, 28);
            buttonMoveDown.TabIndex = 4;
            buttonMoveDown.UseVisualStyleBackColor = true;
            buttonMoveDown.Click += buttonMoveDown_Click;

            buttonOk.DialogResult = DialogResult.OK;
            buttonOk.FlatStyle = FlatStyle.System;
            buttonOk.Location = new Point(370, 314);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(98, 28);
            buttonOk.TabIndex = 5;
            buttonOk.UseVisualStyleBackColor = true;

            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.FlatStyle = FlatStyle.System;
            buttonCancel.Location = new Point(474, 314);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(98, 28);
            buttonCancel.TabIndex = 6;
            buttonCancel.UseVisualStyleBackColor = true;

            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = buttonCancel;
            ClientSize = new Size(584, 354);
            Font = new Font("Microsoft Yahei", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
            Controls.Add(buttonCancel);
            Controls.Add(buttonOk);
            Controls.Add(buttonMoveDown);
            Controls.Add(buttonMoveUp);
            Controls.Add(buttonRemove);
            Controls.Add(buttonAdd);
            Controls.Add(listItems);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormStringArrayEditor";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;

            ResumeLayout(false);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            int insertIndex = listItems.SelectedIndex >= 0 ? listItems.SelectedIndex + 1 : listItems.Items.Count;
            listItems.Items.Insert(insertIndex, string.Empty);
            listItems.SelectedIndex = insertIndex;
            listItems.BeginEditSelectedItem();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listItems.SelectedIndex < 0)
            {
                return;
            }

            int selectedIndex = listItems.SelectedIndex;
            listItems.Items.RemoveAt(selectedIndex);
            if (listItems.Items.Count > 0)
            {
                listItems.SelectedIndex = Math.Min(selectedIndex, listItems.Items.Count - 1);
            }
        }

        private void buttonMoveUp_Click(object sender, EventArgs e)
        {
            if (listItems.SelectedIndex <= 0)
            {
                return;
            }

            int selectedIndex = listItems.SelectedIndex;
            object temp = listItems.Items[selectedIndex];
            listItems.Items[selectedIndex] = listItems.Items[selectedIndex - 1];
            listItems.Items[selectedIndex - 1] = temp;
            listItems.SelectedIndex = selectedIndex - 1;
        }

        private void buttonMoveDown_Click(object sender, EventArgs e)
        {
            if (listItems.SelectedIndex < 0 || listItems.SelectedIndex >= listItems.Items.Count - 1)
            {
                return;
            }

            int selectedIndex = listItems.SelectedIndex;
            object temp = listItems.Items[selectedIndex];
            listItems.Items[selectedIndex] = listItems.Items[selectedIndex + 1];
            listItems.Items[selectedIndex + 1] = temp;
            listItems.SelectedIndex = selectedIndex + 1;
        }

        protected override void OnAfterDpiChanged(int oldDpi, int newDpi)
        {
            base.OnAfterDpiChanged(oldDpi, newDpi);
            listItems.ApplyDpiScaling(newDpi);
        }

        private void ApplyLocalizedText()
        {
            buttonAdd.Text = AppLocalization.Get("Common.Add");
            buttonRemove.Text = AppLocalization.Get("Common.Delete");
            buttonMoveUp.Text = AppLocalization.Get("Common.MoveUp");
            buttonMoveDown.Text = AppLocalization.Get("Common.MoveDown");
            buttonOk.Text = AppLocalization.Get("Common.Save");
            buttonCancel.Text = AppLocalization.Get("Common.Cancel");
            Text = AppLocalization.Get("StringArrayEditor.Title");
        }
    }
}
