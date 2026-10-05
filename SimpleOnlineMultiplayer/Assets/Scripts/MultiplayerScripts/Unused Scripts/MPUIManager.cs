using UnityEngine;

public class MPUIManager : MonoBehaviour
{
    // Muliplayer UI Manager

    [SerializeField] private GameObject authenticateUI;
    [SerializeField] private GameObject lobbyUI; // lobbies showen
    [SerializeField] private GameObject createLobbyUI; // maak lobby
    [SerializeField] private GameObject inLobbyUI; // in de lobby
    [SerializeField] private GameObject inGameUI; // in de game ui

    //private void Start()
    //{
    //    authenticateUI.SetActive(true);
    //}

    public void AuthenticateUIOnOff()
    {
        if (authenticateUI.activeSelf)
        {
            authenticateUI.SetActive(false);
        }
        else { authenticateUI.SetActive(true); }
    }

    public void LobbyUIOnOff()
    {
        if (lobbyUI.activeSelf)
        {
            lobbyUI.SetActive(false);
        }
        else { lobbyUI.SetActive(true); }
    }

    public void CreateLobbyUIOnOff()
    {
        lobbyUI.SetActive(false);
        if (createLobbyUI.activeSelf)
        {
            createLobbyUI.SetActive(false);
        }
        else { createLobbyUI.SetActive(true); }
    }

    public void InLobbyUIOnOff()
    {
        if (inLobbyUI.activeSelf)
        {
            inLobbyUI.SetActive(false);
        }
        else { inLobbyUI.SetActive(true); }
    }

    public void InGameUIOnOff()
    {
        if (inGameUI.activeSelf)
        {
            inGameUI.SetActive(false);
        }
        else { inGameUI.SetActive(true); }
    }

    public void AuthenticateToLobbyUI()
    {
        authenticateUI.SetActive(false);
        lobbyUI.SetActive(true);
    }

    public void CreateLobbyToInLobbyUI() 
    {
        createLobbyUI.SetActive(false);
        inLobbyUI.SetActive(true);
    }

    public void LobbyToInLobbyUi()
    {
        lobbyUI.SetActive(false);
        inLobbyUI.SetActive(true);
    }
}
