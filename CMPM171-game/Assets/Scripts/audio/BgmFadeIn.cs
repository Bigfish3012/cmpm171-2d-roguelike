using UnityEngine;

public class FadeInBGM : MonoBehaviour
{
    public float fadeDuration = 2f;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
        audioSource.Play();
        StartCoroutine(FadeIn());
    }

    System.Collections.IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 0.2f, timer / fadeDuration);
            yield return null;
        }
    }
}
