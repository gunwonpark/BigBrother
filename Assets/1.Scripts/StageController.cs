using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

// 각 글자의 상태를 저장하는 내부 클래스
public class CharInfo
{
    public char Character;
    public bool IsMine;        // 정답 글자 인가
    public bool IsRemoved;     // 제거되었는가
    public bool IsHoveredHint; // 마우스 호버로 인해 강조 상태인가
    public int HintState;      // 0: 기본, 1: 흰색, 2: 회색

    public CharInfo(char character)
    {
        Character = character;
        IsMine = false;
        IsRemoved = false;
        IsHoveredHint = false;
        HintState = 0;
    }
 }

public class StageController : MonoBehaviour
{
    [SerializeField] private InfiniteScroller infiniteScroller; // 무한 스크롤 컴포넌트
    [SerializeField] private TextMeshProUGUI[] sentenceText;    // 무한 스크롤을 위한 텍스트 배열
    [SerializeField] private Camera mainCamera;                 // PostProceesing을 적용할 가능성이 있다
    
    private List<CharInfo> charInfos = new List<CharInfo>();              // 각 글자의 상태 정보 저장 배열
    [SerializeField] private int removableLetterCount;          // 남은 지뢰 개수

    private const float DRAG_THRESHOLD = 10f;  // 드래그로 간주할 최소 픽셀 거리
    private Vector2 mouseDownPosition;         // 마우스를 눌렀을 때의 좌표 저장
    private int potentialClickIndex = -1;      // 클릭 후보가 된 글자의 인덱스

    private int previousHoverIndex = -1;       // 이전 프레임에서 호버했던 글자의 인덱스
    private List<int> highlightedIndices = new List<int>(); // 현재 핑크색으로 강조된 인덱스 리스트

    // 스테이지 데이터로 초기 설정
    public void SetupStage(StageData data)
    {
        string fullSentence = data.FullSentence;
        string answerWord = data.AnswerWord;

        // 문장 전체를 CharInfo 배열로 초기화
        charInfos.Clear();
        charInfos.AddRange(fullSentence.Select(c => new CharInfo(c)));

        // fullSentence에서 answerWord의 글자 위치를 무작위로 매핑한다
        for (int i = 0; i < answerWord.Length; i++)
        {
            char key = answerWord[i];
            if (!char.IsLetter(key)) continue;

            List<int> positions = new List<int>();

            for(int j = 0; j < fullSentence.Length; j++)
            {
                if(fullSentence[j] == key && charInfos[j].IsMine == false)
                {
                    positions.Add(j);
                }
            }

            // 랜덤으로 한가지 선택
            int randomValue = Random.Range(0, positions.Count);

            // 해당 위치를 정답(마인) 글자로 표시
            charInfos[positions[randomValue]].IsMine = true;
        }

        // 제거해야 할 글자 계산
        removableLetterCount = charInfos.Count(item => item.IsMine == false && char.IsLetter(item.Character));

        // 무한 스크롤 설정
        for (int i = 0; i < sentenceText.Length; i++)
        {
            sentenceText[i].text = fullSentence;
        }

        infiniteScroller.gameObject.SetActive(true);
    }

