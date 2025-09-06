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
	// 001=틸로슨, 002=시안, 003=텔레스크린, 004=채링턴

	[Header("Which NPC")]
	[SerializeField] private NpcId npc = NpcId.NPC_001;

	[Header("Names / Typing")]
	[SerializeField] private string npcDisplayName = "NPC";
	[SerializeField] private string playerDisplayName = "나";
	[SerializeField] private float charDelay = 0.05f;
	[SerializeField] private float punctuationHold = 0.08f;
	[SerializeField] private bool usePunctuationHold = true;

	[Header("UI Refs")]
	[SerializeField] private RectTransform talkRowRoot; // 이름+대사 부모(좌우 반전 대상)
	[SerializeField] private TMP_Text nameLeft;         // 단일 이름 텍스트
	[SerializeField] private TMP_Text dialogueText;
	[SerializeField] private Button backgroundClick;    // 화면 전체 클릭 캐쳐

	[Header("Choice (Text Hover)")]
	[SerializeField] private CanvasGroup choiceGroup;   // 선택지 그룹
	[SerializeField] private ChoiceHoverText choice1Text;
	[SerializeField] private ChoiceHoverText choice2Text;

	[Header("Navigation")]
	[SerializeField] private string nextSceneOnFinish = "MainScene"; // 끝나면 가는 씬 (스테이지 진입)

	// 내부 상태
	private readonly List<string> lines = new();
	private int index;
	private bool isTyping;
	private Coroutine typingCo;
	private string currentFullRich;
	private bool isTransitioning;

	// 선택지 트리거 인덱스(없으면 -1)
	private int choiceTriggerIndex = -1;

	// Choice용 메서드 캐시
	private void OnChoose1() => OnChoose(1);
	private void OnChoose2() => OnChoose(2);

	void Awake()
	{
		// 기본 이름 자동 세팅(비워두면)
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

	// --- 인라인 대사(새 기획, 플레이어는 @…@ 래핑) ---
	void BuildLinesInline()
	{
		lines.Clear();

		switch (npc)
		{
			// [틸로슨] → 끝나면 Stage 1
			case NpcId.NPC_001:
				lines.Add("<b><size=70>빅 브라더 님은 위대하다!!</size></b>");
				// (여기서 선택지 오픈 예정)
				lines.Add("@그래 이 모든 건 빅 브라더님의 뜻으로.@");
				lines.Add("그나저나 어디로 가나?");
				lines.Add("@아 이쪽에 잠시 볼일이 있어서…@");
				lines.Add("그래 그 쪽에 <b>사상 경찰</b>이 정말 많이 있더라고.");
				lines.Add("뭐 중요한 게 있나 보더군.");
				lines.Add("@(대장 [          ]이 남겨둔 암호를 지키는 자들인가보군. 쉽지 않겠는걸.)@");
				lines.Add("@그렇군. 다들 고생하시네.@");
				lines.Add("<b><size=70>그래. 이 모든 건 빅 브라더님을 위하여.</size></b>");
				break;

			// [시안] → 끝나면 Stage 2
			case NpcId.NPC_002:
				lines.Add("어제 사상범들이 교수형에 처하는 걸 봤나?");
				// (여기서 선택지 오픈 예정)
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

			// [텔레스크린] → 선택지 없음, 끝나면 Stage 3
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

			// [채링턴] → 끝나면 Stage 4
			case NpcId.NPC_004:
				lines.Add("…");
				lines.Add("너는 <b>너를 믿나?</b>");
				lines.Add("너의 <b>선택을 믿냐</b>는 말이다.");
				// (여기서 선택지 오픈 예정)
				lines.Add("<b><size=70>뭐가 됐든 너의 선택을 존중하겠다.</size></b>");
				lines.Add("<b><size=70>그 결과 또한 네가 짊어지어야 하는 것.</size></b>");
				break;
		}
	}

	// 선택지 트리거 위치(기획 고정)
	void SetChoiceTriggerByDesign()
	{
		choiceTriggerIndex = npc switch
		{
			NpcId.NPC_001 => 0, // 틸로슨: 첫 줄 후
			NpcId.NPC_002 => 0, // 시안: 첫 줄 후
			NpcId.NPC_003 => -1, // 텔레스크린: 선택지 없음
			NpcId.NPC_004 => 2, // 채링턴: 두 번째 줄 후
			_ => -1
		};
	}

	// --- 표시 ---
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
		// 선택지 트리거 지점에서 클릭 → 선택지 오픈
		if (choiceTriggerIndex >= 0 && index == choiceTriggerIndex && !isTyping)
		{
			OpenChoice();
			return;
		}

		if (isTyping) ForceComplete();
		else Next();
	}

	// === 선택지 ===
	void OpenChoice()
	{
		if (dialogueText) dialogueText.text = string.Empty;

		if (choice1Text && choice2Text)
		{
			switch (npc)
			{
				case NpcId.NPC_001: // 틸로슨
					choice1Text.SetText("1) 뭐라고?");
					choice2Text.SetText("2) 빅브라더님은 위대하다!!");
					break;
				case NpcId.NPC_002: // 시안
					choice1Text.SetText("1) 일을 했었네. 영화로 볼 수 있겠지.");
					choice2Text.SetText("2) 정말 볼만한 교수형이었어.");
					break;
				case NpcId.NPC_004: // 채링턴
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

		var inserts = GetInsertedLinesForChoice(which);
		if (inserts != null && inserts.Count > 0)
		{
			for (int k = 0; k < inserts.Count; k++)
				lines.Insert(index + 1 + k, inserts[k]);

			Next(); // 첫 삽입 줄부터 출력
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
			case NpcId.NPC_001: // 틸로슨: 플레이어 → 틸로슨 응답
				if (which == 1)
					return new List<string> { "@뭐라고?@", "지금 뭐라고 했나?" };
				else
					return new List<string> { "@빅브라더님은 위대하다!!@", "요즘 일은 잘 되어가나?" };

			case NpcId.NPC_002: // 시안: 플레이어 응답만
				return new List<string>
				{
					which == 1 ? "@일을 했었네. 영화로 볼 수 있겠지.@"
							   : "@정말 볼만한 교수형이었어.@"
				};

			case NpcId.NPC_004: // 채링턴: 플레이어 응답만
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
		if (backgroundClick) backgroundClick.interactable = !v; // 선택지 열리면 배경 클릭 차단
	}

	// --- 유틸 ---
	// 플레이어 대사: 문자열이 @...@ 로 감싸져 있으면 true
	bool IsPlayerLine(string src, out string stripped)
	{
		if (!string.IsNullOrEmpty(src) && src.Length >= 2 && src[0] == '@' && src[^1] == '@')
		{
			// @@ → @ 이스케이프 허용 (원치 않으면 Replace 제거)
			stripped = src.Substring(1, src.Length - 2).Replace("@@", "@");
			return true;
		}
		stripped = src; // NPC 대사(리치태그 포함 가능)
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
	}

	void ForceComplete()
	{
		if (!isTyping) return;
		if (typingCo != null) StopCoroutine(typingCo);
		typingCo = null;
		isTyping = false;
		dialogueText.text = currentFullRich ?? dialogueText.text;
	}

	// --- 씬 전환 ---
	void FinishSequence()
	{
		if (isTransitioning) return;
		isTransitioning = true;

		int next = GetDefaultNextStageByNpc(); // 1/2/3/4 고정 매핑
		if (next > 0)
		{
			PlayerPrefs.SetInt(KEY_NEXT_STAGE, next);
			PlayerPrefs.Save();
		}
		else
		{
			PlayerPrefs.DeleteKey(KEY_NEXT_STAGE);
		}

		SceneManager.LoadScene(nextSceneOnFinish);
	}

	int GetDefaultNextStageByNpc()
	{
		switch (npc)
		{
			case NpcId.NPC_001: return 1; // 틸로슨
			case NpcId.NPC_002: return 2; // 시안
			case NpcId.NPC_003: return 3; // 텔레스크린
			case NpcId.NPC_004: return 4; // 채링턴
			default: return 0;
		}
	}

	// === 좌우 반전(플레이어 대사 전용) ===
	void SetFlipped(bool flipped)
	{
		// 부모 컨테이너 반전
		if (talkRowRoot)
		{
			var s = talkRowRoot.localScale;
			s.x = flipped ? -1f : 1f;
			talkRowRoot.localScale = s;
		}

		// 텍스트는 다시 반전해서 정상 읽기
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
