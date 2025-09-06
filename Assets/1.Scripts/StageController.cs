using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

// �� ������ ���¸� �����ϴ� ���� Ŭ����
public class CharInfo
{
    public char Character;
    public bool IsMine;        // ���� ���� �ΰ�
    public bool IsRemoved;     // ���ŵǾ��°�
    public bool IsHoveredHint; // ���콺 ȣ���� ���� ���� �����ΰ�
    public int HintState;      // 0: �⺻, 1: ���, 2: ȸ��
    public bool CanClicked;    // Ŭ�� ������ �����ΰ�(Ʃ�丮���)
    public bool CanRightClicked; 
    public bool CanLeftClicked;
    public CharInfo(char character)
    {
        Character = character;
        IsMine = false;
        IsRemoved = false;
        IsHoveredHint = false;
        CanClicked = true;
        CanRightClicked = true;
        CanLeftClicked = true;
        HintState = 0;
    }
 }

public class StageController : MonoBehaviour
{
    [SerializeField] private InfiniteScroller infiniteScroller; // ���� ��ũ�� ������Ʈ
    [SerializeField] private TextMeshProUGUI[] sentenceText;    // ���� ��ũ���� ���� �ؽ�Ʈ �迭
    [SerializeField] private Camera mainCamera;                 // PostProceesing�� ������ ���ɼ��� �ִ�

    private List<CharInfo> charInfos = new List<CharInfo>();              // �� ������ ���� ���� ���� �迭
    [SerializeField] private int removableLetterCount;          // ���� ���� ����

    private const float DRAG_THRESHOLD = 10f;  // �巡�׷� ������ �ּ� �ȼ� �Ÿ�
    private Vector2 mouseDownPosition;         // ���콺�� ������ ���� ��ǥ ����
    private int potentialClickIndex = -1;      // Ŭ�� �ĺ��� �� ������ �ε���

    private int previousHoverIndex = -1;       // ���� �����ӿ��� ȣ���ߴ� ������ �ε���
    private List<int> highlightedIndices = new List<int>(); // ���� ��ũ������ ������ �ε��� ����Ʈ

    // �������� �����ͷ� �ʱ� ����
    public void SetupStage(StageData data)
    {
        string fullSentence = data.FullSentence;
        string answerWord = data.AnswerWord;

        // ���� ��ü�� CharInfo �迭�� �ʱ�ȭ
        charInfos.Clear();
        charInfos.AddRange(fullSentence.Select(c => new CharInfo(c)));

        // Ʃ�丮�� �ܰ��� ��� ���Ƿ� ������ ������ ����Ѵ�
        if (DataManager.Instance.CurrentWorldLevel == 0)
        {
            // ��ü Ŭ�� ���
            charInfos.ForEach(c => { c.CanClicked = false; c.CanLeftClicked = false; c.CanRightClicked = false; });

            // 5, 7, 13, 16 ,17 ��° ���� �������� ����
            charInfos[5].IsMine = true;
            charInfos[7].IsMine = true;
            charInfos[13].IsMine = true;
            charInfos[16].IsMine = true;
            charInfos[17].IsMine = true;
        }
        else
        {
            // fullSentence���� answerWord�� ���� ��ġ�� �������� �����Ѵ�
            for (int i = 0; i < answerWord.Length; i++)
            {
                char key = answerWord[i];
                if (!char.IsLetter(key)) continue;

                List<int> positions = new List<int>();

                for (int j = 0; j < fullSentence.Length; j++)
                {
                    if (fullSentence[j] == key && charInfos[j].IsMine == false)
                    {
                        positions.Add(j);
                    }
                }

                // �������� �Ѱ��� ����
                int randomValue = Random.Range(0, positions.Count);

                // �ش� ��ġ�� ����(����) ���ڷ� ǥ��
                charInfos[positions[randomValue]].IsMine = true;
            }
        }
            
        // �����ؾ� �� ���� ���
        removableLetterCount = charInfos.Count(item => item.IsMine == false && char.IsLetter(item.Character));

        // ���� ��ũ�� ����
        for (int i = 0; i < sentenceText.Length; i++)
        {
            sentenceText[i].text = fullSentence;
        }

        infiniteScroller.gameObject.SetActive(true);
    }

