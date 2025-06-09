using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    private static float volume = 1.0f;

    void Start()
    {
        if (volumeSlider != null)
        {
            volume = PlayerPrefs.GetFloat("Volume", 1.0f); // Load the saved volume
            volumeSlider.value = volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        ApplyVolume();
    }

    public void SetVolume(float value)
    {
        volume = value;
        PlayerPrefs.SetFloat("Volume", volume); // Save the volume
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        AudioListener.volume = volume;
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
