using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhonicsAudioManager : MonoBehaviour
{
    public static PhonicsAudioManager instance;

    [Header("Audio Setup")]
    public List<AudioClip> phonemeClips;
    public AudioSource audioSource;

    [Header("Feedback Clips")]
    public AudioClip correctClip;
    public AudioClip wrongClip;

    private Dictionary<string, AudioClip> phonemeMap =
        new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (instance == null)
            instance = this;

        foreach (var clip in phonemeClips)
        {
            if (clip != null)
            {
                phonemeMap[clip.name.ToLower()] = clip;
            }
        }
    }

    void Start()
    {
        ApplySavedVolume();
    }

    public void ApplySavedVolume()
    {
        if (audioSource == null)
            return;

        float volume =
            PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (volume <= 0.05f)
            volume = 0f;

        audioSource.volume = volume;
    }

    public void SetVolume(float volume)
    {
        if (audioSource == null)
            return;

        if (volume <= 0.05f)
            volume = 0f;

        audioSource.volume = volume;
    }

    /// <summary>
    /// Play a single phoneme immediately.
    /// </summary>
    public void PlayPhoneme(string phoneme)
    {
        string key = phoneme.ToLower();

        if (phonemeMap.ContainsKey(key))
        {
            audioSource.clip = phonemeMap[key];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning(
                "No audio clip found for phoneme: " +
                phoneme);
        }
    }

    /// <summary>
    /// Convenience method for letters.
    /// </summary>
    public void PlayLetter(string letter)
    {
        PlayPhoneme(letter);
    }

    /// <summary>
    /// Play a word by sequencing phoneme clips.
    /// </summary>
    public void PlayWord(string word)
    {
        if (string.IsNullOrEmpty(word))
            return;

        StartCoroutine(
            PlayPhonemes(word.ToLower()));
    }

    private IEnumerator PlayPhonemes(string word)
    {
        foreach (char c in word)
        {
            string phoneme = c.ToString();

            if (phonemeMap.ContainsKey(phoneme))
            {
                audioSource.clip =
                    phonemeMap[phoneme];

                audioSource.Play();

                yield return new WaitForSeconds(
                    audioSource.clip.length);
            }
        }
    }

    /// <summary>
    /// Play Correct feedback sound.
    /// </summary>
    public void PlayCorrect()
    {
        if (correctClip != null)
        {
            audioSource.clip = correctClip;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Play Wrong feedback sound.
    /// </summary>
    public void PlayWrong()
    {
        if (wrongClip != null)
        {
            audioSource.clip = wrongClip;
            audioSource.Play();
        }
    }
}