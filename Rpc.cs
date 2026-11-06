using System;
using System.Diagnostics;
using DiscordRPC;

namespace BlueArchiveJP_RPC
{
    class Rpc : IDisposable
    {
        const string ClientId = "1496482810007912479";

        DiscordRpcClient _client;
        bool _active;

        public bool Enabled { get; set; } = true;

        public void Tick(bool shouldBeOn)
        {
            if (!Enabled) shouldBeOn = false;

            if (_active && (_client == null || !_client.IsInitialized))
                Disconnect();

            if (shouldBeOn && !_active) Connect();
            else if (!shouldBeOn && _active) Disconnect();
        }

        void Connect()
        {
            if (Process.GetProcessesByName("Discord").Length == 0)
                return;

            for (int pipe = 0; pipe < 10; pipe++)
            {
                var c = new DiscordRpcClient(ClientId, pipe);
                try
                {
                    c.Initialize();
                    if (!c.IsInitialized)
                    {
                        c.Dispose();
                        continue;
                    }

                    c.SetPresence(new RichPresence
                    {
                        Timestamps = Timestamps.Now,
                        Assets = new Assets
                        {
                            LargeImageKey = "bluearchive",
                            LargeImageText = "ブルーアーカイブ",
                        }
                    });

                    _client = c;
                    _active = true;
                    return;
                }
                catch
                {
                    c.Dispose();
                }
            }
        }

        void Disconnect()
        {
            try { _client?.ClearPresence(); } catch { }
            try { _client?.Dispose(); } catch { }
            _client = null;
            _active = false;
        }

        public void Dispose() => Disconnect();
    }
}