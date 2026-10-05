using UnityEngine;

public class Utilities : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject startScreenObject;
    //[SerializeField] private GameObject 
    public void GoToMainMenu()
    {
       mainMenuPanel.SetActive(true); 
    }
}
