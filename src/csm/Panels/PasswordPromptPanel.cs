using ColossalFramework.PlatformServices;
using ColossalFramework.Threading;
using ColossalFramework.UI;
using CSM.API;
using CSM.API.Commands;
using CSM.Helpers;
using CSM.Networking;
using CSM.Networking.Config;
using UnityEngine;

namespace CSM.Panels
{
    /// <summary>
    ///     A focused password-entry prompt used when joining a password-protected
    ///     server from the public server browser. Deliberately doesn't expose the
    ///     target IP/port in the UI - unlike manually typing an address into
    ///     JoinGamePanel, a browsed server's address wasn't something the player
    ///     entered themselves, so there's no need to display it.
    /// </summary>
    public class PasswordPromptPanel : UIPanel
    {
        private UITextField _usernameField;
        private UITextField _passwordField;
        private UILabel _connectionStatus;
        private UIButton _connectButton;
        private UIButton _cancelButton;

        private string _targetIp;
        private int _targetPort;

        public override void Start()
        {
            AddUIComponent(typeof(UIDragHandle));

            backgroundSprite = "GenericPanel";
            color = new Color32(110, 110, 110, 255);

            width = 360;
            height = 335;
            relativePosition = PanelManager.GetCenterPosition(this);

            this.CreateTitleLabel("Password Required", new Vector2(80, -20));

            this.CreateLabel("This server requires a password to join.", new Vector2(10, -60), 340);

            this.CreateLabel("Username:", new Vector2(10, -95));
            _usernameField = this.CreateTextField("", new Vector2(10, -120));

            this.CreateLabel("Password:", new Vector2(10, -160));
            _passwordField = this.CreateTextField("", new Vector2(10, -185));
            _passwordField.isPasswordField = true;

            _connectionStatus = this.CreateLabel("", new Vector2(10, -225));
            _connectionStatus.textAlignment = UIHorizontalAlignment.Center;
            _connectionStatus.textColor = new Color32(255, 0, 0, 255);

            _connectButton = this.CreateButton("Connect", new Vector2(10, -265), 165);
            _connectButton.eventClick += OnConnectClick;

            _cancelButton = this.CreateButton("Cancel", new Vector2(185, -265), 165);
            _cancelButton.eventClick += (component, param) =>
            {
                isVisible = false;
            };
        }

        public override void Update()
        {
            if (isVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                isVisible = false;
            }
        }

        /// <summary>
        ///     Shows the prompt for the given target server. The IP/port are kept
        ///     only in memory here, never rendered in the UI.
        /// </summary>
        public void Show(string ip, int port)
        {
            _targetIp = ip;
            _targetPort = port;

            ClientConfig savedConfig = null;
            ConfigData.Load(ref savedConfig, ConfigData.ClientFile);

            string username = savedConfig?.Username;
            if (string.IsNullOrEmpty(username))
            {
                username = PlatformService.active && PlatformService.personaName != null
                    ? PlatformService.personaName
                    : "Player";
            }

            _usernameField.text = username;
            _passwordField.text = "";
            _connectionStatus.text = "";

            isVisible = true;
        }

        private void OnConnectClick(UIComponent component, UIMouseEventParameter param)
        {
            if (MultiplayerManager.Instance.CurrentRole == MultiplayerRole.Server)
            {
                _connectionStatus.text = "Already Running Server";
                return;
            }

            if (string.IsNullOrEmpty(_usernameField.text))
            {
                _connectionStatus.textColor = new Color32(255, 0, 0, 255);
                _connectionStatus.text = "Invalid Username";
                return;
            }

            ClientConfig clientConfig = new ClientConfig(_targetIp, _targetPort, _usernameField.text, _passwordField.text);

            _connectionStatus.textColor = new Color32(255, 255, 0, 255);
            _connectionStatus.text = "Connecting...";

            MultiplayerManager.Instance.CurrentClient.StartMainMenuEventProcessor();

            MultiplayerManager.Instance.ConnectToServer(clientConfig, success =>
            {
                ThreadHelper.dispatcher.Dispatch(() =>
                {
                    if (!success)
                    {
                        _connectionStatus.textColor = new Color32(255, 0, 0, 255);
                        _connectionStatus.text = MultiplayerManager.Instance.CurrentClient.ConnectionMessage;
                    }
                    else
                    {
                        _connectionStatus.text = "";
                        isVisible = false;
                        MultiplayerManager.Instance.BlockGameFirstJoin();
                    }
                });
            });
        }
    }
}
