namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using UnityEngine.Networking;
    using UnityEngine.Networking.Match;

    public class UIController : NetworkBehaviour
    {
        [SerializeField] GameObject snackbar;
        [SerializeField] Text SnackbarText;

        [SerializeField] GameObject roomInfo;
        [SerializeField] GameObject IPAdressInfo;


        [SerializeField] GameObject createRoomButton;
        [SerializeField] GameObject joinRoomButton;

        [SerializeField] GameObject startButton;
        [SerializeField] Button backButton;
        Color backButtonColor;

        // time
        [SerializeField] GameObject scoreBoard;
        public TextMeshProUGUI timer;

        // color ui
        [SerializeField] GameObject HostColor;
        [SerializeField] GameObject ClientColor;

        [SerializeField] GameObject InputRoot;

        [SerializeField] InputField RoomInputField;

        [SerializeField] InputField IpAddressInputField;

        [SerializeField] Toggle ResolveOnDeviceToggle;

        [SerializeField] GameObject background;

        static UIController s_instance;
        public static UIController Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindObjectOfType<UIController>();
                }
                return s_instance;
            }
        }

        void Awake()
        {
            if (s_instance != null)
            {
                Debug.Log("UI controller already exists");
            }
            s_instance = this;
            IPAdressInfo.GetComponentInChildren<TextMeshProUGUI>().text = "My IP Address: " + _GetDeviceIpAddress();
        }

        void Start()
        {
            // backButtonColor = backButton.GetComponent<Image>().color;
        }

        // Lobby mode
        public void ShowLobbyUI()
        {
            // ui
            startButton.SetActive(false);
            background.SetActive(true);
            createRoomButton.SetActive(true);
            joinRoomButton.SetActive(true);
            roomInfo.SetActive(false);
            IPAdressInfo.SetActive(false);
            scoreBoard.SetActive(false);
            snackbar.SetActive(true);
            DisableBackButton();

            SnackbarText.text = "Please create or join a room";
            InputRoot.SetActive(false);
        }

        public void ShowHostingModeBegin(string snackbarText = null)
        {
            // remove buttons and background image
            createRoomButton.SetActive(false);
            joinRoomButton.SetActive(false);
            background.SetActive(false);
            scoreBoard.SetActive(true);
            roomInfo.SetActive(true);
            IPAdressInfo.SetActive(true);
            // EnableBackButton();

            if (string.IsNullOrEmpty(snackbarText))
            {
                SnackbarText.text =
                    "The room code is now available. Please place a grid wall to host the game, press back to exit.";
            }
            else
            {
                SnackbarText.text = snackbarText;
            }

            InputRoot.SetActive(false);
        }

        public void ShowHostingModeAttemptingHost()
        {
            DisableBackButton();
            SnackbarText.text = "Attempting to host...";
            InputRoot.SetActive(false);
        }

        // join the game
        public void ShowResolvingModeBegin(string snackbarText = null)
        {
            // EnableBackButton();

            if (string.IsNullOrEmpty(snackbarText))
            {
                SnackbarText.text = "Input Room and IP address to join the game!";
            }
            else
            {
                SnackbarText.text = snackbarText;
            }

            createRoomButton.SetActive(false);
            joinRoomButton.SetActive(false);
            InputRoot.SetActive(true);
        }

        public void ShowResolvingModeAttemptingResolve()
        {
            DisableBackButton();
            SnackbarText.text = "Attempting to join...";
            InputRoot.SetActive(false);
        }

        public void ShowResolvingModeSuccess()
        {
            SnackbarText.text = "Successfully joined!";
            InputRoot.SetActive(false);
            StartGameUI();
        }

        public void SetRoomTextValue(int roomNumber)
        {
            roomInfo.GetComponentInChildren<TextMeshProUGUI>().text = "Room: " + roomNumber;
        }

        void GetRoomTextValue()
        {

        }

        public bool GetResolveOnDeviceValue()
        {
            return ResolveOnDeviceToggle.isOn;
        }

        public int GetRoomInputValue()
        {
            int roomNumber;
            if (int.TryParse(RoomInputField.text, out roomNumber))
            {
                return roomNumber;
            }

            return 0;
        }

        public string GetIpAddressInputValue()
        {
            return IpAddressInputField.text;
        }

        public void OnResolveOnDeviceValueChanged(bool isResolveOnDevice)
        {
            IpAddressInputField.interactable = !isResolveOnDevice;
        }

        private string _GetDeviceIpAddress()
        {
            string ipAddress = "Unknown";
#if UNITY_2018_2_OR_NEWER
            string hostName = Dns.GetHostName();
            // Debug.Log("hostname: " + hostName);
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);

            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = address.ToString();
                    break;
                }
            }
#else
            ipAddress = Network.player.ipAddress;
#endif
            return ipAddress;
        }

        void DisableBackButton()
        {
            backButton.interactable = false;
            backButton.GetComponentInChildren<Text>().text = "";
            backButton.GetComponent<Image>().color = Color.clear;
        }

        void EnableBackButton()
        {
            backButton.interactable = true;
            backButton.GetComponentInChildren<Text>().text = "BACK";
            backButton.GetComponent<Image>().color = backButtonColor;
        }

        public void ShowHostReadyUI()
        {
            // enable start button
            startButton.SetActive(true);
        }

        // where the actual game starts
        public void StartGameUI()
        {
            DisableBackButton();
            background.SetActive(false);
            startButton.SetActive(false);
            createRoomButton.SetActive(false);
            joinRoomButton.SetActive(false);
            roomInfo.SetActive(false);
            IPAdressInfo.SetActive(false);
            // snackbar.SetActive(false);
            scoreBoard.SetActive(true);
        }

        public void ShowEndUI()
        {
            // display score and replay button
            background.SetActive(false);
            // score active and replay active
            Debug.Log("display score and replay button");
        }

        public void GetHostColor()
        {
            Debug.Log("Get host(player1) color");
            ClientColor.SetActive(false);
            HostColor.SetActive(true);
        }

        public void GetClientColor()
        {
            Debug.Log("Get client(player2) color");
            HostColor.SetActive(false);
            ClientColor.SetActive(true);
        }

        public void SetSnackbarText(string text)
        {
            SnackbarText.text = text;
        }
    }
}