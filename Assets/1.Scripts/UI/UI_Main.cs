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
        // ���� �ܾ� ����
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
            1 => "ù ��° ",
            2 => "�� ��° ",
            3 => "�� ��° ",
            4 => "������ ",
            _ => index + "��°"
        };

        aquireText.text = $"{suffix} ��ȣ�� ȹ���߽��ϴ�.";
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
