using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace tbm_launcher
{
    public partial class Form1
    {
        private const int WmSysCommand = 0x0112;
        private const int MfByCommand = 0x0000;
        private const int MfSeparator = 0x0800;
        private const int MfString = 0x0000;
        private const int SettingsSystemMenuId = 0x1FF0;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitializeSystemMenu();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmSysCommand && (m.WParam.ToInt32() & 0xFFF0) == SettingsSystemMenuId)
            {
                button2_Click(this, EventArgs.Empty);
                return;
            }

            base.WndProc(ref m);
        }

        private void InitializeSystemMenu()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            IntPtr systemMenu = GetSystemMenu(Handle, false);
            if (systemMenu == IntPtr.Zero)
            {
                return;
            }

            if (GetMenuState(systemMenu, SettingsSystemMenuId, MfByCommand) != -1)
            {
                UpdateSystemMenuText();
                return;
            }

            AppendMenu(systemMenu, MfSeparator, 0, string.Empty);
            AppendMenu(systemMenu, MfString, SettingsSystemMenuId, GetSystemMenuSettingsText());
        }

        private void UpdateSystemMenuText()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            IntPtr systemMenu = GetSystemMenu(Handle, false);
            if (systemMenu == IntPtr.Zero || GetMenuState(systemMenu, SettingsSystemMenuId, MfByCommand) == -1)
            {
                return;
            }

            ModifyMenu(systemMenu, SettingsSystemMenuId, MfByCommand | MfString, SettingsSystemMenuId, GetSystemMenuSettingsText());
            DrawMenuBar(Handle);
        }

        private static string GetSystemMenuSettingsText()
        {
            switch (AppLocalization.CurrentLanguageCode)
            {
                case AppLanguageCodes.SimplifiedChinese:
                    return "\u8bbe\u7f6e(&S)...";
                case AppLanguageCodes.TraditionalChinese:
                    return "\u8a2d\u5b9a(&S)...";
                case AppLanguageCodes.Japanese:
                    return "\u8a2d\u5b9a(&S)...";
                default:
                    return "&Settings...";
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern int GetMenuState(IntPtr hMenu, int uId, int uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool ModifyMenu(IntPtr hMnu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern bool DrawMenuBar(IntPtr hWnd);
    }
}
