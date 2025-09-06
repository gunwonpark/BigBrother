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
	// 001=ƿ�ν�, 002=�þ�, 003=�ڷ���ũ��, 004=ä����

	[Header("Which NPC")]
	[SerializeField] private NpcId npc = NpcId.NPC_001;

	[Header("Names / Typing")]
	[SerializeField] private string npcDisplayName = "NPC";
	[SerializeField] private string playerDisplayName = "��";
	[SerializeField] private float charDelay = 0.05f;
	[SerializeField] private float punctuationHold = 0.08f;
	[SerializeField] private bool usePunctuationHold = true;

	[Header("UI Refs")]
	[SerializeField] private RectTransform talkRowRoot; // �� �̸�+��� �θ�(�¿� ���� ���)
	[SerializeField] private TMP_Text nameLeft;         // �� �׻� ���� �ϳ��� ���
	[SerializeField] private TMP_Text dialogueText;
	[SerializeField] private Button backgroundClick;    // ȭ�� ��ü Ŭ�� ĳ��

	[Header("Choice (Text Hover)")]
	[SerializeField] private CanvasGroup choiceGroup;   // ������ �׷�
	[SerializeField] private ChoiceHoverText choice1Text;
	[SerializeField] private ChoiceHoverText choice2Text;

	[Header("Navigation")]
	[SerializeField] private string nextSceneOnFinish = "MainScene"; // ������ ���� �� (�������� ����)

	// ���� ����
	private readonly List<string> lines = new();
	private int index;
	private bool isTyping;
	private Coroutine typingCo;
	private string currentFullRich;
	private bool isTransitioning;

	// ������ Ʈ���� �ε���(������ -1)
	private int choiceTriggerIndex = -1;

	// Choice�� �޼��� ĳ��
	private void OnChoose1() => OnChoose(1);
	private void OnChoose2() => OnChoose(2);

	void Awake()
	{
		// �⺻ �̸� �ڵ� ����(����θ�)
		if (string.IsNullOrWhiteSpace(npcDisplayName))
		{
			npcDisplayName = npc switch
			{
				NpcId.NPC_001 => "ƿ�ν�",
				NpcId.NPC_002 => "�þ�",
				NpcId.NPC_003 => "�ڷ���ũ��",
				NpcId.NPC_004 => "ä����",
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

	// --- �ζ��� ���(�� ��ȹ) ---
	void BuildLinesInline()
	{
		lines.Clear();

		switch (npc)
		{
			// [ƿ�ν�] �� ������ Stage 1
			case NpcId.NPC_001:
				lines.Add("�� ���� ���� �����ϴ�!!");
				// (���⼭ ������ ���� ����)
				lines.Add("<�׷� �� ��� �� �� �������� ������.>");
				lines.Add("�׳����� ���� ����?");
				lines.Add("<�� ���ʿ� ��� ������ �־��>");
				lines.Add("�׷� �� �ʿ� ��� ������ ���� ���� �ִ����.");
				lines.Add("�� �߿��� �� �ֳ� ������.");
				lines.Add("<(���� [          ]�� ���ܵ� ��ȣ�� ��Ű�� �ڵ��ΰ�����. ���� �ʰڴ°�.)>");
				lines.Add("<�׷���. �ٵ� ����Ͻó�.>");
				lines.Add("�׷�. �� ��� �� �� �������� ���Ͽ�.");
				break;

			// [�þ�] �� ������ Stage 2
			case NpcId.NPC_002:
				lines.Add("���� �������� �������� ó�ϴ� �� �ó�?");
				// (���⼭ ������ ���� ����)
				lines.Add("�׷� ���� �� �������� ���� �ſ��ϴ� �̵��̶��");
				lines.Add("�������� ���ص� ���ڸ��� �ʾ� ����.");
				lines.Add("�׷��� ������?");
				lines.Add("<�׷���.>");
				lines.Add("<(������ �츮 ������� �� �װ� ���ұ���.)>");
				lines.Add("�׳����� ��� ���ٿԱ淡 �׸� �� �����ΰ�?");
				lines.Add("�� ��� ��̶� �ϰ� �Դ°�?");
				lines.Add("<�ƹ��͵� �ƴҼ�.>");
				lines.Add("�ý��ϱ�. �ƹ�ư �� ��� �� �� �������� ���Ͽ�.");
				break;

			// [�ڷ���ũ��] �� ������ ����, ������ Stage 3
			case NpcId.NPC_003:
				lines.Add("�۳� ���, ��� �ķ��� 15% �����Ͽ����ϴ�.");
				lines.Add("�� �����Բ��� �����Ͻ� �������� ���� ���п� ����� ���Ӱ� �ູ�� ���� ��ư��硦");
				lines.Add("<���Ӱ� �ູ�� ���̶�� ���� �� ��° ������ ����.>");
				lines.Add("<ǳ��ΰ� ���� ���� ���� �������� ���̱�.>");
				lines.Add("�����Ͻ� �� �����Բ����� ������ �����ϱ� ���� ���ο� ������ �����ϼ̽��ϴ�.");
				lines.Add("������� ���� �ùķ��̼��� �����Ͽ�, ���� ��Ȳ������ �� �����Կ� ���� �漺���� �����Ǵ����� Ȯ���ϰ�");
				lines.Add("<���� ���� �ڽ��� �漺���� Ȯ���ϱ� ���� ���õ���� ������� �����ϴ±���.>");
				lines.Add("<���� ���������ϴ�.>");
				break;

			// [ä����] �� ������ Stage 4
			case NpcId.NPC_004:
				lines.Add("��");
				lines.Add("�ʴ� �ʸ� �ϳ�? ���� ������ �ϳĴ� ���̴�.");
				// (���⼭ ������ ���� ����)
				lines.Add("���� �Ƶ� ���� ������ �����ϰڴ�.");
				lines.Add("�� ��� ���� �װ� ��������� �ϴ� ��.");
				break;
		}
	}

	// ������ Ʈ���� ��ġ(��ȹ ����)
	void SetChoiceTriggerByDesign()
	{
		choiceTriggerIndex = npc switch
		{
			NpcId.NPC_001 => 0, // ƿ�ν�: ù �� ��
			NpcId.NPC_002 => 0, // �þ�: ù �� ��
			NpcId.NPC_003 => -1, // �ڷ���ũ��: ������ ����
			NpcId.NPC_004 => 1, // ä����: �� ��° �� ��
			_ => -1
		};
	}

	// --- ǥ�� ---
	void ShowCurrentLine()
	{
		if (index >= lines.Count)
		{
			FinishSequence();
			return;
		}

		string raw = lines[index];
		bool isPlayer = IsPlayerLine(raw, out string plain);

		// �̸��� �׻� ���� �ؽ�Ʈ �ϳ����� ǥ��
		if (nameLeft) nameLeft.text = isPlayer ? playerDisplayName : npcDisplayName;

		// �÷��̾� ����� ���� ���� �¿� ����(�ؽ�Ʈ�� ������ ����)
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
		// ������ Ʈ���� �������� Ŭ�� �� ������ ����
		if (choiceTriggerIndex >= 0 && index == choiceTriggerIndex && !isTyping)
		{
			OpenChoice();
			return;
		}

		if (isTyping) ForceComplete();
		else Next();
	}

	// === ������ ===
	void OpenChoice()
	{
		// ��� ��ħ ����
		if (dialogueText) dialogueText.text = string.Empty;

		// NPC�� ������ ��
		if (choice1Text && choice2Text)
		{
			switch (npc)
			{
				case NpcId.NPC_001: // ƿ�ν�
					choice1Text.SetText("1) �����?");
					choice2Text.SetText("2) ��������� �����ϴ�!!");
					break;
				case NpcId.NPC_002: // �þ�
					choice1Text.SetText("1) ���� �߾���. ��ȭ�� �� �� �ְ���.");
					choice2Text.SetText("2) ���� ������ �������̾���.");
					break;
				case NpcId.NPC_004: // ä����
					choice1Text.SetText("1) �׷�");
					choice2Text.SetText("2) �ƴ�");
					break;
			}
		}

		SetChoiceVisible(true);
	}

	void OnChoose(int which)
	{
		SetChoiceVisible(false);

		// ���ÿ� ���� 1�� ����(�����)
		string insert = GetInsertedLineForChoice(which);
		if (!string.IsNullOrEmpty(insert))
		{
			lines.Insert(index + 1, insert);
			Next(); // ������ �� ���
		}
		else
		{
			// ������ ���� ���ٸ� �ٷ� ����
			Next();
		}
	}

	string GetInsertedLineForChoice(int which)
	{
		switch (npc)
		{
			case NpcId.NPC_001: // ƿ�ν�: ���� �� '�÷��̾�' ���� ����
				return which == 1
					? "<�����?>"
					: "<��������� �����ϴ�!!>";

			case NpcId.NPC_002: // �þ�: �÷��̾� ����
				return which == 1
					? "<���� �߾���. ��ȭ�� �� �� �ְ���.>"
					: "<���� ������ �������̾���.>";

			case NpcId.NPC_004: // ä����: �÷��̾� ����
				return which == 1 ? "<�׷�>" : "<�ƴ�>";
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
		if (backgroundClick) backgroundClick.interactable = !v; // ������ ������ ��� Ŭ�� ����
	}

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

	// --- �� ��ȯ ---
	void FinishSequence()
	{
		if (isTransitioning) return;
		isTransitioning = true;

		int next = GetDefaultNextStageByNpc(); // 1/2/3/4 ���� ����
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
			case NpcId.NPC_001: return 1; // ƿ�ν�
			case NpcId.NPC_002: return 2; // �þ�
			case NpcId.NPC_003: return 3; // �ڷ���ũ��
			case NpcId.NPC_004: return 4; // ä����
			default: return 0;
		}
	}

	// === �¿� ����(�÷��̾� ��� ����) ===
	void SetFlipped(bool flipped)
	{
		// �θ� �����̳ʴ� �¿� ����
		if (talkRowRoot)
		{
			var s = talkRowRoot.localScale;
			s.x = flipped ? -1f : 1f;
			talkRowRoot.localScale = s;
		}

		// �ؽ�Ʈ�� �ٽ� �� �� �����ؼ� ���ڰ� �Ųٷ� ������ �ʰ�
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
