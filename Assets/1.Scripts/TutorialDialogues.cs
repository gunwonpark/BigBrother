using UnityEngine;

[System.Serializable]
public class TutorialDialogue
{
    public int groupIndex;
    public string[] dialogues;
}

[CreateAssetMenu(fileName = "TutorialDialogues", menuName = "ScriptableObjects/TutorialDialogues", order = 1)]
public class TutorialDialogues : ScriptableObject
{
    public TutorialDialogue[] dialogues;
}