using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameClear : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI clearText;

    private void OnEnable()
    {
        StartCoroutine(ShowClearText());
    }

    private IEnumerator ShowClearText()
    {
        nextButton.interactable = false;
        clearText.alpha = 0;
        float duration = 3.0f; // 페이드 인 지속 시간
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            clearText.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        clearText.alpha = 1; // 확실히 완전히 보이도록 설정
        nextButton.interactable = true;
    }

    private void Start()
    {
        nextButton.onClick.AddListener(NextScene);
    }

    private void NextScene()
    {
        GameManager.Instance.LoadStage(GameManager.Instance.CurrentStageIndex);
        this.gameObject.SetActive(false);
    }
}
