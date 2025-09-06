using UnityEngine;

public class NPCSceneController : MonoBehaviour
{
	private const string KEY_NEXT_NPC = "NextNpc";

	[Tooltip("����: 1=ƿ�ν�, 2=�þ�, 3=�ڷ���ũ��, 4=ä����")]
	[SerializeField] private GameObject[] npcRoots;

	[Tooltip("������ �ܵ� ��� �� ����� �⺻ NPC ��ȣ (1~4)")]
	[SerializeField] private int fallbackNpcNumber = 1;

	private void Awake()
	{
		// 1) ���� ������ �Ѿ�� ���� NPC ��ȣ(1~4) ������. ������ fallback ���
		int npcNum = Mathf.Clamp(
			PlayerPrefs.GetInt(KEY_NEXT_NPC, fallbackNpcNumber),
			1, npcRoots != null ? npcRoots.Length : 4
		);

		// ��Ű�� �ִٸ� ��� ����١� �� �� ���� ��ȸ�� ��ūó�� �Һ�
		// ���� ���� ����� / ���������� �� ���� ������ ���Ƽ� �� ����Ǵ� ���� ����
		if (PlayerPrefs.HasKey(KEY_NEXT_NPC))
			PlayerPrefs.DeleteKey(KEY_NEXT_NPC);

		if (npcRoots == null) return;

		for (int i = 0; i < npcRoots.Length; i++)
		{
			if (!npcRoots[i]) continue;
			bool shouldActive = (i + 1) == npcNum;
			npcRoots[i].SetActive(shouldActive);
		}
	}
}
