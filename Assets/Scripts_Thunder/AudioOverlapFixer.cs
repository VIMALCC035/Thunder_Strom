using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOverlapFixer : MonoBehaviour
{
    [Header("Detection")]
    public float fadeDuration = 1f;
    public float checkInterval = 0.1f;

    [Header("Ignore These Audio Sources")]
    public AudioSource[] ignoredSources;

    [Header("Ignore Objects With These Tags")]
    public string[] ignoredTags;

    private Dictionary<AudioSource, float> playTimes =
        new Dictionary<AudioSource, float>();

    private HashSet<AudioSource> fadingSources =
        new HashSet<AudioSource>();

    private void Start()
    {
        StartCoroutine(MonitorAudio());
    }

    bool ShouldIgnore(AudioSource source)
    {
        if (source == null)
            return true;

        // Ignore specific AudioSources
        if (ignoredSources != null)
        {
            foreach (AudioSource ignored in ignoredSources)
            {
                if (ignored == source)
                    return true;
            }
        }

        // Ignore by tag
        if (ignoredTags != null)
        {
            foreach (string tag in ignoredTags)
            {
                if (string.IsNullOrEmpty(tag))
                    continue;

                if (source.gameObject.CompareTag(tag))
                    return true;
            }
        }

        return false;
    }

    IEnumerator MonitorAudio()
    {
        while (true)
        {
            AudioSource[] sources =
                FindObjectsByType<AudioSource>(
                    FindObjectsSortMode.None);

            List<AudioSource> playingSources =
                new List<AudioSource>();

            foreach (AudioSource source in sources)
            {
                if (source == null)
                    continue;

                if (ShouldIgnore(source))
                    continue;

                if (source.isPlaying)
                {
                    if (!playTimes.ContainsKey(source))
                    {
                        playTimes[source] = Time.time;
                    }

                    playingSources.Add(source);
                }
                else
                {
                    playTimes.Remove(source);
                }
            }

            if (playingSources.Count > 1)
            {
                AudioSource newest =
                    playingSources[0];

                float newestTime =
                    playTimes[newest];

                foreach (AudioSource source in playingSources)
                {
                    if (playTimes[source] > newestTime)
                    {
                        newest = source;
                        newestTime = playTimes[source];
                    }
                }

                foreach (AudioSource source in playingSources)
                {
                    if (source == newest)
                        continue;

                    if (!fadingSources.Contains(source))
                    {
                        StartCoroutine(
                            FadeOutAndStop(source));
                    }
                }
            }

            yield return new WaitForSeconds(
                checkInterval);
        }
    }

    IEnumerator FadeOutAndStop(AudioSource source)
    {
        if (source == null)
            yield break;

        fadingSources.Add(source);

        float originalVolume =
            source.volume;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            if (source == null)
                yield break;

            timer += Time.deltaTime;

            source.volume =
                Mathf.Lerp(
                    originalVolume,
                    0f,
                    timer / fadeDuration);

            yield return null;
        }

        if (source)
        {
            source.Stop();
            source.volume = originalVolume;
        }

        fadingSources.Remove(source);
    }
}