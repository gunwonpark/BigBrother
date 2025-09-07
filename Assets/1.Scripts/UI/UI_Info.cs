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
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        memoButton.gameObject.SetActive(true);
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
        while(true)
        {
            memoButton.gameObject.SetActive(!memoButton.gameObject.activeSelf);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnDisable()
    {
        if(blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
    }
}
