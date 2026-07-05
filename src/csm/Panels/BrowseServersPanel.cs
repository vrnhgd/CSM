using System;
using System.Collections.Generic;
using System.Threading;
using ColossalFramework.Threading;
using ColossalFramework.UI;
using CSM.API;
using CSM.Helpers;
using CSM.Networking;
using CSM.Util;
using UnityEngine;

namespace CSM.Panels
{
    public class BrowseServersPanel : UIPanel
    {
        private const int PollIntervalMs = 10000;

        private UIScrollablePanel _scrollablePanel;
        private UILabel _statusLabel;
        private UIButton _refreshButton;
        private UIButton _closeButton;

        private readonly List<UILabel> _rowLabels = new List<UILabel>();
        private readonly List<UIButton> _joinButtons = new List<UIButton>();

        private Thread _pollThread;
        private volatile bool _polling;

        public override void Start()
        {
            AddUIComponent(typeof(UIDragHandle));

            backgroundSprite = "GenericPanel";
            color = new Color32(110, 110, 110, 255);

            width = 450;
            height = 500;
            relativePosition = PanelManager.GetCenterPosition(this);

            this.CreateTitleLabel("Public Servers", new Vector2(140, -20));

            _statusLabel = this.CreateLabel("Loading...", new Vector2(10, -60));
            _statusLabel.textAlignment = UIHorizontalAlignment.Center;
            _statusLabel.width = 430;

            _scrollablePanel = AddUIComponent<UIScrollablePanel>();
            _scrollablePanel.width = 410;
            _scrollablePanel.height = 330;
            _scrollablePanel.position = new Vector2(10, -85);
            _scrollablePanel.clipChildren = true;
            _scrollablePanel.name = "BrowseServersScrollablePanel";

            this.AddScrollbar(_scrollablePanel);

            _refreshButton = this.CreateButton("Refresh", new Vector2(10, -430), 200);
            _refreshButton.eventClick += (component, param) => new Thread(FetchServerList) { IsBackground = true }.Start();

            _closeButton = this.CreateButton("Close", new Vector2(220, -430), 200);
            _closeButton.eventClick += (component, param) =>
            {
                isVisible = false;
            };

            eventVisibilityChanged += (component, visible) =>
            {
                if (visible)
                {
                    StartPolling();
                }
                else
                {
                    StopPolling();
                }
            };
        }

        public override void OnDestroy()
        {
            StopPolling();
            base.OnDestroy();
        }

        private void StartPolling()
        {
            if (_pollThread != null)
                return;

            _polling = true;
            _pollThread = new Thread(PollLoop) { IsBackground = true };
            _pollThread.Start();
        }

        private void StopPolling()
        {
            _polling = false;
            _pollThread = null;
        }

        private void PollLoop()
        {
            while (_polling)
            {
                FetchServerList();

                // Sleep in small increments so we react quickly when the panel is hidden.
                for (int i = 0; i < PollIntervalMs / 100 && _polling; i++)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void FetchServerList()
        {
            try
            {
                string url = $"http://{CSM.Settings.ApiServer}:{CSM.Settings.ApiServerHttpPort}/api/servers";
                Log.Info($"[BrowseServersPanel] Requesting public server list from: {url}");
                Log.Info($"[BrowseServersPanel] Fetch running on thread: {Thread.CurrentThread.ManagedThreadId}, IsBackground: {Thread.CurrentThread.IsBackground}");

                string json = new CSMWebClient().DownloadString(url);
                Log.Info($"[BrowseServersPanel] Raw response: {json}");

                // JsonUtility is a UnityEngine API and must only be called from the main
                // thread (unlike DownloadString above, which is fine off-thread) - calling
                // it here on a background thread silently fails to populate nested arrays.
                ThreadHelper.dispatcher.Dispatch(() =>
                {
                    Log.Info($"[BrowseServersPanel] Parsing on thread: {Thread.CurrentThread.ManagedThreadId}");

                    PublicServerListResponse response = JsonUtility.FromJson<PublicServerListResponse>(json);
                    Log.Info($"[BrowseServersPanel] response == null: {response == null}, response.Servers == null: {response?.Servers == null}");

                    PublicServerListing[] servers = response?.Servers ?? new PublicServerListing[0];
                    Log.Info($"[BrowseServersPanel] Parsed {servers.Length} server(s) from response.");

                    UpdateRows(servers);
                });
            }
            catch (Exception e)
            {
                Log.Warn($"Failed to fetch public server list: {e.Message}");
                ThreadHelper.dispatcher.Dispatch(() =>
                {
                    _statusLabel.text = "Failed to fetch the public server list.";
                    _statusLabel.isVisible = true;
                });
            }
        }

        private void UpdateRows(PublicServerListing[] servers)
        {
            foreach (UILabel label in _rowLabels)
            {
                Destroy(label);
            }

            foreach (UIButton button in _joinButtons)
            {
                Destroy(button);
            }

            _rowLabels.Clear();
            _joinButtons.Clear();

            if (servers.Length == 0)
            {
                _statusLabel.text = "No public servers found.";
                _statusLabel.isVisible = true;
                return;
            }

            _statusLabel.isVisible = false;

            int rowOffset = 0;
            const int rowHeight = 40;

            foreach (PublicServerListing server in servers)
            {
                string lockIcon = server.HasPassword ? " [Locked]" : "";
                string name = string.IsNullOrEmpty(server.Name) ? "Unnamed Server" : server.Name;
                string label = $"{name}{lockIcon}\n{server.CurrentPlayers}/{server.MaxPlayers} players";

                UILabel serverLabel = _scrollablePanel.CreateLabel(label, new Vector2(5, rowOffset), 260, rowHeight);
                serverLabel.textScale = 0.8f;
                serverLabel.wordWrap = true;
                _rowLabels.Add(serverLabel);

                UIButton joinButton = _scrollablePanel.CreateButton("Join", new Vector2(280, rowOffset), 100, rowHeight);
                string address = server.Address;
                joinButton.eventClick += (component, param) => OnJoinClick(address);
                _joinButtons.Add(joinButton);

                rowOffset -= rowHeight;
            }
        }

        private void OnJoinClick(string address)
        {
            string[] parts = address.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int port))
            {
                Log.Warn($"Received invalid server address from public server list: {address}");
                return;
            }

            isVisible = false;

            JoinGamePanel joinPanel = PanelManager.ShowPanel<JoinGamePanel>();
            joinPanel.PrefillJoinAddress(parts[0], port);
        }
    }
}
