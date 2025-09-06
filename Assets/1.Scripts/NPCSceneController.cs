using UnityEngine;

public class NPCSceneController : MonoBehaviour
{
	private const string KEY_NEXT_NPC = "NextNpc";

	[Tooltip("순서: 1=틸로슨, 2=시안, 3=텔레스크린, 4=채링턴")]
	[SerializeField] private GameObject[] npcRoots;

	[Tooltip("에디터 단독 재생 시 사용할 기본 NPC 번호 (1~4)")]
	[SerializeField] private int fallbackNpcNumber = 1;

	private void Awake()
	{
		// 1) 이전 씬에서 넘어온 다음 NPC 번호(1~4) 꺼내기. 없으면 fallback 사용
		int npcNum = Mathf.Clamp(
			PlayerPrefs.GetInt(KEY_NEXT_NPC, fallbackNpcNumber),
			1, npcRoots != null ? npcRoots.Length : 4
		);

		// “키가 있다면 즉시 지운다” → 이 값은 일회성 토큰처럼 소비
		// 같은 씬을 재시작 / 재입장했을 때 이전 선택이 남아서 또 적용되는 일을 막기
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
