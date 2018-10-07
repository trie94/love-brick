namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class CloudAnchorUIController : MonoBehaviour
    {
        /// <summary>
        /// A gameobject parenting UI for displaying feedback and errors.
        /// </summary>
        public Text SnackbarText;

        [SerializeField] GameObject roomInfo;
        [SerializeField] GameObject IPAdressInfo;


        [SerializeField] GameObject createRoomButton;
        [SerializeField] GameObject joinRoomButton;

        [SerializeField] GameObject startButton;
        [SerializeField] Button backButton;
        Color backButtonColor;

        // time
        [SerializeField] GameObject scoreBoard;
        [SerializeField] TextMeshProUGUI timer;
        [SerializeField] float totalTime;
        string min;
        string sec;

        // room info


        /// <summary>
        /// The resolve anchor mode button.
        /// </summary>
        public Button ResolveAnchorModeButton;

        /// <summary>
        /// The root for the input interface.
        /// </summary>
        public GameObject InputRoot;

        /// <summary>
        /// The input field for the room.
        /// </summary>
        public InputField RoomInputField;

        /// <summary>
        /// The input field for the ip address.
        /// </summary>
        public InputField IpAddressInputField;

        /// <summary>
        /// The field for toggling loopback (local) anchor resoltion.
        /// </summary>
        public Toggle ResolveOnDeviceToggle;

        [SerializeField] GameObject background;

        void Awake()
        {
            EventManager.StartListening("OnGameReady", OnGameReady);
            EventManager.StartListening("OnGameStart", OnGameStart);
            EventManager.StartListening("OnGameEnd", OnGameEnd);
        }

        public void Start()
        {
            IPAdressInfo.GetComponentInChildren<TextMeshProUGUI>().text = "My IP Address: " + _GetDeviceIpAddress();
            backButtonColor = backButton.GetComponent<Image>().color;
            DisableBackButton();
        }

        // ready mode
        public void ShowReadyMode()
        {
            if (!createRoomButton.activeSelf || !joinRoomButton.activeSelf)
            {
                createRoomButton.SetActive(true);
                joinRoomButton.SetActive(true);
            }
            startButton.SetActive(false);
            background.SetActive(true);
            scoreBoard.SetActive(false);

            SnackbarText.text = "Please create or join a room";
            InputRoot.SetActive(false);
        }

        /// <summary>
        /// Shows UI for the beginning phase of application "Hosting Mode".
        /// </summary>
        /// <param name="snackbarText">Optional text to put in the snackbar.</param>
        public void ShowHostingModeBegin(string snackbarText = null)
        {
            // remove buttons and background image
            createRoomButton.SetActive(false);
            joinRoomButton.SetActive(false);
            background.SetActive(false);
            EnableBackButton();

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

        /// <summary>
        /// Shows UI for the attempting to host phase of application "Hosting Mode".
        /// </summary>
        public void ShowHostingModeAttemptingHost()
        {
            backButton.interactable = false;
            SnackbarText.text = "Attempting to host...";
            InputRoot.SetActive(false);
        }

        /// <summary>
        /// Shows UI for the beginning phase of application "Resolving Mode".
        /// </summary>
        /// <param name="snackbarText">Optional text to put in the snackbar.</param>
        public void ShowResolvingModeBegin(string snackbarText = null)
        {
            backButton.GetComponentInChildren<Text>().text = "Host";
            backButton.interactable = false;
            ResolveAnchorModeButton.GetComponentInChildren<Text>().text = "Cancel";
            ResolveAnchorModeButton.interactable = true;

            if (string.IsNullOrEmpty(snackbarText))
            {
                SnackbarText.text = "Input Room and IP address to resolve anchor.";
            }
            else
            {
                SnackbarText.text = snackbarText;
            }

            InputRoot.SetActive(true);
        }

        /// <summary>
        /// Shows UI for the attempting to resolve phase of application "Resolving Mode".
        /// </summary>
        public void ShowResolvingModeAttemptingResolve()
        {
            backButton.GetComponentInChildren<Text>().text = "Host";
            backButton.interactable = false;
            ResolveAnchorModeButton.GetComponentInChildren<Text>().text = "Cancel";
            ResolveAnchorModeButton.interactable = false;
            SnackbarText.text = "Attempting to resolve anchor.";
            InputRoot.SetActive(false);
        }

        /// <summary>
        /// Shows UI for the successful resolve phase of application "Resolving Mode".
        /// </summary>
        public void ShowResolvingModeSuccess()
        {
            backButton.GetComponentInChildren<Text>().text = "Host";
            backButton.interactable = false;
            ResolveAnchorModeButton.GetComponentInChildren<Text>().text = "Cancel";
            ResolveAnchorModeButton.interactable = true;
            SnackbarText.text = "The anchor was successfully resolved.";
            InputRoot.SetActive(false);
        }

        /// <summary>
        /// Sets the room number in the UI.
        /// </summary>
        /// <param name="roomNumber">The room number to set.</param>
        public void SetRoomTextValue(int roomNumber)
        {
            roomInfo.GetComponentInChildren<TextMeshProUGUI>().text = "Room: " + roomNumber;
        }

        /// <summary>
        /// Gets the value of the resolve on device checkbox.
        /// </summary>
        /// <returns>The value of the resolve on device checkbox.</returns>
        public bool GetResolveOnDeviceValue()
        {
            return ResolveOnDeviceToggle.isOn;
        }

        /// <summary>
        /// Gets the value of the room number input field.
        /// </summary>
        /// <returns>The value of the room number input field.</returns>
        public int GetRoomInputValue()
        {
            int roomNumber;
            if (int.TryParse(RoomInputField.text, out roomNumber))
            {
                return roomNumber;
            }

            return 0;
        }

        /// <summary>
        /// Gets the value of the ip address input field.
        /// </summary>
        /// <returns>The value of the ip address input field.</returns>
        public string GetIpAddressInputValue()
        {
            return IpAddressInputField.text;
        }

        /// <summary>
        /// Handles a change to the "Resolve on Device" checkbox.
        /// </summary>
        /// <param name="isResolveOnDevice">If set to <c>true</c> resolve on device.</param>
        public void OnResolveOnDeviceValueChanged(bool isResolveOnDevice)
        {
            IpAddressInputField.interactable = !isResolveOnDevice;
        }

        /// <summary>
        /// Gets the device ip address.
        /// </summary>
        /// <returns>The device ip address.</returns>
        private string _GetDeviceIpAddress()
        {
            string ipAddress = "Unknown";
#if UNITY_2018_2_OR_NEWER
            string hostName = Dns.GetHostName();
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

        void OnGameReady(object data)
        {
            // enable start button
            startButton.SetActive(true);
        }

        void OnGameStart(object data)
        {
            // change ui
            Debug.Log("Game start! and start count down");
            StartCoroutine(ChangeToInGameUI());
            StartCoroutine(CountDown());
        }

        IEnumerator ChangeToInGameUI()
        {
            yield return new WaitForSeconds(0.2f);
            DisableBackButton();
            startButton.SetActive(false);
            roomInfo.SetActive(false);
            IPAdressInfo.SetActive(false);
            scoreBoard.SetActive(true);

            yield break;
        }

        void OnGameEnd(object data)
        {
            // display score and replay button
            scoreBoard.SetActive(false);
            // score active and replay active
            Debug.Log("display score and replay button");
        }

        IEnumerator CountDown()
        {
            while (totalTime > 0f)
            {
                totalTime--;
                min = Mathf.FloorToInt(totalTime / 60).ToString("00");
                sec = Mathf.RoundToInt(totalTime % 60).ToString("00");
                // Debug.Log(min +":" +sec);
                timer.text = (min + ":" + sec);
                yield return new WaitForSeconds(1f);

                if (totalTime <= 0f)
                {
                    // final
                    EventManager.TriggerEvent("OnGameEnd");
                    timer.text = "00:00";
                    yield break;
                }
            }
        }
    }
}