    // 입력 처리
    void Update()
    {
        if (!GameManager.Instance.IsGameActive) return;
        if (infiniteScroller.IsDragging) return;

        // 마우스 호버 처리
        int currentHoverIndex = GetCharacterIndexAt(Input.mousePosition);

        if (currentHoverIndex != previousHoverIndex)
        {
            // 이전에 강조했던 글자들을 모두 원래 상태로 되돌림
            foreach (int index in highlightedIndices)
            {
                if (index >= 0 && index < charInfos.Count)
                    charInfos[index].IsHoveredHint = false;
            }
            highlightedIndices.Clear();

            // 새로 호버한 글자가 유효하다면 (제거되지 않은 알파벳)
            if (currentHoverIndex != -1 &&
                char.IsLetter(charInfos[currentHoverIndex].Character) &&
                !charInfos[currentHoverIndex].IsRemoved)
            {
                // 좌우 2 '글자' 범위의 인덱스를 찾아 리스트에 추가
                highlightedIndices.AddRange(GetLetterIndicesInRange(currentHoverIndex, -1, 2));
                highlightedIndices.AddRange(GetLetterIndicesInRange(currentHoverIndex, 1, 2));

                // 새로 강조할 글자들의 상태를 변경
                foreach (int index in highlightedIndices)
                {
                    charInfos[index].IsHoveredHint = true;
                }
            }

            UpdateDisplayText();
        }

        previousHoverIndex = currentHoverIndex;

        // 마우스 클릭 처리
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            mouseDownPosition = Input.mousePosition;
            potentialClickIndex = GetCharacterIndexAt(mouseDownPosition);
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            if (potentialClickIndex != -1)
            {
                // 누른 위치와 뗀 위치 사이의 거리를 계산
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
            if (char.IsLetter(charInfos[currentIndex].Character) && !charInfos[currentIndex].IsRemoved)
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

    // 좌클릭 로직
    private void OnLeftClick(int index)
    {
        CharInfo info = charInfos[index];
        if (info.IsRemoved || !char.IsLetter(info.Character)) return;
        // 지뢰를 클릭한 경우
        if (info.IsMine)
        {
            // 지뢰는 제거되지 않고, 라이프만 감소
            GameManager.Instance.OnMineClicked();
        }
        // 지뢰가 아닌 글자를 클릭한 경우
        else
        {
            info.IsRemoved = true;
            removableLetterCount--;
            
            UpdateDisplayText();

            // 제거해야 할 글자를 모두 제거했으면 승리
            if (removableLetterCount <= 0)
            {
                GameManager.Instance.StageClear();
            }
        }
    }

    // 우클릭 힌트 로직
    private void OnRightClick(int index)
    {
        CharInfo info = charInfos[index];
       
        if (info.IsMine || info.IsRemoved || !char.IsLetter(info.Character)) return;

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

        // 유효한 범위 내에서 지뢰를 찾았을 경우에만 상태 변경
        if (finalMineRange != -1)
        {
            info.HintState = finalMineRange;
            UpdateDisplayText();
        }
        // 지뢰가 없다면 그 범위의 글자들 제거처리
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

            // 제거해야 할 글자를 모두 제거했으면 승리
            if (removableLetterCount <= 0)
            {
                GameManager.Instance.StageClear();
            }
        }

    }

    private int FindMineInRange(int startIndex, int direction, int maxLetterChecks)
    {
        int sentenceLength = charInfos.Count;
        int lettersChecked = 0; // 검사한 알파벳의 수

        for (int i = 1; i < sentenceLength; i++)
        {
            int currentIndex = (startIndex + (i * direction) + sentenceLength) % sentenceLength;
            CharInfo currentInfo = charInfos[currentIndex];

            // 현재 위치가 알파벳일 경우에만 카운트
            if (char.IsLetter(currentInfo.Character))
            {
                lettersChecked++;

                // 해당 알파벳이 지뢰라면, 몇 번째 알파벳인지 반환
                if (currentInfo.IsMine)
                {
                    return lettersChecked;
                }

                // 최대 검사 횟수를 넘었으면 탐색 중지
                if (lettersChecked >= maxLetterChecks)
                {
                    return -1;
                }
            }
        }

        // 문장 전체를 다 돌아도 못 찾았으면 -1 반환
        return -1;
    }

    // 화면 업데이트
    void UpdateDisplayText()
    {
        // 이 함수는 변경할 필요 없이 그대로 작동합니다.
        StringBuilder sb = new StringBuilder();
        foreach (CharInfo info in charInfos)
        {
            string finalTag = "<color=red>";
            if (info.HintState == 1) finalTag = "<color=white>";
            else if (info.HintState == 2) finalTag = "<color=#8C8C8C>";
            if (info.IsRemoved) finalTag = "<color=black>";
            if (info.IsHoveredHint) finalTag = "<color=#FF6969>"; // 핑크색 강조

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
}