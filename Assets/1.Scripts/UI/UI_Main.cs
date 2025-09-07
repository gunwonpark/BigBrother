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
    [SerializeField] private GameObject EndingUI;
    [SerializeField] private UI_Info infoUI;

    private void Start()
    {
        ResetUI();
    }

    public void SetText(StageData data)
    {
        string answerWord = data.AnswerWord;
        string koreanSentence = data.KoreanSentence;

        answerText.text = "";
   
        foreach (var answerChar in answerWord)
        {
            answerText.text += answerChar + "  ";
        }

        koreanText.text = $"[{koreanSentence}]";
    }

    public void ResetUI()
    {
        gameTextGroup.alpha = 1f;
        ShowInfoUI();
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

        HideInfoUI();
    }

    public void EnableGameTextClick()
    {
        gameTextGroup.interactable = true;
        gameTextGroup.blocksRaycasts = true;

        ShowInfoUI();
    }

    public void ShowInfoUI()
    {
        infoUI.gameObject.SetActive(true);
    }

    public void HideInfoUI()
    {
        infoUI.gameObject.SetActive(false);
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

        string text = index switch
        {
            1 => "튜토리얼 스테이지를 클리어했습니다.\r\n실전 스테이지로 진입합니다.",
            2 => "첫 번째 암호를 획득하였습니다",
            3 => "두 번째 암호를 획득하였습니다",
            4 => "세 번째 암호를 획득하였습니다",
            5 => "마지막 암호를 획득하였습니다",
            _ => index + "번째"
        };

        aquireText.text = text;
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

    public void BlinkMemo()
    {
        infoUI.BlinkMemo();
    }

    public void DoEndingUI()
    {
        EndingUI.SetActive(true);
    }
}
