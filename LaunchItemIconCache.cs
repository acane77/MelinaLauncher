using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace tbm_launcher
{
    internal static class LaunchItemIconCache
    {
        private const int IconPixelSize = 32;

        private static readonly object Gate = new object();
        private static readonly Dictionary<string, Bitmap> ExtractedIcons = new Dictionary<string, Bitmap>(StringComparer.OrdinalIgnoreCase);

        private static Bitmap folderIcon;
        private static Bitmap defaultExeIcon;
        private static Bitmap scriptIcon;
        private static Bitmap genericIcon;
        private static Bitmap missingIcon;
        private static Bitmap selectIcon;
        private static Bitmap urlIcon;
        private static Bitmap watchIcon;

        public static Bitmap Resolve(string command, string workingDirectory, string executableName, int port)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                if (!string.IsNullOrWhiteSpace(executableName) || port > 0)
                {
                    return GetWatchIcon();
                }

                return null;
            }

            string[] args = GetArguments(command);
            string argv0 = args.Length > 0 ? args[0] : null;
            if (string.IsNullOrWhiteSpace(argv0))
            {
                return null;
            }

            bool hasWorkingDirectory = !string.IsNullOrWhiteSpace(workingDirectory);
            string exePath = Locate(argv0, workingDirectory, hasWorkingDirectory);
            if (string.IsNullOrEmpty(exePath))
            {
                return hasWorkingDirectory ? GetMissingIcon() : GetDefaultExeIcon();
            }

            Bitmap siblingIcon = TryLoadSiblingIcon(exePath);
            if (siblingIcon != null)
            {
                return siblingIcon;
            }

            if (IsExplorerExecutable(exePath))
            {
                return GetExplorerIcon(args, workingDirectory);
            }

            return GetIconForPath(exePath);
        }

        private static string Locate(string argv0, string workingDirectory, bool hasWorkingDirectory)
        {
            try
            {
                if (hasWorkingDirectory)
                {
                    string workDir = workingDirectory.Trim();
                    if (!Path.IsPathRooted(workDir))
                    {
                        workDir = Path.GetFullPath(Path.Combine(Application.StartupPath, workDir));
                    }

                    string workDirCandidate = Probe(workDir, argv0);
                    if (!string.IsNullOrEmpty(workDirCandidate))
                    {
                        return workDirCandidate;
                    }

                    if (HasDirectory(argv0))
                    {
                        return null;
                    }

                    return SearchPath(argv0);
                }

                string startupCandidate = Probe(Application.StartupPath, argv0);
                if (!string.IsNullOrEmpty(startupCandidate))
                {
                    return startupCandidate;
                }

                if (HasDirectory(argv0))
                {
                    return null;
                }

                return SearchPath(argv0);
            }
            catch
            {
                return null;
            }
        }

        private static string SearchPath(string fileName)
        {
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
            {
                return null;
            }

            string[] directories = pathEnv.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string directory in directories)
            {
                string trimmed = directory.Trim().Trim('"');
                if (trimmed.Length == 0)
                {
                    continue;
                }

                string found = Probe(trimmed, fileName);
                if (!string.IsNullOrEmpty(found))
                {
                    return found;
                }
            }

            return null;
        }

        private static string Probe(string directory, string fileName)
        {
            string exact;
            try
            {
                if (Path.IsPathRooted(fileName))
                {
                    exact = Path.GetFullPath(fileName);
                }
                else
                {
                    exact = Path.GetFullPath(Path.Combine(directory, fileName));
                }
            }
            catch
            {
                return null;
            }

            if (!Path.HasExtension(fileName))
            {
                foreach (string extension in GetSearchExtensions(fileName))
                {
                    if (Exists(exact + extension))
                    {
                        return exact + extension;
                    }
                }
            }

            return Exists(exact) ? exact : null;
        }

        private static string[] GetSearchExtensions(string fileName)
        {
            if (Path.HasExtension(fileName))
            {
                return new[] { string.Empty };
            }

            string pathext = Environment.GetEnvironmentVariable("PATHEXT");
            if (string.IsNullOrWhiteSpace(pathext))
            {
                pathext = ".COM;.EXE;.BAT;.CMD";
            }

            string[] parts = pathext.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var extensions = new List<string>();
            foreach (string part in parts)
            {
                string extension = part.Trim();
                if (extension.Length == 0)
                {
                    continue;
                }

                if (extension[0] != '.')
                {
                    extension = "." + extension;
                }

                extensions.Add(extension);
            }

            if (extensions.Count == 0)
            {
                return new[] { ".EXE" };
            }

            return extensions.ToArray();
        }

        private static bool HasDirectory(string argv0)
        {
            return argv0.IndexOf('\\') >= 0
                || argv0.IndexOf('/') >= 0
                || Path.IsPathRooted(argv0);
        }

        private static bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        private static Bitmap TryLoadSiblingIcon(string exePath)
        {
            string icoPath;
            try
            {
                icoPath = Path.ChangeExtension(exePath, ".ico");
                if (string.IsNullOrEmpty(icoPath) || !File.Exists(icoPath))
                {
                    return null;
                }

                icoPath = Path.GetFullPath(icoPath);
            }
            catch
            {
                return null;
            }

            lock (Gate)
            {
                Bitmap cached;
                if (ExtractedIcons.TryGetValue(icoPath, out cached))
                {
                    return cached;
                }

                Bitmap loaded = LoadIconFile(icoPath);
                if (loaded == null)
                {
                    return null;
                }

                ExtractedIcons[icoPath] = loaded;
                return loaded;
            }
        }

        private static Bitmap LoadIconFile(string icoPath)
        {
            try
            {
                using (Icon icon = new Icon(icoPath, IconPixelSize, IconPixelSize))
                {
                    return ToBitmap(icon);
                }
            }
            catch
            {
                return null;
            }
        }

        private static bool IsExplorerExecutable(string exePath)
        {
            string fileName = Path.GetFileName(exePath);
            return fileName.Equals("explorer.exe", StringComparison.OrdinalIgnoreCase);
        }

        private static Bitmap GetExplorerIcon(string[] args, string workingDirectory)
        {
            if (HasSelectSwitch(args))
            {
                return GetSelectIcon();
            }

            string target = args.Length > 1 ? args[1] : null;
            if (!string.IsNullOrWhiteSpace(target))
            {
                if (IsUrl(target))
                {
                    return GetUrlIcon();
                }

                if (IsDirectoryTarget(target, workingDirectory))
                {
                    return GetFolderIcon();
                }
            }

            return GetGenericIcon();
        }

        private static bool HasSelectSwitch(string[] args)
        {
            for (int index = 1; index < args.Length; index++)
            {
                string arg = args[index];
                if (!string.IsNullOrEmpty(arg)
                    && arg.IndexOf("/select", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsUrl(string value)
        {
            return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDirectoryTarget(string target, string workingDirectory)
        {
            try
            {
                string path = target;
                if (!Path.IsPathRooted(path))
                {
                    string baseDir = Application.StartupPath;
                    if (!string.IsNullOrWhiteSpace(workingDirectory))
                    {
                        baseDir = workingDirectory.Trim();
                        if (!Path.IsPathRooted(baseDir))
                        {
                            baseDir = Path.Combine(Application.StartupPath, baseDir);
                        }
                    }

                    path = Path.Combine(baseDir, path);
                }

                return Directory.Exists(Path.GetFullPath(path));
            }
            catch
            {
                return false;
            }
        }

        private static Bitmap GetIconForPath(string exePath)
        {
            try
            {
                if (Directory.Exists(exePath))
                {
                    return GetFolderIcon();
                }
            }
            catch
            {
                return GetGenericIcon();
            }

            string extension = Path.GetExtension(exePath);
            if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return GetExecutableIcon(exePath);
            }

            if (extension.Equals(".bat", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".sh", StringComparison.OrdinalIgnoreCase))
            {
                return GetScriptIcon();
            }

            return GetGenericIcon();
        }

        private static Bitmap GetExecutableIcon(string exePath)
        {
            string key = exePath;
            try
            {
                key = Path.GetFullPath(exePath);
            }
            catch
            {
            }

            lock (Gate)
            {
                Bitmap cached;
                if (ExtractedIcons.TryGetValue(key, out cached))
                {
                    return cached;
                }

                Bitmap extracted = ExtractIconBitmap(key);
                if (extracted == null)
                {
                    extracted = GetDefaultExeIcon();
                }

                ExtractedIcons[key] = extracted;
                return extracted;
            }
        }

        private static Bitmap ExtractIconBitmap(string path)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = ExtractIcon(IntPtr.Zero, path, 0);
                if (handle == IntPtr.Zero || handle == new IntPtr(1))
                {
                    return null;
                }

                Icon icon = Icon.FromHandle(handle);
                return ToBitmap(icon);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (handle != IntPtr.Zero && handle != new IntPtr(1))
                {
                    DestroyIcon(handle);
                }
            }
        }

        private static Bitmap GetFolderIcon()
        {
            return GetBuiltinIcon(ref folderIcon, Properties.Resources.Icon1498);
        }

        private static Bitmap GetDefaultExeIcon()
        {
            return GetBuiltinIcon(ref defaultExeIcon, Properties.Resources.Icon87);
        }

        private static Bitmap GetScriptIcon()
        {
            return GetBuiltinIcon(ref scriptIcon, Properties.Resources.Icon641);
        }

        private static Bitmap GetGenericIcon()
        {
            return GetBuiltinIcon(ref genericIcon, Properties.Resources.Icon633);
        }

        private static Bitmap GetMissingIcon()
        {
            return GetBuiltinIcon(ref missingIcon, Properties.Resources.Icon1112);
        }

        private static Bitmap GetSelectIcon()
        {
            return GetBuiltinIcon(ref selectIcon, Properties.Resources.Icon71);
        }

        private static Bitmap GetUrlIcon()
        {
            return GetBuiltinIcon(ref urlIcon, Properties.Resources.Icon175);
        }

        private static Bitmap GetWatchIcon()
        {
            return GetBuiltinIcon(ref watchIcon, Properties.Resources.Icon625);
        }

        private static Bitmap GetBuiltinIcon(ref Bitmap cache, Icon source)
        {
            lock (Gate)
            {
                if (cache == null)
                {
                    cache = LoadResourceIcon(source);
                }

                return cache;
            }
        }

        private static Bitmap LoadResourceIcon(Icon source)
        {
            if (source == null)
            {
                return new Bitmap(IconPixelSize, IconPixelSize, PixelFormat.Format32bppArgb);
            }

            try
            {
                using (Icon sized = new Icon(source, IconPixelSize, IconPixelSize))
                {
                    return ToBitmap(sized);
                }
            }
            catch
            {
                return ToBitmap(source);
            }
        }

        private static Bitmap ToBitmap(Icon icon)
        {
            int width = icon.Width > 0 ? icon.Width : IconPixelSize;
            int height = icon.Height > 0 ? icon.Height : IconPixelSize;
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.DrawIcon(icon, new Rectangle(0, 0, width, height));
            }

            return bitmap;
        }

        private static string[] GetArguments(string command)
        {
            int argc;
            IntPtr argv = CommandLineToArgvW(command, out argc);
            if (argv == IntPtr.Zero || argc < 1)
            {
                return new string[0];
            }

            try
            {
                string[] args = new string[argc];
                for (int index = 0; index < argc; index++)
                {
                    IntPtr item = Marshal.ReadIntPtr(argv, index * IntPtr.Size);
                    args[index] = Marshal.PtrToStringUni(item) ?? string.Empty;
                }

                return args;
            }
            finally
            {
                LocalFree(argv);
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW(string lpCmdLine, out int pNumArgs);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}
