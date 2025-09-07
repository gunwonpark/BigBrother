using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Ending : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI answerText1;
    [SerializeField] private TextMeshProUGUI answerText2;
    [SerializeField] private TextMeshProUGUI answerText3;
    [SerializeField] private TextMeshProUGUI answerText4;

    string dialogue1 = "이제 대장 [           ]이 남긴 모든 암호를 모았습니다.";
    string dialogue2 = "획득한 암호들을 조합하여 \r\n그가 전하려던 메시지를 확인합니다.";

    private IEnumerator Start()
    {
        answerText1.text = "ESCAPE";
        answerText2.text = "THIS CAGE";
        answerText3.text = "BEFORE IT IS";
        answerText4.text = "TOO LATE";

        dialogueText.text = dialogue1;
        yield return new WaitForSeconds(4f);
        yield return Fade(dialogueText, 1f, 0f, 2f);

        dialogueText.color = Color.white;

        dialogueText.text = dialogue2;
        yield return new WaitForSeconds(4f);
        yield return Fade(dialogueText, 1f, 0f, 2f);

        yield return new WaitForSeconds(2f);
        
        answerText1.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        answerText2.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        answerText3.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        answerText4.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);

        answerText1.gameObject.SetActive(false);
        answerText2.gameObject.SetActive(false);
        answerText3.gameObject.SetActive(false);
        answerText4.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene("EndingScene");
    }

    private IEnumerator Fade(Graphic image, float startAlpha, float endAlpha, float duration = 1f)
    {
        float elapsed = 0f;
        Color color = image.color;
        color.a = startAlpha;
        image.color = color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            color.a = alpha;
            image.color = color;
            yield return null;
        }
        color.a = endAlpha;
        image.color = color;
    }
}
