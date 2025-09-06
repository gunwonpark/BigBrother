using UnityEngine;

public class NPCSceneController : MonoBehaviour
{
	[Tooltip("순서: 1=틸로슨, 2=시안, 3=텔레스크린, 4=채링턴")]
	[SerializeField] private GameObject[] npcRoots;

	private void Awake()
	{
		// 1) 이전 씬에서 넘어온 다음 NPC 번호(1~4) 꺼내기.
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
