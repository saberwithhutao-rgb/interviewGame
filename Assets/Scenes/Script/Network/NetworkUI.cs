using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField steamIdInputField;  // 改成输入 Steam ID
    [SerializeField] private TMP_InputField PlayerNumInputField;  // 改成输入服务器人数
    [SerializeField] private TMP_Dropdown DropdownType; // 改成选择大厅类型
    public async void StartAsHost()
    {
        if (!SteamClient.IsValid)
        {
            SteamClient.Init(480);
        }

        int typeIndex = DropdownType.value;
        ELobbyType lobbyType = (ELobbyType)(typeIndex + 1);

        if (int.TryParse(PlayerNumInputField.text, out int maxPlayers) && maxPlayers > 0 && maxPlayers <= 250)
        {
            GameSessionData.NetworkRole = "Host";
            await SteamManager.Instance.CreatLobbyAsync(lobbyType, maxPlayers);
            SceneManager.LoadScene("MultiplayerScene");
        }
        else
        {
            Debug.LogError("请输入有效的人数（1-250）");
        }
    }

    //目前没用
    public void StartAsServer()
    {
        GameSessionData.NetworkRole = "Server";
        SceneManager.LoadScene("MultiplayerScene");
    }

    public async void StartAsClient()
    {
        string lobbyId = steamIdInputField.text; // 让玩家输入大厅 ID
        GameSessionData.NetworkRole = "Client";
        bool success = await SteamManager.Instance.JoinLobbyAsync(lobbyId);
        if (success)
            SceneManager.LoadScene("MultiplayerScene");
    }
}