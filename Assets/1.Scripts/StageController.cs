using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class CharInfo
{
    public char Character;
    public bool IsMine;        
    public bool IsRemoved;     
    public bool IsHoveredHint;       
    public bool IsHintClicked;
    public bool IsChecked;
    public int HintState; // 0: 없음, 1: 흰색, 2: 회색, 3: 번갈아서 깜빡임

    // 튜토리얼용 제어
    public bool CanClicked;    
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
        IsHintClicked = false;
        IsChecked = false;
        HintState = 0;
    }
 }

public class StageController : MonoBehaviour
{
    [SerializeField] private GameObject HintImage;
    [SerializeField] private RectTransform[] hintImages;

    [SerializeField] private InfiniteScroller infiniteScroller; 
    [SerializeField] private TextMeshProUGUI[] sentenceText;    
    [SerializeField] private Camera mainCamera;

    [SerializeField] private float blinkSpeed = 2f;
    private bool isBlinkingActive = false;

    private List<CharInfo> charInfos = new List<CharInfo>();              
    [SerializeField] private int removableLetterCount;          

    private const float DRAG_THRESHOLD = 10f; 
    private Vector2 mouseDownPosition;         
    private int potentialClickIndex = -1;     

    private int previousHoverIndex = -1;     
    private List<int> highlightedIndices = new List<int>(); 

    public void SetupStage(StageData data)
    {
        string fullSentence = data.FullSentence;
        string answerWord = data.AnswerWord;

        charInfos.Clear();
        charInfos.AddRange(fullSentence.Select(c => new CharInfo(c)));

        hintImages = new RectTransform[charInfos.Count * 3];
        
        isBlinkingActive = false;

        for (int i = 0; i < charInfos.Count; i++)
        {
            if (char.IsLetter(charInfos[i].Character))
            {
                for (int j = 0; j < 3; j++)
                {
                    RectTransform hint = Instantiate(HintImage).GetComponent<RectTransform>();
                    hintImages[i + j * charInfos.Count] = hint;
                    hint.gameObject.SetActive(false);
                    hint.GetComponent<Image>().color = Color.white;
                }
            }
        }

        if (DataManager.Instance.NeedTutorial)
        {
            charInfos.ForEach(c => { c.CanClicked = false; c.CanLeftClicked = false; c.CanRightClicked = false; });

            
            charInfos[5].IsMine = true;
            charInfos[7].IsMine = true;
            charInfos[13].IsMine = true;
            charInfos[16].IsMine = true;
            charInfos[17].IsMine = true;
        }
        else
        {
            
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

                int randomValue = Random.Range(0, positions.Count);

               
                charInfos[positions[randomValue]].IsMine = true;
            }
        }
            
      
        removableLetterCount = charInfos.Count(item => item.IsMine == false && char.IsLetter(item.Character));

        
        for (int i = 0; i < sentenceText.Length; i++)
        {
            sentenceText[i].text = fullSentence;
        }

