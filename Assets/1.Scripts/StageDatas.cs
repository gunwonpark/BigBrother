using UnityEngine;

[System.Serializable]
public class StageData
{
    public string FullSentence;     // ��ü ����
    public string AnswerWord;       // ���� �ܾ�
}

[CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObjects/StageData", order = 1)]
public class StageDatas : ScriptableObject
{
    public System.Collections.Generic.List<StageData> stageDatas = new System.Collections.Generic.List<StageData>();
}

