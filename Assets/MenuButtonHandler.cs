using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonHandler : MonoBehaviour
{
    // This method is called when the Play button is pressed
    public void OnPlayButton()
    {
        // Replace "GameScene" with the name of your main game scene
        SceneManager.LoadScene("Classroom");
    }

    // This method is called when the Quit button is pressed
    public void OnQuitButton()
    {
        // Quit the application
        Debug.Log("Quit button pressed");
        Application.Quit();

        // For editor testing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
