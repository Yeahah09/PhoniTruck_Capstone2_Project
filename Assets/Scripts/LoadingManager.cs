using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("Media")]
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    [Header("UI")]
    public Slider progressBar;
    public TMP_Text loadingText;

    [Header("Animation")]
    public float floatSpeed = 2f;
    public float floatHeight = 20f;
    public float dotSpeed = 0.5f;

    private RectTransform textRect;
    private Vector2 startPosition;
    private Vector2 animatedPosition;

    private float dotTimer;
    private int dotCount;

    private float messageTimer;
    private int currentMessage;

    private bool isDropping;

    private string[] messages =
    {
        "Getting everything ready...",
        "Loading your adventure...",
        "Preparing learning games...",
        "Almost there...",
        "Let's have fun!"
    };

    private void Start()
    {
        if (loadingText != null)
        {
            textRect = loadingText.GetComponent<RectTransform>();
            startPosition = textRect.anchoredPosition;
            animatedPosition = startPosition;

            StartCoroutine(DropInText());
        }

        if (videoPlayer != null)
        {
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.Play();
        }

        StartCoroutine(LoadGame());
    }

    private void Update()
    {
        if (textRect == null)
            return;

        float floatY =
            Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        float pulse =
            1f + Mathf.Abs(Mathf.Sin(Time.time * 4f)) * 0.12f;

        textRect.localScale = new Vector3(pulse, pulse, 1f);

        if (!isDropping)
        {
            textRect.anchoredPosition =
                startPosition + new Vector2(0, floatY);
        }
        else
        {
            textRect.anchoredPosition = animatedPosition;
        }
    }

    IEnumerator DropInText()
    {
        if (textRect == null)
            yield break;

        isDropping = true;

        Vector2 startDropPosition =
            startPosition + new Vector2(0, 200f);

        float timer = 0f;
        float duration = 0.5f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            float bounce =
                Mathf.Sin(t * Mathf.PI) * (1f - t) * 40f;

            animatedPosition =
                Vector2.Lerp(startDropPosition, startPosition, t)
                + new Vector2(0, bounce);

            yield return null;
        }

        animatedPosition = startPosition;
        isDropping = false;
    }

    IEnumerator LoadGame()
    {
        string nextScene =
            PlayerPrefs.GetString("NextScene", "Main Game");

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(nextScene);

        operation.allowSceneActivation = false;

        float duration = 4f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / duration);

            if (progressBar != null)
                progressBar.value = progress;

            dotTimer += Time.deltaTime;

            if (dotTimer >= dotSpeed)
            {
                dotTimer = 0f;

                dotCount++;

                if (dotCount > 3)
                    dotCount = 0;
            }

            messageTimer += Time.deltaTime;

            if (messageTimer >= 1f)
            {
                messageTimer = 0f;

                currentMessage++;

                if (currentMessage >= messages.Length)
                    currentMessage = 0;

                StartCoroutine(DropInText());
            }

            string dots = new string('.', dotCount);

            if (loadingText != null)
            {
                loadingText.text =
                    messages[currentMessage] +
                    "\n" +
                    $"Loading{dots} {Mathf.RoundToInt(progress * 100)}%";
            }

            yield return null;
        }

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;

        if (loadingText != null)
        {
            loadingText.text = "Let's Go!";
        }

        yield return new WaitForSeconds(0.5f);

        operation.allowSceneActivation = true;
    }
}