        infiniteScroller.gameObject.SetActive(true);
    }

    
    void Update()
    {
        if (!GameManager.Instance.IsGameActive) return;
        if (infiniteScroller.IsDragging) return;

        if (isBlinkingActive)
        {
            UpdateDisplayText();
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if(results.Count > 0)
        {
            bool isTextHit = false;
            for(int i = 0; i < sentenceText.Length; i++)
            {
                if (results[0].gameObject == sentenceText[i].gameObject)
                {
                    isTextHit = true;
                    break;
                }
            }
            if (isTextHit == false)
            {
                if (highlightedIndices.Count > 0)
                {
                    foreach (int index in highlightedIndices)
                    {
                        if (index >= 0 && index < charInfos.Count)
                            charInfos[index].IsHoveredHint = false;
                    }
                    highlightedIndices.Clear();
                    UpdateDisplayText();
                }
                return;
            }
        }

        int currentHoverIndex = GetCharacterIndexAt(Input.mousePosition);

        if (currentHoverIndex != previousHoverIndex && currentHoverIndex != -1 && charInfos[currentHoverIndex].CanClicked)
        {
           
            foreach (int index in highlightedIndices)
            {
                if (index >= 0 && index < charInfos.Count)
                    charInfos[index].IsHoveredHint = false;
            }

            highlightedIndices.Clear();

            
            if (currentHoverIndex != -1 &&
                char.IsLetter(charInfos[currentHoverIndex].Character) &&
                !charInfos[currentHoverIndex].IsRemoved)
            {
            
                highlightedIndices.AddRange(GetLetterIndicesInRange(currentHoverIndex, -1, 2));
                highlightedIndices.AddRange(GetLetterIndicesInRange(currentHoverIndex, 1, 2));

            
                foreach (int index in highlightedIndices)
                {
                    charInfos[index].IsHoveredHint = true;
                }
            }

            UpdateDisplayText();
        }

        previousHoverIndex = currentHoverIndex;

       
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            mouseDownPosition = Input.mousePosition;
            potentialClickIndex = GetCharacterIndexAt(mouseDownPosition);
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            if (potentialClickIndex != -1)
            {
            
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

    private void OnLeftClick(int index)
    {
        CharInfo info = charInfos[index];
        if (info.IsRemoved || !char.IsLetter(info.Character) || info.CanClicked == false || info.CanLeftClicked == false) return;

        SoundManager.Instance.Play("left_click", Sound.Effect);
        
        if (info.IsMine)
        {            
            GameManager.Instance.OnMineClicked();
        }
        else
        {
            DataManager.Instance.IsTextClicked = true;
            info.IsRemoved = true;
            removableLetterCount--;
            
            UpdateDisplayText();

            if (removableLetterCount <= 0)
            {
                GameManager.Instance.StageClear();
            }
        }
    }

    private void OnRightClick(int index)
    {
        CharInfo info = charInfos[index];
       
        if (info.IsChecked || info.IsRemoved || !char.IsLetter(info.Character) || info.CanClicked == false || info.CanRightClicked == false) return;

        if(Input.GetKey(KeyCode.LeftControl))
        {
            if (info.IsHintClicked)
            {
                info.IsHintClicked = false;
                for (int i = 0; i < sentenceText.Count(); i++)
                {
                    hintImages[index + i * charInfos.Count].gameObject.SetActive(false);
                }
            }
            else
            {
                info.IsHintClicked = true;

                ShowHintImage(index);
            }
            return;
        }

        if (GameManager.Instance.RemainHintCount <= 0) return;
        

        SoundManager.Instance.Play("right_click", Sound.Effect);

        DataManager.Instance.IsTextClicked = true;

        // 정답인 경우 노란색힌트로 표시한다
        if(info.IsMine)
        {
            info.IsHintClicked = true;
            info.IsChecked = true;

            ShowHintImage(index);

            for (int i = 0; i < sentenceText.Count(); i++)
            {
                GameObject hintImage = hintImages[index + i * charInfos.Count].gameObject;
                hintImage.GetComponent<Image>().color = new Color(1f, 0.92f, 0.016f); // 노란색으로 변경
            }

            return;
        }

        GameManager.Instance.RemainHintCount--;

        List<int> leftMineDistances = FindMinesInDirection(index, -1);
        List<int> rightMineDistances = FindMinesInDirection(index, 1);

        // 양쪽 또는 한쪽 1, 2칸에 지뢰가 있는 경우 깜빡임 상태로 설정
        bool isBlinkingCondition = (leftMineDistances.Contains(1) && leftMineDistances.Contains(2)) ||
                                   (rightMineDistances.Contains(1) && rightMineDistances.Contains(2)) ||
                                   (leftMineDistances.Contains(1) && rightMineDistances.Contains(2)) ||
                                   (leftMineDistances.Contains(2) && rightMineDistances.Contains(1));


        if (isBlinkingCondition)
        {
            info.HintState = 3; // 깜빡임 상태로 설정
            isBlinkingActive = true; // Update에서 감지하도록 플래그 설정
            UpdateDisplayText();
            return; // 힌트 처리 완료
        }


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

        if (finalMineRange != -1)
        {
            info.HintState = finalMineRange;
            UpdateDisplayText();
        }
        else
        {
            charInfos[index].IsRemoved = true;
            removableLetterCount--;

            foreach (int idx in GetLetterIndicesInRange(index, -1, 2))
            {
                if (charInfos[idx].IsRemoved) continue;
                charInfos[idx].IsRemoved = true;
                removableLetterCount--;
            }

            foreach (int idx in GetLetterIndicesInRange(index, 1, 2))
            {
                if (charInfos[idx].IsRemoved) continue;
                charInfos[idx].IsRemoved = true;
                removableLetterCount--;
            }

            UpdateDisplayText();

            if (removableLetterCount <= 0)
            {
                GameManager.Instance.StageClear();
            }
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

    private List<int> FindMinesInDirection(int startIndex, int direction)
    {
        var mineDistances = new List<int>();
        int sentenceLength = charInfos.Count;
        int lettersChecked = 0;
        const int maxLetterChecks = 2;

        for (int i = 1; i < sentenceLength; i++)
        {
            int currentIndex = (startIndex + (i * direction) + sentenceLength) % sentenceLength;
            CharInfo currentInfo = charInfos[currentIndex];

            if (char.IsLetter(currentInfo.Character))
            {
                lettersChecked++;

                if (currentInfo.IsMine)
                {
                    mineDistances.Add(lettersChecked);
                }

                if (lettersChecked >= maxLetterChecks)
                {
                    break; // 2칸까지만 확인
                }
            }
        }
        return mineDistances;
    }

    private int FindMineInRange(int startIndex, int direction, int maxLetterChecks)
    {
        int sentenceLength = charInfos.Count;
        int lettersChecked = 0;

        for (int i = 1; i < sentenceLength; i++)
        {
            int currentIndex = (startIndex + (i * direction) + sentenceLength) % sentenceLength;
            CharInfo currentInfo = charInfos[currentIndex];

            if (char.IsLetter(currentInfo.Character))
            {
                lettersChecked++;

                if (currentInfo.IsMine)
                {
                    return lettersChecked;
                }

                if (lettersChecked >= maxLetterChecks)
                {
                    return -1;
                }
            }
        }

        return -1;
    }

    private void ShowHintImage(int index)
    {
        for (int i = 0; i < sentenceText.Count(); i++)
        {
            RectTransform hintImage = hintImages[index + i * charInfos.Count];
            // sentenceText[i]의 자식으로 설정
            hintImage.transform.SetParent(sentenceText[i].transform, false);

            Vector3 topLeft = sentenceText[i].textInfo.characterInfo[index].topLeft;
            Vector3 topRight = sentenceText[i].textInfo.characterInfo[index].topRight;
            
            Vector3 center = new Vector3((topLeft.x + topRight.x) / 2, topLeft.y + 20f, 0);

            Debug.Log($"Char World Pos: {center}");

            hintImage.anchoredPosition = center;
            hintImage.gameObject.SetActive(true);
        }
    }



    private void UpdateDisplayText()
    {
        StringBuilder sb = new StringBuilder();

        foreach (CharInfo info in charInfos)
        {
            string finalTag = "<color=red>";
            if (info.HintState == 1) finalTag = "<color=white>";
            else if (info.HintState == 2) finalTag = "<color=#8C8C8C>";
            else if (info.HintState == 3) // 깜빡임 상태
            {
                // 시간에 따라 흰색과 회색을 번갈아 적용
                float pingPong = Mathf.PingPong(Time.time * blinkSpeed, 1.0f);
                finalTag = pingPong < 0.5f ? "<color=white>" : "<color=#8C8C8C>";
            }
            if (info.IsRemoved) finalTag = "<color=black>";
            else if (info.IsHoveredHint) finalTag = "<color=#FF6969>"; 

            sb.Append(finalTag);
            sb.Append(info.Character);
            sb.Append("</color>");
        }

        string richTextResult = sb.ToString();
        foreach (var textUI in sentenceText)
        {
            textUI.text = richTextResult;
        }

        bool anyBlinking = charInfos.Any(c => c.HintState == 3);
        isBlinkingActive = anyBlinking;
    }

    // 튜토리얼용 제어
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