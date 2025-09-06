using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NPCDialogueController : MonoBehaviour
{
	private const string KEY_NEXT_STAGE = "NextStage";

	public enum NpcId { NPC_001, NPC_002, NPC_003, NPC_004 }

	[Header("Which NPC")]
	[SerializeField] private NpcId npc = NpcId.NPC_001;

	[Header("Names / Typing")]
	[SerializeField] private string npcDisplayName = "NPC";
	[SerializeField] private string playerDisplayName = "나";
	[SerializeField] private float charDelay = 0.05f;
	[SerializeField] private float punctuationHold = 0.08f;
	[SerializeField] private bool usePunctuationHold = true;

	[Header("UI Refs")]
	[SerializeField] private RectTransform talkRowRoot;
	[SerializeField] private TMP_Text nameLeft;
	[SerializeField] private TMP_Text dialogueText;
	[SerializeField] private Button backgroundClick;

	[Header("Choice (Text Hover)")]
	[SerializeField] private CanvasGroup choiceGroup;
	[SerializeField] private ChoiceHoverText choice1Text;
	[SerializeField] private ChoiceHoverText choice2Text;

	[Header("NPC Visual (반투명 제어)")]
	[SerializeField] private CanvasGroup npcVisualGroup;
	[SerializeField, Range(0f, 1f)] private float npcAlphaNpcSpeaking = 1f;       // NPC가 말할 때
	[SerializeField, Range(0f, 1f)] private float npcAlphaPlayerSpeaking = 0.71f; // 플레이어가 말할 때

	[Header("Navigation")]
	[SerializeField] private string nextScene = "MainScene";

	[Header("Tilosen FX")]
	[SerializeField] private CanvasGroup eye1;
	[SerializeField] private CanvasGroup eye2;
	[SerializeField] private float eyeFadeDuration = 2f;   // fade-in
	[SerializeField] private float eyeGapBetween = 1f;     // 눈1→눈2 사이
	[SerializeField] private float afterEyesWait = 2f;     // 눈2 후 대기
	[SerializeField] private float eyeFadeOutDuration = 1.2f; // fade-out

	// 내부 상태
	private readonly List<string> lines = new();
	private int index;
	private bool isTyping;
	private Coroutine typingCo;
	private string currentFullRich;
	private bool isTransitioning;

	private int choiceTriggerIndex = -1;

	// 틸로슨 연출 상태
	private bool tilosenPendingEyeFX = false;        // "뭐라고?" 라인 끝났고, 클릭 시 눈 연출 시작
	private string tilosenPendingNpcLine = null;     // 눈 연출 후 출력할 빨간 대사
	private bool tilosenAwaitingLoopChoice = false;  // 빨간 대사 후 클릭 시 다시 선택지
	private int tilosenEyeFXTriggerLineIndex = -1;   // "@뭐라고?@" 라인 인덱스

	// ("빅브라더님은 위대하다!!") 라인 끝나면 자동 fade-out
	private bool tilosenPendingFadeOutAfterShout = false;
	private int tilosenShoutLineIndex = -1;

	// 한 줄 타이핑 완료 콜백 중복 방지
	private int lastTypingCompleteLineIndex = -1;

	private void OnChoose1() => OnChoose(1);
	private void OnChoose2() => OnChoose(2);

	void Awake()
	{
		if (string.IsNullOrWhiteSpace(npcDisplayName))
		{
			npcDisplayName = npc switch
			{
				NpcId.NPC_001 => "틸로슨",
				NpcId.NPC_002 => "시안",
				NpcId.NPC_003 => "텔레스크린",
				NpcId.NPC_004 => "채링턴",
				_ => "NPC"
			};
		}

		BuildLinesInline();
		SetChoiceTriggerByDesign();

		index = 0;

		if (backgroundClick) backgroundClick.onClick.AddListener(OnClickBackground);
		if (choice1Text) choice1Text.Clicked += OnChoose1;
		if (choice2Text) choice2Text.Clicked += OnChoose2;

		SetChoiceVisible(false);
		ShowCurrentLine();
	}

	void OnDestroy()
	{
		if (backgroundClick) backgroundClick.onClick.RemoveListener(OnClickBackground);
		if (choice1Text) choice1Text.Clicked -= OnChoose1;
		if (choice2Text) choice2Text.Clicked -= OnChoose2;
	}

	void BuildLinesInline()
	{
		lines.Clear();

		switch (npc)
		{
			case NpcId.NPC_001:
				lines.Add("<b><size=70>빅 브라더 님은 위대하다!!</size></b>");
				lines.Add("@그래 이 모든 건 빅 브라더님의 뜻으로.@");
				lines.Add("그나저나 어디로 가나?");
				lines.Add("@아 이쪽에 잠시 볼일이 있어서…@");
				lines.Add("그래 그 쪽에 <b>사상 경찰</b>이 정말 많이 있더라고.");
				lines.Add("뭐 중요한 게 있나 보더군.");
				lines.Add("@(대장 [          ]이 남겨둔 암호를 지키는 자들인가보군. 쉽지 않겠는걸.)@");
				lines.Add("@그렇군. 다들 고생하시네.@");
				lines.Add("<b><size=70>그래. 이 모든 건 빅 브라더님을 위하여.</size></b>");
				break;

			case NpcId.NPC_002:
				lines.Add("어제 사상범들이 교수형에 처하는 걸 봤나?");
				lines.Add("그래 감히 빅 브라더님의 뜻을 거역하는 이들이라니");
				lines.Add("교수형을 당해도 모자르지 않아 보여.");
				lines.Add("그렇지 않은가?");
				lines.Add("@그렇지.@");
				lines.Add("@(무고한 우리 동료들이 또 죽고 말았구나.)@");
				lines.Add("그나저나 어딜 갔다왔길래 그리 땀 투성인가?");
				lines.Add("뭐 어디 운동이라도 하고 왔는가?");
				lines.Add("@아무것도 아닐세.@");
				lines.Add("시시하군.");
				lines.Add("<b><size=70>아무튼 이 모든 건 빅 브라더님을 위하여.</size></b>");
				break;

			case NpcId.NPC_003:
				lines.Add("<i>작년 대비, 배급 식량은 15% 증가하였습니다.</i>");
				lines.Add("<i>빅 브라더님께서 보장하신 안정적인 공급 덕분에 저희는 새롭고 행복한 삶을 살아가며…</i>");
				lines.Add("@새롭고 행복한 삶이라는 말이 몇 번째 나오는 건지.@");
				lines.Add("@풍요부가 요즘 자주 쓰는 상투적인 말이군.@");
				lines.Add("<i>위대하신 빅 브라더님께서는 <b>사상범을 색출하기 위해 새로운 실험을 시행</b>하셨습니다.</i>");
				lines.Add("<i>사람들의 뇌에 시뮬레이션을 적용하여</i>");
				lines.Add("<i><b>극한 상황에서도 빅 브라더님에 대한 충성심이 유지</b>되는지를 확인하고…</i>");
				lines.Add("@이젠 정말 자신의 충성심을 확인하기 위해 기상천외한 방법들을 동원하는구나.@");
				lines.Add("@정말 지긋지긋하다.@");
				break;

			case NpcId.NPC_004:
				lines.Add("…");
				lines.Add("너는 <b>너를 믿나?</b>");
				lines.Add("너의 <b>선택을 믿냐</b>는 말이다.");
				lines.Add("<b><size=70>뭐가 됐든 너의 선택을 존중하겠다.</size></b>");
				lines.Add("<b><size=70>그 결과 또한 네가 짊어지어야 하는 것.</size></b>");
				break;
		}
	}

	void SetChoiceTriggerByDesign()
	{
		choiceTriggerIndex = npc switch
		{
			NpcId.NPC_001 => 0,
			NpcId.NPC_002 => 0,
			NpcId.NPC_003 => -1,
			NpcId.NPC_004 => 2,
			_ => -1
		};
	}

	void ShowCurrentLine()
	{
		if (index >= lines.Count)
		{
			FinishSequence();
			return;
		}

		string raw = lines[index];
		bool isPlayer = IsPlayerLine(raw, out string plain);

		if (nameLeft) nameLeft.text = isPlayer ? playerDisplayName : npcDisplayName;
		SetFlipped(isPlayer);
		UpdateNpcVisualAlpha(isPlayer);

		if (typingCo != null) StopCoroutine(typingCo);
		typingCo = StartCoroutine(TypeRichText(dialogueText, plain));
	}

	void Next()
	{
		index++;
		ShowCurrentLine();
	}

	public void OnClickBackground()
	{
		// 틸로슨: "@뭐라고?@" 라인 이후 → 클릭하면 눈 연출 시작
		if (npc == NpcId.NPC_001 && tilosenPendingEyeFX && !isTyping && index == tilosenEyeFXTriggerLineIndex)
		{
			StartCoroutine(CoTilosenEyeFXThenReply());
			return;
		}

		// 틸로슨 루프: 빨간 대사 뒤 클릭 시, 선택지 다시 열기
		if (npc == NpcId.NPC_001 && tilosenAwaitingLoopChoice && !isTyping)
		{
			if (choiceGroup && choiceGroup.alpha <= 0.001f)
				OpenChoice();
			return;
		}

		if (choiceTriggerIndex >= 0 && index == choiceTriggerIndex && !isTyping)
		{
			OpenChoice();
			return;
		}

		if (isTyping)
		{
			ForceComplete();
			return;
		}

		// 4) 일반 진행
		Next();
	}

	void OpenChoice()
	{
		if (dialogueText) dialogueText.text = string.Empty;

		if (choice1Text && choice2Text)
		{
			switch (npc)
			{
				case NpcId.NPC_001:
					choice1Text.SetText("1) 뭐라고?");
					choice2Text.SetText("2) 빅브라더님은 위대하다!!");
					break;
				case NpcId.NPC_002:
					choice1Text.SetText("1) 일을 했었네. 영화로 볼 수 있겠지.");
					choice2Text.SetText("2) 정말 볼만한 교수형이었어.");
					break;
				case NpcId.NPC_004:
					choice1Text.SetText("1) 그래");
					choice2Text.SetText("2) 아니");
					break;
			}
		}

		SetChoiceVisible(true);
	}

	void OnChoose(int which)
	{
		SetChoiceVisible(false);

		if (npc == NpcId.NPC_001)
		{
			if (which == 1) // "뭐라고?"
			{
				// 플레이어 대사 삽입 
				lines.Insert(index + 1, "@뭐라고?@");

				// 클릭 시점에 눈 연출을 시작하도록 플래그만 세팅
				tilosenPendingEyeFX = true;
				tilosenEyeFXTriggerLineIndex = index + 1;

				tilosenPendingNpcLine = "<b><color=#FF3B3B>지금 뭐라고 했나?</color></b>";
				tilosenAwaitingLoopChoice = true;

				Next(); // "@뭐라고?@" 라인 출력 시작
				return;
			}
			else // "빅브라더님은 위대하다!!"
			{
				tilosenAwaitingLoopChoice = false;

				// 샤우트 라인 + 다음 NPC 라인
				lines.Insert(index + 1, "@<b><size=70>빅브라더님은 위대하다!!</size></b>@");
				lines.Insert(index + 2, "요즘 일은 잘 되어가나?");

				// 샤우트 라인 종료 시 자동 FadeOut
				tilosenPendingFadeOutAfterShout = true;
				tilosenShoutLineIndex = index + 1;

				Next(); // 샤우트 라인 출력 시작
				return;
			}
		}

		// 그 외 NPC
		var inserts = GetInsertedLinesForChoice(which);
		if (inserts != null && inserts.Count > 0)
		{
			for (int k = 0; k < inserts.Count; k++)
				lines.Insert(index + 1 + k, inserts[k]);
			Next();
		}
		else
		{
			Next();
		}
	}

	List<string> GetInsertedLinesForChoice(int which)
	{
		switch (npc)
		{
			case NpcId.NPC_002:
				return new List<string>
				{
					which == 1 ? "@일을 했었네. 영화로 볼 수 있겠지.@"
							   : "@정말 볼만한 교수형이었어.@"
				};

			case NpcId.NPC_004:
				return new List<string> { which == 1 ? "@그래@" : "@아니@" };
		}
		return null;
	}

	void SetChoiceVisible(bool v)
	{
		if (choiceGroup)
		{
			choiceGroup.alpha = v ? 1f : 0f;
			choiceGroup.interactable = v;
			choiceGroup.blocksRaycasts = v;
		}
		backgroundClick.interactable = !v;
	}

	// 플레이어 대사: 문자열이 @...@ 로 감싸져 있으면 true
	bool IsPlayerLine(string src, out string stripped)
	{
		if (!string.IsNullOrEmpty(src) && src.Length >= 2 && src[0] == '@' && src[^1] == '@')
		{
			stripped = src.Substring(1, src.Length - 2).Replace("@@", "@");
			return true;
		}
		stripped = src;
		return false;
	}

	IEnumerator TypeRichText(TMP_Text target, string content)
	{
		isTyping = true;
		target.text = "";
		currentFullRich = content;

		int i = 0;
		while (i < content.Length)
		{
			if (content[i] == '<')
			{
				int close = content.IndexOf('>', i);
				if (close == -1) break;
				target.text += content.Substring(i, close - i + 1);
				i = close + 1;
				yield return null;
				continue;
			}

			char ch = content[i];
			target.text += ch;
			i++;

			if (usePunctuationHold && (ch == '.' || ch == ',' || ch == '!' || ch == '?'))
				yield return new WaitForSeconds(punctuationHold);
			else
				yield return new WaitForSeconds(charDelay);
		}

		isTyping = false;
		typingCo = null;

		OnLineTypingComplete(index);
	}

	void ForceComplete()
	{
		if (!isTyping) return;
		if (typingCo != null) StopCoroutine(typingCo);
		typingCo = null;
		isTyping = false;
		dialogueText.text = currentFullRich ?? dialogueText.text;

		// 스킵으로 끝내도 동일하게 콜백
		OnLineTypingComplete(index);
	}

	void OnLineTypingComplete(int lineIndex)
	{
		// 중복 방지
		if (lastTypingCompleteLineIndex == lineIndex) return;
		lastTypingCompleteLineIndex = lineIndex;

		// 샤우트 라인 끝난 즉시 눈 FadeOut
		if (npc == NpcId.NPC_001 && tilosenPendingFadeOutAfterShout && lineIndex == tilosenShoutLineIndex)
		{
			StartCoroutine(CoFadeOutEyesOnly());
			return;
		}
	}

	void UpdateNpcVisualAlpha(bool isPlayerSpeaking)
	{
		if (!npcVisualGroup) return;
		npcVisualGroup.alpha = isPlayerSpeaking ? npcAlphaPlayerSpeaking : npcAlphaNpcSpeaking;
	}

	IEnumerator CoTilosenEyeFXThenReply()
	{
		backgroundClick.interactable = false;

		dialogueText.gameObject.SetActive(false);

		eye1.alpha = 0f;
		eye2.alpha = 0f;

		yield return FadeCanvas(eye1, 0f, 1f, eyeFadeDuration);


		yield return new WaitForSeconds(eyeGapBetween);

		yield return FadeCanvas(eye2, 0f, 1f, eyeFadeDuration);

		yield return new WaitForSeconds(afterEyesWait);

		dialogueText.gameObject.SetActive(true);

		if (!string.IsNullOrEmpty(tilosenPendingNpcLine))
		{
			lines.Insert(index + 1, tilosenPendingNpcLine);
			tilosenPendingNpcLine = null;
		}
		tilosenPendingEyeFX = false;

		backgroundClick.interactable = true;
		Next(); // 빨간 대사 표시
	}

	IEnumerator CoFadeOutEyesOnly()
	{
		tilosenPendingFadeOutAfterShout = false; // 소비

		backgroundClick.interactable = false;

		if (AreEyesVisible())
			yield return FadeEyesOut(eyeFadeOutDuration);

		backgroundClick.interactable = true;
	}

	bool AreEyesVisible()
	{
		bool e1 = eye1 && eye1.gameObject.activeInHierarchy && eye1.alpha > 0.01f;
		bool e2 = eye2 && eye2.gameObject.activeInHierarchy && eye2.alpha > 0.01f;
		return e1 || e2;
	}

	IEnumerator FadeEyesOut(float dur)
	{
		float t = 0f;
		float a1s = eye1 ? eye1.alpha : 0f;
		float a2s = eye2 ? eye2.alpha : 0f;

		while (t < dur)
		{
			t += Time.deltaTime;
			float k = Mathf.Clamp01(t / dur);
			eye1.alpha = Mathf.Lerp(a1s, 0f, k);
			eye2.alpha = Mathf.Lerp(a2s, 0f, k);
			yield return null;
		}

		eye1.alpha = 0f;
		eye2.alpha = 0f;
	}

	IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float dur)
	{
		float t = 0f;
		if (cg) cg.alpha = from;
		while (t < dur)
		{
			t += Time.deltaTime;
			float k = Mathf.Clamp01(t / dur);
			if (cg) cg.alpha = Mathf.Lerp(from, to, k);
			yield return null;
		}
		if (cg) cg.alpha = to;
	}

	// --- 씬 전환 ---
	void FinishSequence()
	{
		if (isTransitioning) return;
		isTransitioning = true;

		int next = GetDefaultNextStageByNpc();
		if (next > 0)
		{
			DataManager.Instance.CurrentWorldLevel = next;
		}

		SceneManager.LoadScene(nextScene);
	}

	int GetDefaultNextStageByNpc()
	{
		switch (npc)
		{
			case NpcId.NPC_001: return 1;
			case NpcId.NPC_002: return 2;
			case NpcId.NPC_003: return 3;
			case NpcId.NPC_004: return 4;
			default: return 0;
		}
	}

	// === 좌우 반전(플레이어 대사 전용) ===
	void SetFlipped(bool flipped)
	{
		if (talkRowRoot)
		{
			var s = talkRowRoot.localScale;
			s.x = flipped ? -1f : 1f;
			talkRowRoot.localScale = s;
		}

		if (nameLeft)
		{
			var s = nameLeft.rectTransform.localScale;
			s.x = flipped ? -1f : 1f;
			nameLeft.rectTransform.localScale = s;
		}
		if (dialogueText)
		{
			var s = dialogueText.rectTransform.localScale;
			s.x = flipped ? -1f : 1f;
			dialogueText.rectTransform.localScale = s;
		}
	}
}
