using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Sound : MonoBehaviour
{
    [SerializeField] private Button _soundToggleButton;
    private bool _isSoundOn = false;

    [SerializeField] private Slider _masterSlider;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        _soundToggleButton.onClick.AddListener(() =>
        {
            ToggleSoundSlider();
        });

        _masterSlider.value = SoundManager.Instance.GetVolume("MasterVolume");

        _masterSlider.onValueChanged.AddListener(value => SoundManager.Instance.SetMasterVolume(value));
        _masterSlider.gameObject.SetActive(false);
    }

    private void ToggleSoundSlider()
    {
        if (_isSoundOn)
        {
            _masterSlider.gameObject.SetActive(false);
        }
        else
        {
            _masterSlider.gameObject.SetActive(true);
        }
        _isSoundOn = !_isSoundOn;
    }

    private void OnDisable()
    {
        SoundManager.Instance.SaveVolumeSettings();
    }

    private void OnDestroy()
    {
        SoundManager.Instance.SaveVolumeSettings();
    }
}
