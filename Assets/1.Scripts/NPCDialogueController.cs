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
	[SerializeField] private GameObject nameLeftBox;
	[SerializeField] private GameObject nameRightBox;
	[SerializeField] private TMP_Text nameLeft;
	[SerializeField] private TMP_Text nameRight;
	[SerializeField] private TMP_Text dialogueText;
	[SerializeField] private Button backgroundClick;

	[Header("Choice (for NPC_003)")]
	//[SerializeField] private CanvasGroup choiceGroup;
	//[SerializeField] private Button choice1Button; // 1) 뭐라고?
	//[SerializeField] private Button choice2Button; // 2) 그렇구나.
	//[SerializeField] private TMP_Text choice1Label;
	//[SerializeField] private TMP_Text choice2Label;

	[Header("Navigation")]
	[SerializeField] private string nextSceneOnFinish = "MainScene"; // 001/002/004 끝나면 가는 씬
	[SerializeField] private string stageSceneName = "MainScene"; // 스테이지 진입 씬

	// 내부 상태
	private readonly List<string> lines = new();
	private int index;
	private bool isTyping;
	private Coroutine typingCo;
	private string currentFullRich;
	private bool isTransitioning;

	void Awake()
	{
		BuildLinesInline();
		index = 0;

		if (backgroundClick) backgroundClick.onClick.AddListener(OnClickBackground);
		//if (choice1Button) choice1Button.onClick.AddListener(() => OnChoose(1));
		//if (choice2Button) choice2Button.onClick.AddListener(() => OnChoose(2));
		//SetChoiceVisible(false);

		ShowCurrentLine();
	}

	void OnDestroy()
	{
		if (backgroundClick) backgroundClick.onClick.RemoveListener(OnClickBackground);
		//if (choice1Button) choice1Button.onClick.RemoveAllListeners();
		//if (choice2Button) choice2Button.onClick.RemoveAllListeners();
	}

	// --- 인라인 대사 구성 ---
	void BuildLinesInline()
	{
		lines.Clear();
		switch (npc)
		{
			case NpcId.NPC_001:
				lines.Add("빅 브라더님은 위대하다!!");
				lines.Add("안녕?");
				lines.Add("네가 옴으로써 이곳은 비로소 움직이기 시작했네.");
				lines.Add("반가워.");
				lines.Add("<이곳은 어디지?>");
				lines.Add("이곳은 말이지.");
				lines.Add("위대하신 빅 브라더님의 공간이지.");
				lines.Add("그 분은 정말 대단하신 분이야.");
				lines.Add("우리 모두를 숨쉬게 만들고 살 수 있게 해주니까 말이야.");
				lines.Add("<빅 브라더가 누구길래 그러는거지?>");
				lines.Add("빅 브라더 님이 누군지 알고 싶어?");
				lines.Add("그러면 빅 브라더님이 시험해내는 것들을 모두 풀어봐.");
				lines.Add("그러면 그 분을 알 수 있을거야.");
				lines.Add("그럼 그 시험을 통과하러 갈 준비가 됐어?");
				lines.Add("<준비됐어.>");
				break;

			case NpcId.NPC_002:
				lines.Add("빅 브라더 님은 위대하다!!");
				lines.Add("어때 즐거워?");
				lines.Add("긴장감, 그리고 두뇌가 회전하는 게 느껴지지 않아?");
				lines.Add("이 모든 건 빅브라더님의 뜻이라고~");
				lines.Add("<그 분의 뜻이 뭔데?>");
				lines.Add("그건 끝까지 가보면 알 수 있어.");
				lines.Add("중간에 포기하면 알 수 없을거야. 그 분의 뜻을.");
				lines.Add("걱정은 안 해도 돼.");
				lines.Add("널 행복하게 해줄 거라는 건 분명하니까 걱정하지 말라고~");
				break;

			case NpcId.NPC_003:
				lines.Add("빅 브라더 님은 위대하다!!");
				lines.Add("그거 알아?");
				lines.Add("빅 브라더님은 [      ]이라고도 불려");
				lines.Add("<1) 뭐라고? / 2) 그렇구나.>");
				break;

			case NpcId.NPC_004:
				lines.Add("빅 브라더 님은 위대하다!!");
				lines.Add("넌 곧 빅브라더님의 뜻을 알게 되겠네.");
				lines.Add("어때? 진실을 알 생각에 설레?");
				break;
		}
	}

	// --- 표시 ---
	void ShowCurrentLine()
	{
		if (index >= lines.Count)
		{
			FinishSequence(); // 001/002/004는 여기서 자동 스테이지 저장+로드
			return;
		}

		string raw = lines[index];
		bool isPlayer = IsPlayerLine(raw, out string plain);

		nameLeftBox.gameObject.SetActive(!isPlayer);
		nameRightBox.gameObject.SetActive(isPlayer);
		if (!isPlayer) nameLeft.text = npcDisplayName;
		else nameRight.text = playerDisplayName;

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
		// NPC_003: 선택지 라인(4번째) 완료 후 클릭이면 선택창 오픈
		//if (npc == NpcId.NPC_003 && index == 3 && !isTyping)
		//{
		//	OpenChoice();
		//	return;
		//}

		if (isTyping) ForceComplete();
		else Next();
	}

	// --- 선택지 (NPC_003 전용) ---
	//void OpenChoice()
	//{
	//	SetChoiceVisible(true);
	//	if (choice1Label) choice1Label.text = "1) 뭐라고?";
	//	if (choice2Label) choice2Label.text = "2) 그렇구나.";
	//}

	//void OnChoose(int which)
	//{
	//	SetChoiceVisible(false);

	//	if (which == 1)
	//	{
	//		// 한 줄 더 출력 후 스테이지3로
	//		lines.Insert(index + 1, "1) 빅 브라더님은 [      ]이라고도 불려");
	//		Next(); // 삽입한 줄을 타이핑으로 보여줌 → 끝나면 TypeRichText에서 GoStage(3)
	//	}
	//	else
	//	{
	//		GoStage(3); // 바로 스테이지3
	//	}
	//}

	//void SetChoiceVisible(bool v)
	//{
	//	if (!choiceGroup) return;
	//	choiceGroup.alpha = v ? 1f : 0f;
	//	choiceGroup.interactable = choiceGroup.blocksRaycasts = v;
	//}

	// --- 유틸 ---
	bool IsPlayerLine(string src, out string stripped)
	{
		if (!string.IsNullOrEmpty(src) && src.Length >= 2 && src[0] == '<' && src[^1] == '>')
		{
			stripped = src.Substring(1, src.Length - 2);
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
			// TMP 리치태그는 한 프레임에 통째로
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

		// NPC_003: 1) 선택 후 삽입한 추가 한 줄 출력이 끝났다면 → Stage3
		if (npc == NpcId.NPC_003 && index > 3 && index == lines.Count - 1)
		{
			GoStage(3);
		}
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

		// NPC_001/002/004 → 자동으로 1/2/4 저장 후 MainScene 로드
		int next = GetDefaultNextStageByNpc();
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

	void GoStage(int stageIndex)
	{
		if (isTransitioning) return;
		isTransitioning = true;

		PlayerPrefs.SetInt(KEY_NEXT_STAGE, stageIndex);
		PlayerPrefs.Save();

		SceneManager.LoadScene(stageSceneName);
	}

	int GetDefaultNextStageByNpc()
	{
		switch (npc)
		{
			case NpcId.NPC_001: return 1;
			case NpcId.NPC_002: return 2;
			case NpcId.NPC_004: return 4;
			default: return 0; // NPC_003은 여기 안 옴(선택 로직으로 GoStage 처리)
		}
	}
}
