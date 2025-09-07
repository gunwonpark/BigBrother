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

    private void Awake()
    {
        if (_screen) _screen.enabled = false;

        _vp.playOnAwake = false;
        _vp.waitForFirstFrame = true;
        _vp.isLooping = false;
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

    private void OnVideoEnd(VideoPlayer _)
    {
        SceneManager.LoadScene("NPCScene");
    }

    private void OnDestroy()
    {
        _vp.loopPointReached -= OnVideoEnd;
    }
}
