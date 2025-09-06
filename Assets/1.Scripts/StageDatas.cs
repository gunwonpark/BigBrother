using UnityEngine;

[System.Serializable]
public class StageData
{
    public string FullSentence;     // 전체 문장
    public string AnswerWord;       // 정답 단어
    public string KoreanSentence;   // 한글 문장
    public int HintCount;           // 힌트 개수
    public int AnswerCount;         // 정답 개수

}

[CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObjects/StageData", order = 1)]
public class StageDatas : ScriptableObject
{
    public System.Collections.Generic.List<StageData> stageDatas = new System.Collections.Generic.List<StageData>();
}

