using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tbm_launcher
{
    class CusomizedListBoxItemCollection : IList, ICollection, IEnumerable
    {
        private CusomizedListBox m_onwer;

        public CusomizedListBoxItemCollection(CusomizedListBox onwer)
        {
            this.m_onwer = onwer;
        }

        internal CusomizedListBox Owner
        {
            get { return this.m_onwer; }
        }


        public object this[int index] { 
            get => Owner.OldItemSource[index]; 
            set => Owner.OldItemSource[index] = value; 
        }

        public int Count => Owner.OldItemSource.Count;

        public object SyncRoot => Owner.OldItemSource.IsReadOnly;

        public bool IsSynchronized => false;

        public bool IsReadOnly => Owner.OldItemSource.IsReadOnly;

        public bool IsFixedSize => false;

        object IList.this[int index] { 
            get => this[index]; 
            set => this[index] = value; 
        }

        public int Add(object value)
        {
            return Owner.OldItemSource.Add(value);
        }

        public void Clear()
        {
            m_onwer.OldItemSource.Clear();
        }

        public bool Contains(object value)
        {
            return m_onwer.OldItemSource.Contains(value);
        }

        public void CopyTo(object[] array, int index)
        {
            m_onwer.OldItemSource.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return Owner.OldItemSource.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return Owner.OldItemSource.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            Owner.OldItemSource.Insert(index, value);
        }

        public void Remove(object value)
        {
            Owner.OldItemSource.Remove(value);
        }

        public void RemoveAt(int index)
        {
            Owner.OldItemSource.RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo(array.OfType<object>().ToArray<object>(), index);
        }
    }

    internal class CusomizedListBox : ListBox
    {
        public object mouseItem;
        private CusomizedListBoxItemCollection m_Items;
        private readonly TextBox inlineEditor;
        private int editingIndex = -1;

        public Color _MoveItemBackColor = Color.FromArgb(255, 229, 243, 255);
        public Color _SelectedItemBackColor = Color.FromArgb(255, 204, 232, 255);
        public Color _SelectedForeColor = Color.White;
        public Color _SelectedItemUnfocusedBackColor = Color.FromArgb(255, 217, 217, 217);

        public int HoverIndex { get; set; }
        public bool EnableInlineStringEditing { get; set; }

        public CusomizedListBox() : base()
        {

            m_Items = new CusomizedListBoxItemCollection(this);

            base.DrawMode = DrawMode.OwnerDrawVariable;
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // 双缓冲   
            this.SetStyle(ControlStyles.ResizeRedraw, true); // 调整大小时重绘
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景. 
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true); // 开启控件透明

            HoverIndex = -1;
            EnableInlineStringEditing = false;

            inlineEditor = new TextBox();
            inlineEditor.Visible = false;
            inlineEditor.BorderStyle = BorderStyle.FixedSingle;
            inlineEditor.KeyDown += InlineEditor_KeyDown;
            inlineEditor.LostFocus += InlineEditor_LostFocus;
            Controls.Add(inlineEditor);
        }

        public new CusomizedListBoxItemCollection Items => m_Items;

        internal ListBox.ObjectCollection OldItemSource => base.Items;

        private bool CanSafelyUseHandle()
        {
            return !IsDisposed && !Disposing && IsHandleCreated;
        }

        private bool TryGetItemBounds(int index, out Rectangle bounds)
        {
            bounds = Rectangle.Empty;
            if (index < 0 || index >= Items.Count || !CanSafelyUseHandle())
            {
                return false;
            }

            try
            {
                bounds = base.GetItemRectangle(index);
                return true;
            }
            catch (Win32Exception)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!CanSafelyUseHandle())
            {
                return;
            }

            for (int i = 0; i < Items.Count; i++)
            {
                Rectangle bounds;
                if (!TryGetItemBounds(i, out bounds))
                {
                    continue;
                }
                Rectangle deleteBounds = new Rectangle(bounds.Width - 28, (bounds.Height - 16) / 2 + bounds.Top, 16, 16);

                if (bounds.Contains(e.X, e.Y))
                {
                    HoverIndex = i;

                    if (Items[i] != mouseItem)
                    {
                        mouseItem = Items[i];
                    }

                    if (deleteBounds.Contains(e.X, e.Y))
                    {
                        //mouseItem.IsFocus = true;
                        //this.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        //mouseItem.IsFocus = false;
                        //this.Cursor = Cursors.Arrow;
                    }

                    this.Invalidate();
                    break;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!CanSafelyUseHandle())
            {
                return;
            }

            Graphics g = e.Graphics;

            // you can set SeletedItem background
            if (this.Focused && this.SelectedItem != null)
            {

            }

            for (int i = 0; i < Items.Count; i++)
            {
                Rectangle bounds;
                if (!TryGetItemBounds(i, out bounds))
                {
                    break;
                }

                if (this.SelectedItem == Items[i])
                {
                    Color bgColor = Focused || mouseItem == Items[i] ? _SelectedItemBackColor : _SelectedItemUnfocusedBackColor;

                    using (SolidBrush brush = new SolidBrush(bgColor))
                    {
                        g.FillRectangle(brush, new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height));
                    }

                    Color borderColor = Color.FromArgb(255, 0, 120, 212);
                    float borderWidth = 1; // 设置边框宽度

                    if (Focused || mouseItem == Items[i])
                    {
                        using (Pen pen = new Pen(borderColor, borderWidth))
                        {
                            // 绘制边界
                            g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 2);
                        }
                    }

                    //description += " (Process:" + Items[i].PID.ToString() + " / Window: " + Handle.ToString() + ")";
                }
                else if (mouseItem == Items[i])
                {
                    Color bgColor = _MoveItemBackColor;

                    using (SolidBrush brush = new SolidBrush(bgColor))
                    {
                        g.FillRectangle(brush, new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height));
                    }
                }

                int fontLeft = bounds.Left + 60 - 57;
                System.Drawing.Font font = new System.Drawing.Font("微软雅黑", 9);
                // System.Drawing.Font fontDesc = new System.Drawing.Font("微软雅黑", 9);

                g.DrawString(Items[i].ToString(), font, new SolidBrush(this.ForeColor), fontLeft, bounds.Top + 3);

                //if (Items[i].Icon != null)
                //{
                //    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                //    g.DrawImage(Items[i].Icon, new Rectangle(bounds.X + 10, (bounds.Height - 40) / 2 + bounds.Top, 40, 40));

                //                }
                //g.DrawImage(Properties.Resources.error, new Rectangle(bounds.Width - 28, (bounds.Height - 16) / 2 + bounds.Top, 16, 16));
            }

            base.OnPaint(e);
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            base.OnMeasureItem(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (!EnableInlineStringEditing)
            {
                return;
            }

            int index = IndexFromPoint(e.Location);
            if (index < 0 || index >= Items.Count)
            {
                return;
            }

            BeginEdit(index);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.mouseItem = null;
            this.Invalidate();
        }

        public void BeginEditSelectedItem()
        {
            if (SelectedIndex >= 0)
            {
                BeginEdit(SelectedIndex);
            }
        }

        public void BeginEdit(int index)
        {
            if (!EnableInlineStringEditing || index < 0 || index >= Items.Count || !CanSafelyUseHandle())
            {
                return;
            }

            object item = Items[index];
            if (!(item is string))
            {
                return;
            }

            editingIndex = index;
            SelectedIndex = index;

            Rectangle bounds;
            if (!TryGetItemBounds(index, out bounds))
            {
                return;
            }
            inlineEditor.Bounds = new Rectangle(bounds.Left + 1, bounds.Top + 1, Math.Max(20, bounds.Width - 2), Math.Max(20, bounds.Height - 2));
            inlineEditor.Text = item.ToString();
            inlineEditor.Visible = true;
            inlineEditor.BringToFront();
            inlineEditor.Focus();
            inlineEditor.SelectAll();
        }

        private void EndEdit(bool commit)
        {
            if (editingIndex < 0)
            {
                inlineEditor.Visible = false;
                return;
            }

            if (commit && editingIndex < Items.Count && Items[editingIndex] is string)
            {
                Items[editingIndex] = inlineEditor.Text;
            }

            editingIndex = -1;
            inlineEditor.Visible = false;
            Focus();
            Invalidate();
        }

        private void InlineEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                EndEdit(true);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                EndEdit(false);
            }
        }

        private void InlineEditor_LostFocus(object sender, EventArgs e)
        {
            EndEdit(true);
        }
    }
}
