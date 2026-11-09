using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles del Menú")]
    public GameObject creditsPanel;
    public GameObject settingsPanel;

    [Header("Audio (Opcional)")]
    public AudioSource uiAudioSource;
    public AudioClip buttonClickSFX;

    public void PlayGame()
    {
        PlayButtonClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene"); // Cambia por el nombre de tu escena de juego
    }

    public void OpenCredits(bool open)
    {
        PlayButtonClickSound();
        if (creditsPanel != null) creditsPanel.SetActive(open);
    }

    public void OpenSettings(bool open)
    {
        PlayButtonClickSound();
        if (settingsPanel != null) settingsPanel.SetActive(open);
    }

    public void QuitGame()
    {
        PlayButtonClickSound();
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    private void PlayButtonClickSound()
    {
        if (uiAudioSource != null && buttonClickSFX != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSFX);
        }
    }
}