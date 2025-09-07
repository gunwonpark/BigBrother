using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SceneChanger : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private TextMeshProUGUI loadingText;
    private float fadeAmount = 1f;
    private float waitDuration = 3f;
    public void Init(string showText, float fadeAmount, float waitDuration = 2f)
    {
        this.gameObject.SetActive(true);
        loadingText.text = showText;
        fadeImage.color = Color.black;
        this.fadeAmount = fadeAmount;
        this.waitDuration = waitDuration;
    }

    public IEnumerator SceneEnter()
    {
        fadeImage.gameObject.SetActive(true);
        loadingText.gameObject.SetActive(true);

        yield return Fade(loadingText, 0f, 1f, 3f);
        yield return new WaitForSeconds(3f);
        yield return Fade(loadingText, 1f, 0f, 3f);
        yield return Fade(fadeImage, 1f, fadeAmount, waitDuration);

        this.gameObject.SetActive(false);
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
