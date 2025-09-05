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
	[SerializeField] private string playerDisplayName = "��";
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
	//[SerializeField] private Button choice1Button; // 1) �����?
	//[SerializeField] private Button choice2Button; // 2) �׷�����.
	//[SerializeField] private TMP_Text choice1Label;
	//[SerializeField] private TMP_Text choice2Label;

	[Header("Navigation")]
	[SerializeField] private string nextSceneOnFinish = "MainScene"; // 001/002/004 ������ ���� ��
	[SerializeField] private string stageSceneName = "MainScene"; // �������� ���� ��

	// ���� ����
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

	// --- �ζ��� ��� ���� ---
	void BuildLinesInline()
	{
		lines.Clear();
		switch (npc)
		{
			case NpcId.NPC_001:
				lines.Add("�� �������� �����ϴ�!!");
				lines.Add("�ȳ�?");
				lines.Add("�װ� �����ν� �̰��� ��μ� �����̱� �����߳�.");
				lines.Add("�ݰ���.");
				lines.Add("<�̰��� �����?>");
				lines.Add("�̰��� ������.");
				lines.Add("�����Ͻ� �� �������� ��������.");
				lines.Add("�� ���� ���� ����Ͻ� ���̾�.");
				lines.Add("�츮 ��θ� ������ ����� �� �� �ְ� ���ִϱ� ���̾�.");
				lines.Add("<�� ������ �����淡 �׷��°���?>");
				lines.Add("�� ���� ���� ������ �˰� �;�?");
				lines.Add("�׷��� �� �������� �����س��� �͵��� ��� Ǯ���.");
				lines.Add("�׷��� �� ���� �� �� �����ž�.");
				lines.Add("�׷� �� ������ ����Ϸ� �� �غ� �ƾ�?");
				lines.Add("<�غ�ƾ�.>");
				break;

			case NpcId.NPC_002:
				lines.Add("�� ���� ���� �����ϴ�!!");
				lines.Add("� ��ſ�?");
				lines.Add("���尨, �׸��� �γ��� ȸ���ϴ� �� �������� �ʾ�?");
				lines.Add("�� ��� �� ��������� ���̶��~");
				lines.Add("<�� ���� ���� ����?>");
				lines.Add("�װ� ������ ������ �� �� �־�.");
				lines.Add("�߰��� �����ϸ� �� �� �����ž�. �� ���� ����.");
				lines.Add("������ �� �ص� ��.");
				lines.Add("�� �ູ�ϰ� ���� �Ŷ�� �� �и��ϴϱ� �������� �����~");
				break;

			case NpcId.NPC_003:
				lines.Add("�� ���� ���� �����ϴ�!!");
				lines.Add("�װ� �˾�?");
				lines.Add("�� �������� [      ]�̶�� �ҷ�");
				lines.Add("<1) �����? / 2) �׷�����.>");
				break;

			case NpcId.NPC_004:
				lines.Add("�� ���� ���� �����ϴ�!!");
				lines.Add("�� �� ��������� ���� �˰� �ǰڳ�.");
				lines.Add("�? ������ �� ������ ����?");
				break;
		}
	}

	// --- ǥ�� ---
	void ShowCurrentLine()
	{
		if (index >= lines.Count)
		{
			FinishSequence(); // 001/002/004�� ���⼭ �ڵ� �������� ����+�ε�
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
		// NPC_003: ������ ����(4��°) �Ϸ� �� Ŭ���̸� ����â ����
		//if (npc == NpcId.NPC_003 && index == 3 && !isTyping)
		//{
		//	OpenChoice();
		//	return;
		//}

		if (isTyping) ForceComplete();
		else Next();
	}

	// --- ������ (NPC_003 ����) ---
	//void OpenChoice()
	//{
	//	SetChoiceVisible(true);
	//	if (choice1Label) choice1Label.text = "1) �����?";
	//	if (choice2Label) choice2Label.text = "2) �׷�����.";
	//}

	//void OnChoose(int which)
	//{
	//	SetChoiceVisible(false);

	//	if (which == 1)
	//	{
	//		// �� �� �� ��� �� ��������3��
	//		lines.Insert(index + 1, "1) �� �������� [      ]�̶�� �ҷ�");
	//		Next(); // ������ ���� Ÿ�������� ������ �� ������ TypeRichText���� GoStage(3)
	//	}
	//	else
	//	{
	//		GoStage(3); // �ٷ� ��������3
	//	}
	//}

	//void SetChoiceVisible(bool v)
	//{
	//	if (!choiceGroup) return;
	//	choiceGroup.alpha = v ? 1f : 0f;
	//	choiceGroup.interactable = choiceGroup.blocksRaycasts = v;
	//}

	// --- ��ƿ ---
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
			// TMP ��ġ�±״� �� �����ӿ� ��°��
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

		// NPC_003: 1) ���� �� ������ �߰� �� �� ����� �����ٸ� �� Stage3
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

	// --- �� ��ȯ ---
	void FinishSequence()
	{
		if (isTransitioning) return;
		isTransitioning = true;

		// NPC_001/002/004 �� �ڵ����� 1/2/4 ���� �� MainScene �ε�
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
			default: return 0; // NPC_003�� ���� �� ��(���� �������� GoStage ó��)
		}
	}
}
