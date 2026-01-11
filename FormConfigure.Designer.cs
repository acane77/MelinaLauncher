
namespace tbm_launcher
{
    partial class FormConfigure
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.listConfig = new tbm_launcher.CusomizedListBox();
            this.btn_launch = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.panel_config = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btn_moveup = new System.Windows.Forms.Button();
            this.btn_movedown = new System.Windows.Forms.Button();
            this.move_copy = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(17, 40);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "配置项";
            // 
            // listConfig
            // 
            this.listConfig.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listConfig.FormattingEnabled = true;
            this.listConfig.HoverIndex = -1;
            this.listConfig.ItemHeight = 24;
            this.listConfig.Location = new System.Drawing.Point(16, 59);
            this.listConfig.Name = "listConfig";
            this.listConfig.Size = new System.Drawing.Size(196, 364);
            this.listConfig.TabIndex = 1;
            this.listConfig.SelectedIndexChanged += new System.EventHandler(this.listConfig_SelectedIndexChanged);
            // 
            // btn_launch
            // 
            this.btn_launch.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_launch.Location = new System.Drawing.Point(703, 438);
            this.btn_launch.Name = "btn_launch";
            this.btn_launch.Size = new System.Drawing.Size(103, 28);
            this.btn_launch.TabIndex = 2;
            this.btn_launch.Text = "取消";
            this.btn_launch.UseVisualStyleBackColor = true;
            this.btn_launch.Click += new System.EventHandler(this.btn_launch_Click);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Location = new System.Drawing.Point(594, 438);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 28);
            this.button1.TabIndex = 3;
            this.button1.Text = "保存";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(68, 10);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(738, 39);
            this.textBox1.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label3.Location = new System.Drawing.Point(17, 13);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 31);
            this.label3.TabIndex = 9;
            this.label3.Text = "标题";
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button4.Location = new System.Drawing.Point(219, 445);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(94, 19);
            this.button4.TabIndex = 10;
            this.button4.Text = "预览";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // panel_config
            // 
            this.panel_config.Location = new System.Drawing.Point(218, 49);
            this.panel_config.Name = "panel_config";
            this.panel_config.Size = new System.Drawing.Size(594, 381);
            this.panel_config.TabIndex = 11;
            this.panel_config.TabStop = false;
            this.panel_config.Text = "配置";
            // 
            // button3
            // 
            this.button3.BackgroundImage = global::tbm_launcher.Properties.Resources.del;
            this.button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button3.FlatAppearance.BorderSize = 0;
            this.button3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Location = new System.Drawing.Point(46, 426);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(22, 22);
            this.button3.TabIndex = 5;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.BackgroundImage = global::tbm_launcher.Properties.Resources.add2;
            this.button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(18, 426);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(22, 22);
            this.button2.TabIndex = 4;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btn_moveup
            // 
            this.btn_moveup.BackgroundImage = global::tbm_launcher.Properties.Resources.moveup;
            this.btn_moveup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btn_moveup.FlatAppearance.BorderSize = 0;
            this.btn_moveup.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btn_moveup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_moveup.Location = new System.Drawing.Point(74, 426);
            this.btn_moveup.Name = "btn_moveup";
            this.btn_moveup.Size = new System.Drawing.Size(22, 22);
            this.btn_moveup.TabIndex = 12;
            this.btn_moveup.UseVisualStyleBackColor = true;
            this.btn_moveup.Click += new System.EventHandler(this.btn_moveup_Click);
            // 
            // btn_movedown
            // 
            this.btn_movedown.BackgroundImage = global::tbm_launcher.Properties.Resources.movedown;
            this.btn_movedown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btn_movedown.FlatAppearance.BorderSize = 0;
            this.btn_movedown.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btn_movedown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_movedown.Location = new System.Drawing.Point(102, 426);
            this.btn_movedown.Name = "btn_movedown";
            this.btn_movedown.Size = new System.Drawing.Size(22, 22);
            this.btn_movedown.TabIndex = 13;
            this.btn_movedown.UseVisualStyleBackColor = true;
            this.btn_movedown.Click += new System.EventHandler(this.btn_movedown_Click);
            // 
            // move_copy
            // 
            this.move_copy.BackgroundImage = global::tbm_launcher.Properties.Resources.copy;
            this.move_copy.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.move_copy.FlatAppearance.BorderSize = 0;
            this.move_copy.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.move_copy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.move_copy.Location = new System.Drawing.Point(130, 426);
            this.move_copy.Name = "move_copy";
            this.move_copy.Size = new System.Drawing.Size(22, 22);
            this.move_copy.TabIndex = 14;
            this.move_copy.UseVisualStyleBackColor = true;
            this.move_copy.Click += new System.EventHandler(this.move_copy_Click);
            // 
            // FormConfigure
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(824, 476);
            this.Controls.Add(this.move_copy);
            this.Controls.Add(this.btn_movedown);
            this.Controls.Add(this.btn_moveup);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel_config);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btn_launch);
            this.Controls.Add(this.listConfig);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConfigure";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Melina Launcher - Configure";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormConfigure_FormClosed);
            this.Load += new System.EventHandler(this.FormConfigure_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_launch;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.GroupBox panel_config;
        private CusomizedListBox listConfig;
        private System.Windows.Forms.Button btn_moveup;
        private System.Windows.Forms.Button btn_movedown;
        private System.Windows.Forms.Button move_copy;
    }
}