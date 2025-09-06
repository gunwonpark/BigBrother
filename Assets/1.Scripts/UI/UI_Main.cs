using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UI_Main : MonoBehaviour
{
    [SerializeField] private CanvasGroup gameTextGroup;
    [SerializeField] private TextMeshProUGUI answerText;
    [SerializeField] private TextMeshProUGUI aquireText;
    [SerializeField] private TextMeshProUGUI koreanText;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameClearUI;

    private void Start()
    {
        ResetUI();
    }

    public void SetText(StageData data)
    {
        string answerWord = data.AnswerWord;
        string koreanSentence = data.KoreanSentence;

        answerText.text = "";
        // 정답 단어 설정
        foreach (var answerChar in answerWord)
        {
            answerText.text += answerChar + "  ";
        }

        koreanText.text = $"[{koreanSentence}]";
    }

    public void ResetUI()
    {
        gameTextGroup.alpha = 1f;
        HideAquireText();
        HideAnswerText();
        EnableGameTextClick();
        HideGameOverUI();
        HideGameClearUI();
    }

    public void DisableGameTextClick()
    {
        gameTextGroup.interactable = false;
        gameTextGroup.blocksRaycasts = false;
    }

    public void EnableGameTextClick()
    {
        gameTextGroup.interactable = true;
        gameTextGroup.blocksRaycasts = true;
    }

    public void ShowAnswerText()
    {
        answerText.gameObject.SetActive(true);
    }

    public void HideAnswerText()
    {
        answerText.gameObject.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        gameOverUI.SetActive(true);
    }

    public void ShowGameClearUI()
    {
        gameClearUI.SetActive(true);
    }

    public void HideGameOverUI()
    {
        gameOverUI.SetActive(false);
    }

    public void HideGameClearUI()
    {
        gameClearUI.SetActive(false);
    }

    public void ShowAquireText(int index)
    {
        aquireText.gameObject.SetActive(true);

        string suffix = index switch
        {
            1 => "첫 번째 ",
            2 => "두 번째 ",
            3 => "세 번째 ",
            4 => "마지막 ",
            _ => index + "번째"
        };

        aquireText.text = $"{suffix} 암호를 획득했습니다.";
    }

    public void HideAquireText()
    {
        aquireText.gameObject.SetActive(false);
    }

    public IEnumerator FadeCanvasGroup(float targetAlpha, float duration)
    {
        float startAlpha = gameTextGroup.alpha;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            gameTextGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }
        gameTextGroup.alpha = targetAlpha;
    }
}
