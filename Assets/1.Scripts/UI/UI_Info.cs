using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Info : MonoBehaviour
{
    [SerializeField] private HoverImage hintImage;
    [SerializeField] private Button memoButton;
    [SerializeField] private GameObject hintObject;
    [SerializeField] private Button ruleObjectButton;

    [SerializeField] private TextMeshProUGUI remainHintText;
    [SerializeField] private TextMeshProUGUI answerCountText;

    private Coroutine blinkCoroutine;

    private void Start()
    {
        memoButton.onClick.AddListener(ShowRuleObject);
        ruleObjectButton.onClick.AddListener(HideRuleObject);
        hintImage.OnHoverEnter += ShowHintObject;
        hintImage.OnHoverExit += HideHintObject;
        hintObject.SetActive(false);
        ruleObjectButton.gameObject.SetActive(false);
    }

    private void HideHintObject()
    {
        hintObject.SetActive(false);
    }

    private void ShowHintObject()
    {
        remainHintText.text = $"남은 힌트 개수 : {GameManager.Instance.RemainHintCount}";
        answerCountText.text = $"암호 개수 : {GameManager.Instance.AnswerCount}";
        hintObject.SetActive(true);
    }

    private void HideRuleObject()
    {
        ruleObjectButton.gameObject.SetActive(false);
        StopCoroutine(blinkCoroutine);
        DataManager.Instance.IsMemoButtonClicked = true;
    }

    private void ShowRuleObject()
    {
        ruleObjectButton.gameObject.SetActive(true);
    }

    public void BlinkMemo()
    {
        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        Image memoImage = memoButton.GetComponent<Image>();
        Color originalColor = memoImage.color;
        Color blinkColor = Color.yellow;
        float blinkDuration = 0.5f;
        float elapsedTime = 0f;
        bool isBlinking = true;
        while (isBlinking)
        {
            // ������ ���� ����
            while (elapsedTime < blinkDuration)
            {
                memoImage.color = Color.Lerp(originalColor, blinkColor, (elapsedTime / blinkDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            // ������ ������� ���� ����
            elapsedTime = 0f;
            while (elapsedTime < blinkDuration)
            {
                memoImage.color = Color.Lerp(blinkColor, originalColor, (elapsedTime / blinkDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            elapsedTime = 0f;
        }
        // ���������� ���� �������� ����
        memoImage.color = originalColor;
    }
}
