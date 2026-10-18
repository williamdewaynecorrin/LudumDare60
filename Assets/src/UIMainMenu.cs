using System;
using TMPro;
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
    private TMP_Text highscore;
    [SerializeField]
    private Button buttonplay;
    [SerializeField]
    private Button buttonexit;
    [SerializeField]
    private Slider volume;
    [SerializeField]
    private AudioClipExt sfxclick;

    void Awake()
    {
        buttonplay.onClick.AddListener(OnPlayClicked);
        buttonexit.onClick.AddListener(OnExitClicked);

        volume.value = AudioManager.gMasterVolumeMult;
        volume.onValueChanged.AddListener(OnSliderVolumeChanged);

        int displayscore = PlayerPrefs.HasKey(PlayerScore.kHighScoreKey) ? PlayerPrefs.GetInt(PlayerScore.kHighScoreKey) : PlayerScore.BestScore;
        highscore.text = string.Format("BEST: {0}{1}", Extensions.kGlyphIDNote, displayscore);
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

    private void OnSliderVolumeChanged(float ratio)
    {
        AudioManager.gMasterVolumeMult = Mathf.Clamp01(ratio);
    }
}
