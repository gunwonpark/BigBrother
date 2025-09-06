using UnityEngine;

public class NPCSceneController : MonoBehaviour
{
	[Tooltip("����: 1=ƿ�ν�, 2=�þ�, 3=�ڷ���ũ��, 4=ä����")]
	[SerializeField] private GameObject[] npcRoots;

	private void Awake()
	{
		// 1) ���� ������ �Ѿ�� ���� NPC ��ȣ(1~4) ������.
		int npcNum = DataManager.Instance.CurrentWorldLevel;

		if (npcRoots == null) return;

		for (int i = 0; i < npcRoots.Length; i++)
		{
			if (!npcRoots[i]) continue;
			bool shouldActive = (i + 1) == npcNum;
			npcRoots[i].SetActive(shouldActive);
		}
	}
}
