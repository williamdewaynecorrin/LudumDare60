using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField]
    private SOGameConstants gameconstants;
    [SerializeField]
    private Image logo;
    [SerializeField]
    private Button buttonplay;
    [SerializeField]
    private Button buttonexit;
    [SerializeField]
    private AudioClipExt sfxclick;

    void Awake()
    {
        buttonplay.onClick.AddListener(OnPlayClicked);
        buttonexit.onClick.AddListener(OnExitClicked);
    }

    private void OnPlayClicked()
    {
        AudioManager.Play2D(sfxclick);
        SceneManager.LoadScene(gameconstants.gamelevel);
    }

    private void OnExitClicked()
    {
        AudioManager.Play2D(sfxclick);
        Application.Quit();
    }
}
