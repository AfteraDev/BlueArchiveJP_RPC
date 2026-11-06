using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using Timer = System.Windows.Forms.Timer;

namespace BlueArchiveJP_RPC
{
    class TrayContext : ApplicationContext
    {
        const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        const string AppKey = "BlueArchiveJP_RPC";

        readonly Rpc _rpc = new Rpc();
        readonly Timer _timer;
        readonly NotifyIcon _tray;
        readonly ToolStripMenuItem _rpcItem;
        readonly ToolStripMenuItem _startupItem;

        public TrayContext()
        {
            _rpcItem = new ToolStripMenuItem("Discord RPC")
            {
                Checked = true,
                CheckOnClick = true
            };
            _rpcItem.CheckedChanged += (s, e) => _rpc.Enabled = _rpcItem.Checked;

            _startupItem = new ToolStripMenuItem("Autostart")
            {
                Checked = IsInStartup(),
                CheckOnClick = true
            };
            _startupItem.CheckedChanged += (s, e) => ToggleStartup(_startupItem.Checked);

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => ExitApp();

            var menu = new ContextMenuStrip();
            menu.Items.Add(_rpcItem);
            menu.Items.Add(_startupItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            _tray = new NotifyIcon
            {
                Icon = LoadIcon(),
                Text = "BlueArchive JP RPC",
                Visible = true,
                ContextMenuStrip = menu
            };

            _timer = new Timer { Interval = 2000 };
            _timer.Tick += (s, e) => CheckNow();
            _timer.Start();

            CheckNow();
        }

        void CheckNow()
        {
            var dir = GameFinder.FindGameDir();

            if (dir == null)
            {
                _tray.Text = "BlueArchive JP RPC — Yostar version not found";
                _rpc.Tick(false);
                return;
            }

            var missing = GameFinder.GetMissingFiles(dir);

            if (missing.Length > 0)
            {
                _tray.Text = "BlueArchive JP RPC — incomplete installation";
                _rpc.Tick(false);
                return;
            }

            bool running = GameFinder.IsRunning(dir);
            
            _tray.Text = running
                ? "BlueArchive JP RPC — in game"
                : "BlueArchive JP RPC — waiting";

            _rpc.Tick(running);
        }

        Icon LoadIcon()
        {
            try
            {
                var ico = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (ico != null) return ico;
            }
            catch { }
            return SystemIcons.Application;
        }

        void ExitApp()
        {
            _timer.Stop();
            _rpc.Dispose();
            _tray.Visible = false;
            _tray.Dispose();
            ExitThread();
        }

        bool IsInStartup()
        {
            using var k = Registry.CurrentUser.OpenSubKey(RunKey, false);
            return k?.GetValue(AppKey) != null;
        }

        void ToggleStartup(bool on)
        {
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(RunKey, true);
                if (k == null) return;

                if (on) k.SetValue(AppKey, $"\"{Application.ExecutablePath}\"");
                else k.DeleteValue(AppKey, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change autostart: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}