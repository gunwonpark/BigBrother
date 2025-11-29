using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class UI_IntroScene : MonoBehaviour
{
    [SerializeField] private RawImage _screen;            // RenderTexture 표시용
    [SerializeField] private VideoPlayer _vp;             // Render Mode: RenderTexture
    [SerializeField] private string _fileName = "IntroFinal.mp4"; // StreamingAssets      
    [SerializeField] private Button _skipButton;         // 스킵 버튼    
    [SerializeField] private Image _fadeImage;
    private bool _isSkipped = false;
    
    private void Awake()
    {
        if (_screen) _screen.enabled = false;

        _vp.playOnAwake = false;
        _vp.waitForFirstFrame = true;
        _vp.isLooping = false;

        _skipButton.onClick.AddListener(SkipVideo);
    }

    private void Start()
    {
        StartCoroutine(PlayFlow());
    }
    private IEnumerator PlayFlow()
    {
        _vp.loopPointReached += OnVideoEnd;

        _vp.url = System.IO.Path.Combine(Application.streamingAssetsPath, _fileName);
        _vp.Prepare();

        while (!_vp.isPrepared)
            yield return null;

        if (_screen) _screen.enabled = true;
        _vp.Play();
    }

    private void SkipVideo()
    {
        if(_isSkipped) return;
        _isSkipped = true;
        _vp.Stop();
        StartCoroutine(FadeAndSkip());
    }

    private IEnumerator FadeAndSkip()
    {
        _fadeImage.gameObject.SetActive(true);
        float duration = 3.0f;
        float elapsed = 0.0f;
        Color startColor = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 0.0f);
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1.0f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        _fadeImage.color = targetColor;
        SceneManager.LoadScene("NPCScene");
    }

    private void OnVideoEnd(VideoPlayer _)
    {
        if(_isSkipped) return;
        SceneManager.LoadScene("NPCScene");
    }

    private void OnDestroy()
    {
        _vp.loopPointReached -= OnVideoEnd;
    }
}