    // �Է� ó��
    void Update()
    {
        if (!GameManager.Instance.IsGameActive) return;
        if (infiniteScroller.IsDragging) return;

        // ���콺 ȣ�� ó��
        int currentHoverIndex = GetCharacterIndexAt(Input.mousePosition);

        if (currentHoverIndex != previousHoverIndex && currentHoverIndex != -1 && charInfos[currentHoverIndex].CanClicked)
        {
            // ������ �����ߴ� ���ڵ��� ��� ���� ���·� �ǵ���
            foreach (int index in highlightedIndices)
            {
                if (index >= 0 && index < charInfos.Count)
                    charInfos[index].IsHoveredHint = false;
            }

            highlightedIndices.Clear();

            // ���� ȣ���� ���ڰ� ��ȿ�ϴٸ� (���ŵ��� ���� ���ĺ�)
            if (currentHoverIndex != -1 &&
                char.IsLetter(charInfos[currentHoverIndex].Character) &&
                !charInfos[currentHoverIndex].IsRemoved)
            {
                // �¿� 2 '����' ������ �ε����� ã�� ����Ʈ�� �߰�
                highlightedIndices.AddRange(GetLetterIndicesInRange(currentHoverIndex, -1, 2));
                highlightedIndices.AddRange(GetLetterIndicesInRange(currentHoverIndex, 1, 2));

                // ���� ������ ���ڵ��� ���¸� ����
                foreach (int index in highlightedIndices)
                {
                    charInfos[index].IsHoveredHint = true;
                }
            }

            UpdateDisplayText();
        }

        previousHoverIndex = currentHoverIndex;

        // ���콺 Ŭ�� ó��
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            mouseDownPosition = Input.mousePosition;
            potentialClickIndex = GetCharacterIndexAt(mouseDownPosition);
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            if (potentialClickIndex != -1)
            {
                // ���� ��ġ�� �� ��ġ ������ �Ÿ��� ���
                float mouseDragDistance = Vector2.Distance(mouseDownPosition, Input.mousePosition);

                if (mouseDragDistance < DRAG_THRESHOLD)
                {
                    if (Input.GetMouseButtonUp(0)) OnLeftClick(potentialClickIndex);
                    else if (Input.GetMouseButtonUp(1)) OnRightClick(potentialClickIndex);
                }
            }

            potentialClickIndex = -1;
        }
    }

    private List<int> GetLetterIndicesInRange(int startIndex, int direction, int maxLetters)
    {
        var indices = new List<int>();
        int sentenceLength = charInfos.Count;
        int lettersFound = 0;

        for (int i = 1; i < sentenceLength; i++)
        {
            int currentIndex = (startIndex + (i * direction) + sentenceLength) % sentenceLength;
            if (char.IsLetter(charInfos[currentIndex].Character))
            {
                lettersFound++;
                indices.Add(currentIndex);
                if (lettersFound >= maxLetters)
                {
                    break;
                }
            }
        }
        return indices;
    }

    private int GetCharacterIndexAt(Vector2 position)
    {
        for (int i = 0; i < sentenceText.Length; i++)
        {
            int charIndex = TMP_TextUtilities.FindIntersectingCharacter(sentenceText[i], position, null, true);
            if (charIndex != -1)
            {
                return charIndex < charInfos.Count ? charIndex : -1;
            }
        }

        return -1;
    }

    // ��Ŭ�� ����
    private void OnLeftClick(int index)
    {
        CharInfo info = charInfos[index];
        if (info.IsRemoved || !char.IsLetter(info.Character) || info.CanClicked == false || info.CanLeftClicked == false) return;
        // ���ڸ� Ŭ���� ���
        if (info.IsMine)
        {
            // ���ڴ� ���ŵ��� �ʰ�, �������� ����
            GameManager.Instance.OnMineClicked();
        }
        // ���ڰ� �ƴ� ���ڸ� Ŭ���� ���
        else
        {
            DataManager.Instance.IsTextClicked = true;
            info.IsRemoved = true;
            removableLetterCount--;
            
            UpdateDisplayText();

            // �����ؾ� �� ���ڸ� ��� ���������� �¸�
            if (removableLetterCount <= 0)
            {
                GameManager.Instance.StageClear();
            }
        }
    }

    // ��Ŭ�� ��Ʈ ����
    private void OnRightClick(int index)
    {
        CharInfo info = charInfos[index];
       
        if (info.IsMine || info.IsRemoved || !char.IsLetter(info.Character) || GameManager.Instance.RemainHintCount <= 0 || info.CanClicked == false || info.CanRightClicked == false) return;

        GameManager.Instance.RemainHintCount--;
        DataManager.Instance.IsTextClicked = true;

        int leftMineRange = FindMineInRange(index, -1, 2);
        
        int rightMineRange = FindMineInRange(index, 1, 2);

        int finalMineRange = -1;

        if (leftMineRange != -1 && rightMineRange != -1)
        {
            finalMineRange = Mathf.Min(leftMineRange, rightMineRange);
        }
        
        else if (leftMineRange != -1)
        {
            finalMineRange = leftMineRange;
        }
        else if (rightMineRange != -1)
        {
            finalMineRange = rightMineRange;
        }

        // ��ȿ�� ���� ������ ���ڸ� ã���� ��쿡�� ���� ����
        if (finalMineRange != -1)
        {
            info.HintState = finalMineRange;
            UpdateDisplayText();
        }
        // ���ڰ� ���ٸ� �� ������ ���ڵ� ����ó��
        else
        {
            charInfos[index].IsRemoved = true;
            removableLetterCount--;

            foreach (int idx in GetLetterIndicesInRange(index, -1, 2))
            {
                charInfos[idx].IsRemoved = true;
                removableLetterCount--;
            }

            foreach (int idx in GetLetterIndicesInRange(index, 1, 2))
            {
                charInfos[idx].IsRemoved = true;
                removableLetterCount--;
            }

            UpdateDisplayText();

            // �����ؾ� �� ���ڸ� ��� ���������� �¸�
            if (removableLetterCount <= 0)
            {
                GameManager.Instance.StageClear();
            }
        }

    }

    private int FindMineInRange(int startIndex, int direction, int maxLetterChecks)
    {
        int sentenceLength = charInfos.Count;
        int lettersChecked = 0; // �˻��� ���ĺ��� ��

        for (int i = 1; i < sentenceLength; i++)
        {
            int currentIndex = (startIndex + (i * direction) + sentenceLength) % sentenceLength;
            CharInfo currentInfo = charInfos[currentIndex];

            // ���� ��ġ�� ���ĺ��� ��쿡�� ī��Ʈ
            if (char.IsLetter(currentInfo.Character))
            {
                lettersChecked++;

                // �ش� ���ĺ��� ���ڶ��, �� ��° ���ĺ����� ��ȯ
                if (currentInfo.IsMine)
                {
                    return lettersChecked;
                }

                // �ִ� �˻� Ƚ���� �Ѿ����� Ž�� ����
                if (lettersChecked >= maxLetterChecks)
                {
                    return -1;
                }
            }
        }

        // ���� ��ü�� �� ���Ƶ� �� ã������ -1 ��ȯ
        return -1;
    }

    // ȭ�� ������Ʈ
    private void UpdateDisplayText()
    {
        // �� �Լ��� ������ �ʿ� ���� �״�� �۵��մϴ�.
        StringBuilder sb = new StringBuilder();
        foreach (CharInfo info in charInfos)
        {
            string finalTag = "<color=red>";
            if (info.HintState == 1) finalTag = "<color=white>";
            else if (info.HintState == 2) finalTag = "<color=#8C8C8C>";
            if (info.IsRemoved) finalTag = "<color=black>";
            else if (info.IsHoveredHint) finalTag = "<color=#FF6969>"; // ��ũ�� ����

            sb.Append(finalTag);
            sb.Append(info.Character);
            sb.Append("</color>");
        }

        string richTextResult = sb.ToString();
        foreach (var textUI in sentenceText)
        {
            textUI.text = richTextResult;
        }
    }

    // Ʃ�丮�� ��
    public void EnableRightClick()
    {
        charInfos[6].CanClicked = true;
        charInfos[6].CanRightClicked = true;
    }

    public void DisableRightClick()
    {
        charInfos[6].CanClicked = false;
        charInfos[6].CanRightClicked = false;
    }

    public void EnableLeftClick()
    {
        charInfos[15].CanClicked = true;
        charInfos[15].CanLeftClicked = true;
    }

    public void DisableLeftClick()
    {
        charInfos[15].CanClicked = false;
        charInfos[15].CanLeftClicked = false;
    }

    public void EnableAllClick()
    {
        charInfos.ForEach(c => { c.CanClicked = true; c.CanLeftClicked = true; c.CanRightClicked = true; });
    }
}