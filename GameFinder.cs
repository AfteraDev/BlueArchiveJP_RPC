using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BlueArchiveJP_RPC
{
    static class GameFinder
    {
        static readonly string[] RequiredFiles =
        {
            "BlueArchive.exe",
            "xldr_BlueArchiveOnline_JP_loader_x64.exe",
            "GameAssembly.dll",
            "UnityPlayer.dll",
            "baselib.dll"
        };

        static string _cachedDir;

        public static string CachedDir => _cachedDir;

        public static string FindGameDir(bool refresh = false)
        {
            if (!refresh && _cachedDir != null && Directory.Exists(_cachedDir))
                return _cachedDir;

            _cachedDir = null;

            var yostar = FindYostar();
            if (yostar != null)
            {
                _cachedDir = yostar;
                return yostar;
            }

            return null;
        }

        static string FindYostar()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed || !drive.IsReady) continue;

                try
                {
                    var p = Path.Combine(drive.RootDirectory.FullName, @"YostarGames\BlueArchive_JP");
                    if (Directory.Exists(p)) return p;
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            return null;
        }

        public static string[] GetMissingFiles(string dir)
        {
            if (string.IsNullOrEmpty(dir)) return Array.Empty<string>();
            return RequiredFiles.Where(f => !File.Exists(Path.Combine(dir, f))).ToArray();
        }

        public static bool IsRunning(string expectedDir)
        {
            if (string.IsNullOrEmpty(expectedDir)) return false;

            string expected = expectedDir.TrimEnd(Path.DirectorySeparatorChar);

            try
            {
                var procs = Process.GetProcessesByName("BlueArchive");
                foreach (var p in procs)
                {
                    var file = GetProcessPath(p.Id);
                    if (file != null)
                    {
                        var procDir = Path.GetDirectoryName(file);
                        if (procDir != null)
                        {
                            procDir = procDir.TrimEnd(Path.DirectorySeparatorChar);
                            if (string.Equals(procDir, expected, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                    }
                }
            }
            catch { }
            
            return false;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        static string GetProcessPath(int pid)
        {
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
            if (hProcess == IntPtr.Zero) return null;

            try
            {
                int size = 1024;
                StringBuilder sb = new StringBuilder(size);
                if (QueryFullProcessImageName(hProcess, 0, sb, ref size))
                {
                    return sb.ToString();
                }
            }
            finally
            {
                CloseHandle(hProcess);
            }
            return null;
        }
    }
}