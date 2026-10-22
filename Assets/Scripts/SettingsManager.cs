using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio Managers")]
    public PhonicsAudioManager phonicsAudioManager;
    public PhonicsGameManager phonicsGameManager;

    [Header("UI")]
    public TMP_Text musicText;
    public TMP_Text sfxText;
    public TMP_Text saveText;

    private float savedMusicVolume;
    private float savedSFXVolume;

    private void Start()
    {
        savedMusicVolume =
            PlayerPrefs.GetFloat("MusicVolume", 1f);

        savedSFXVolume =
            PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = savedMusicVolume;
        sfxSlider.value = savedSFXVolume;

        UpdateMusicText(savedMusicVolume);
        UpdateSFXText(savedSFXVolume);

        if (saveText != null)
            saveText.text = "";

        musicSlider.onValueChanged.AddListener(UpdateMusicText);
        sfxSlider.onValueChanged.AddListener(UpdateSFXText);
    }

    public void UpdateMusicText(float value)
    {
        if (musicText != null)
        {
            musicText.text =
                Mathf.RoundToInt(value * 100) + "%";
        }

        if (BackgroundMusic.Instance != null)
        {
            float volume = value <= 0.05f ? 0f : value;

            BackgroundMusic.Instance.audioSource.volume =
                volume;
        }
    }

    public void UpdateSFXText(float value)
    {
        if (sfxText != null)
        {
            sfxText.text =
                Mathf.RoundToInt(value * 100) + "%";
        }

        float volume = value <= 0.05f ? 0f : value;

        if (phonicsAudioManager != null)
        {
            phonicsAudioManager.SetVolume(volume);
        }

        if (phonicsGameManager != null)
        {
            phonicsGameManager.SetVolume(volume);
        }
    }

    public void SaveSettings()
    {
        savedMusicVolume = musicSlider.value;
        savedSFXVolume = sfxSlider.value;

        PlayerPrefs.SetFloat(
            "MusicVolume",
            savedMusicVolume);

        PlayerPrefs.SetFloat(
            "SFXVolume",
            savedSFXVolume);

        PlayerPrefs.Save();

        StartCoroutine(ShowSaved());
    }

    public void CloseSettings()
    {
        musicSlider.value = savedMusicVolume;
        sfxSlider.value = savedSFXVolume;

        UpdateMusicText(savedMusicVolume);
        UpdateSFXText(savedSFXVolume);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private IEnumerator ShowSaved()
    {
        if (saveText != null)
            saveText.text = "Saved!";

        yield return new WaitForSeconds(2f);

        if (saveText != null)
            saveText.text = "";
    }
}