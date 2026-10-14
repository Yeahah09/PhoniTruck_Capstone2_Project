using UnityEngine;
using System.Collections.Generic;

public class PhonicsAudioManager : MonoBehaviour
{
    [Header("Audio Setup")]
    public List<AudioClip> phonemeClips; // assign in Inspector
    public AudioSource audioSource;

    private Dictionary<string, AudioClip> phonemeMap = new Dictionary<string, AudioClip>();

    void Awake()
    {
        foreach (var clip in phonemeClips)
        {
            if (clip != null)
            {
                phonemeMap[clip.name.ToLower()] = clip; // clip name must match phoneme
            }
        }
    }

    public void PlayWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return;
        StartCoroutine(PlayPhonemes(word.ToLower()));
    }

    private System.Collections.IEnumerator PlayPhonemes(string word)
    {
        foreach (char c in word)
        {
            string phoneme = c.ToString();
            if (phonemeMap.ContainsKey(phoneme))
            {
                audioSource.clip = phonemeMap[phoneme];
                audioSource.Play();
                yield return new WaitForSeconds(audioSource.clip.length);
            }
        }
    }
}
