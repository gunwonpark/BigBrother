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
	[SerializeField] private CanvasGroup titleGroup;     // 로고+버튼 묶음
	[SerializeField] private FullEyeController fullEye;  // 눈 애니메이션 컨트롤러
	[SerializeField] private TMP_Text mottoText;         // "IGNORANCE IS STRENGTH"
	[SerializeField] private CanvasGroup blackFade;      // 전체 검은 페이드

	[Header("Timings")]
	[SerializeField] private float titleFadeDuration = 2f;   // 타이틀 UI 페이드아웃
	[SerializeField] private float openDuration = 1.2f; // 눈 Open 클립 길이(대략)
	[SerializeField] private float betweenOpensWait = 2f;   // 첫 Open 후 대기
	[SerializeField] private float blinkFallbackWait = 0.8f; // Blink 이벤트 없을 때 대기
	[SerializeField] private float mottoFadeInDuration = 0.5f; // 모토 페이드인
	[SerializeField] private float mottoHoldTime = 3f;   // 모토 유지 시간
	[SerializeField] private float blackFadeDuration = 1.5f; // 블랙아웃 시간

	[Header("Next Scene")]
	[SerializeField] private string nextSceneName = "MainScene";

	private bool sequenceRunning;

	private void Awake()
	{
		// 안전 초기화
		if (titleGroup) { titleGroup.alpha = 1f; titleGroup.interactable = true; titleGroup.blocksRaycasts = true; }
		if (blackFade) { blackFade.alpha = 0f; blackFade.interactable = false; blackFade.blocksRaycasts = false; }
		if (mottoText) SetTMPAlpha(mottoText, 0f);
	}

	private void OnEnable()
	{
		if (startButton) startButton.onClick.AddListener(OnClickStartButton);
		if (endButton) endButton.onClick.AddListener(OnClickEndButton);
	}
	private void OnDisable()
	{
		if (startButton) startButton.onClick.RemoveAllListeners();
		if (endButton) endButton.onClick.RemoveAllListeners();
	}

	private void OnClickStartButton()
	{
		if (sequenceRunning) return;
		sequenceRunning = true;

		if (startButton) startButton.interactable = false;
		if (endButton) endButton.interactable = false;

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
		// 타이틀 UI 페이드아웃
		if (titleGroup)
		{
			yield return FadeCanvasGroup(titleGroup, 1f, 0f, titleFadeDuration);
			titleGroup.interactable = false;
			titleGroup.blocksRaycasts = false;
		}

		yield return new WaitForSeconds(1f);

		// 눈 Open 2회(사이 2초 대기)
		yield return WaitForOpenOnce();
		yield return new WaitForSeconds(betweenOpensWait);
		//yield return WaitForOpenOnce();

		// Blink 1회 끝날 때까지 대기
		yield return WaitForBlinkOnce();

		yield return new WaitForSeconds(1f);

		if (mottoText)
		{
			mottoText.text = "IGNORANCE IS STRENGTH";
			yield return FadeTMPAlpha(mottoText, 0f, 1f, 0);
			yield return new WaitForSeconds(mottoHoldTime);
		}

		if (blackFade)
		{
			blackFade.blocksRaycasts = true;
			blackFade.interactable = true;
			yield return FadeCanvasGroup(blackFade, 0f, 1f, blackFadeDuration);
		}

		yield return new WaitForSeconds(3f);

		SceneManager.LoadScene(nextSceneName);
	}

	private IEnumerator WaitForOpenOnce()
	{
		if (fullEye) fullEye.FullEyeOpen();
		// Open에는 애니메이션 이벤트 콜백이 없으니, 길이를 인스펙터로 받는 방식
		yield return new WaitForSeconds(openDuration);
	}

	private IEnumerator WaitForBlinkOnce()
	{
		bool done = false;
		System.Action onEnd = () => done = true;

		if (fullEye)
		{
			fullEye.OnBlinkAnimationEnd += onEnd;
			fullEye.FullEyeBlink();
		}

		float t = 0f;
		while (!done && t < blinkFallbackWait)
		{
			t += Time.deltaTime;
			yield return null;
		}

		if (fullEye) fullEye.OnBlinkAnimationEnd -= onEnd;
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
			SetTMPAlpha(text, Mathf.Lerp(from, to, t / duration));
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
