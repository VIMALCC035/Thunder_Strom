using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField]
    private VideoPlayer videoPlayer;

    [Header("Slider")]
    [SerializeField]
    private Slider progressSlider;

    [Header("Direction")]
    [SerializeField]
    private bool allowReverse = true;

    [SerializeField]
    private bool resetOnReverseToggle = false;

    private float lastSliderValue;
    private bool videoPrepared;

    private void Awake()
    {
        if (progressSlider != null)
        {
            progressSlider.onValueChanged.AddListener(
                OnSliderValueChanged);
        }

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    private void Start()
    {
        InitializeVideo();
    }

    private void OnDestroy()
    {
        if (progressSlider != null)
        {
            progressSlider.onValueChanged.RemoveListener(
                OnSliderValueChanged);
        }

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }

    public void InitializeVideo()
    {
        lastSliderValue = 0f;
        videoPrepared = false;

        if (progressSlider != null)
        {
            progressSlider.SetValueWithoutNotify(0f);
        }

        if (videoPlayer != null)
        {
            videoPlayer.Prepare();
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        videoPrepared = true;

        videoPlayer.Play();
        videoPlayer.Pause();

        videoPlayer.frame = 0;
    }

    private void OnSliderValueChanged(float value)
    {
        if (!videoPrepared)
            return;

        if (!allowReverse)
        {
            if (value < lastSliderValue)
            {
                progressSlider.SetValueWithoutNotify(
                    lastSliderValue);

                return;
            }
        }

        lastSliderValue = value;

        UpdateVideoProgress(value);
    }

    private void UpdateVideoProgress(float normalizedValue)
    {
        if (videoPlayer == null)
            return;

        long targetFrame =
            (long)(normalizedValue *
            (videoPlayer.frameCount - 1));

        videoPlayer.frame = targetFrame;
    }

    public void ResetVideo()
    {
        lastSliderValue = 0f;

        if (progressSlider != null)
        {
            progressSlider.SetValueWithoutNotify(0f);
        }

        if (videoPrepared)
        {
            videoPlayer.frame = 0;
        }
    }

    public void SetProgress(float normalizedValue)
    {
        normalizedValue =
            Mathf.Clamp01(normalizedValue);

        if (!allowReverse &&
            normalizedValue < lastSliderValue)
        {
            return;
        }

        lastSliderValue = normalizedValue;

        if (progressSlider != null)
        {
            progressSlider.SetValueWithoutNotify(
                normalizedValue);
        }

        UpdateVideoProgress(normalizedValue);
    }

    public void EnableReverse()
    {
        allowReverse = true;

        if (resetOnReverseToggle)
        {
            ResetVideo();
        }
    }

    public void DisableReverse()
    {
        allowReverse = false;

        if (resetOnReverseToggle)
        {
            ResetVideo();
        }
    }

    public void SetReverseEnabled(bool enabled)
    {
        allowReverse = enabled;

        if (resetOnReverseToggle)
        {
            ResetVideo();
        }
    }

    public bool IsReverseEnabled()
    {
        return allowReverse;
    }

    public bool IsCompleted()
    {
        return lastSliderValue >= 0.999f;
    }

    public float GetProgress()
    {
        return lastSliderValue;
    }
}