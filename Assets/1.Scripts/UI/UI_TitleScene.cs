using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_TitleScene : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button endButton;

    [Header("Groups / Visuals")]
    [SerializeField] private CanvasGroup titleGroup;   
    [SerializeField] private FullEyeController fullEye; 
    [SerializeField] private TMP_Text mottoText;      
    [SerializeField] private CanvasGroup blackFade;   

    [Header("Timings")]
    [SerializeField] private float titleFadeDuration = 2f;
    [SerializeField] private float betweenOpensWait = 2f;
    [SerializeField] private float mottoFadeInDuration = 0.5f;
    [SerializeField] private float mottoHoldTime = 3f;
    [SerializeField] private float blackFadeDuration = 1.5f;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "MainScene"; // 타이틀 끝나고 넘어갈 씬

    private bool sequenceRunning;

    private void Awake()
    {
        // 안전한 초기값 보장
        if (titleGroup) { titleGroup.alpha = 1f; titleGroup.interactable = true; titleGroup.blocksRaycasts = true; }
        if (blackFade) { blackFade.alpha = 0f; blackFade.interactable = false; blackFade.blocksRaycasts = false; }
        if (mottoText) SetTMPAlpha(mottoText, 0f);
    }

    private void OnEnable()
    {
        if (startButton) startButton.onClick.AddListener(OnClickStartButton);
        if (endButton)   endButton.onClick.AddListener(OnClickEndButton);
    }

    private void OnDisable()
    {
        if (startButton) startButton.onClick.RemoveAllListeners();
        if (endButton)   endButton.onClick.RemoveAllListeners();
    }

    private void OnClickStartButton()
    {
        if (sequenceRunning) return;
        sequenceRunning = true;

        if (startButton) startButton.interactable = false;
        if (endButton)   endButton.interactable = false;

        StartCoroutine(Co_StartSequence());
    }

    private void OnClickEndButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator Co_StartSequence()
    {
        // 1) 타이틀/버튼 페이드 아웃
        if (titleGroup)
        {
            yield return StartCoroutine(FadeCanvasGroup(titleGroup, 1f, 0f, titleFadeDuration));
            titleGroup.interactable = false;
            titleGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup g, float from, float to, float duration)
    {
        if (!g || duration <= 0f) { if (g) g.alpha = to; yield break; }

        float t = 0f;
        g.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        g.alpha = to;
    }

    private IEnumerator FadeTMPAlpha(TMP_Text text, float from, float to, float duration)
    {
        if (!text || duration <= 0f) { if (text) SetTMPAlpha(text, to); yield break; }

        float t = 0f;
        SetTMPAlpha(text, from);
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetTMPAlpha(text, a);
            yield return null;
        }
        SetTMPAlpha(text, to);
    }

    private void SetTMPAlpha(TMP_Text text, float a)
    {
        var c = text.color;
        c.a = a;
        text.color = c;
    }
}