using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DialogueGroup
{
    public GameObject groupObject;
    public TextMeshProUGUI dialogueText;
    public GameObject personImage;
    public GameObject textFrameImage;
    public bool isActivedOnce = false;
    public void SetActive(bool isActive)
    {
        groupObject.SetActive(isActive);
        textFrameImage.SetActive(isActive);
        personImage.SetActive(isActive);
    }
}

public class UI_Tutorial : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private DialogueGroup[] DialogueGroup; 
    [SerializeField] private TutorialDialogues tutorialDialogue;
    [SerializeField] private Image slideIndicator;
    [SerializeField] private GameObject RightClickObject;
    [SerializeField] private GameObject LeftClickObject;

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        StartCoroutine(DoTutorial());
    }   

    public IEnumerator DoTutorial()
    {
        DataManager.Instance.NeedTutorial = false;

        yield return FadeImage(0.97f, 1.0f);

        while(currentDialogueIndex < tutorialDialogue.dialogues.Length)
        {
            yield return ShowCurrentDialogue();
            if(currentDialogueIndex == 0)
            {
                DialogueGroup[0].SetActive(false);
                yield return FadeImage(0.0f, 2.0f);
                fadePanel.gameObject.SetActive(false);
                yield return new WaitForSeconds(1.0f);
            }
            else if(currentDialogueIndex == 1)
            {
                slideIndicator.gameObject.SetActive(true);
                DataManager.Instance.IsSlidingLocked = false;
            }
            else if(currentDialogueIndex == 2)
            {
                yield return new WaitUntil(() => DataManager.Instance.DoSliding);
                slideIndicator.gameObject.SetActive(false);
            }
            else if(currentDialogueIndex == 3)
            {
                GameManager.Instance.ShowRightClick();
                yield return new WaitUntil(() => DataManager.Instance.IsTextClicked);
                GameManager.Instance.HideRightClick();
                RightClickObject.gameObject.SetActive(true);
                DataManager.Instance.IsTextClicked = false;
            }
            else if(currentDialogueIndex == 4)
            {
                GameManager.Instance.ShowLeftClick();
                yield return new WaitUntil(() => DataManager.Instance.IsTextClicked);
                GameManager.Instance.HideLeftClick();
                LeftClickObject.gameObject.SetActive(true);
            }
            else if(currentDialogueIndex == 5)
            {
                GameManager.Instance.BlinkMemo();
                yield return new WaitUntil(() => DataManager.Instance.IsMemoButtonClicked);
            }
            else if(currentDialogueIndex == 6)
            {
                yield return FadeGroup(1, 2.0f);
                DialogueGroup[1].SetActive(false);
                GameManager.Instance.EnableAllClick();
            }

            currentDialogueIndex++;
        }
    }

    private IEnumerator FadeImage(float targetAlpha, float fadeDuration)
    {
        float startAlpha = fadePanel.color.a;
        float elapsedTime = 0.0f;

        while(elapsedTime <= fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, targetAlpha);

        yield return null;
    }   

    private IEnumerator ShowCurrentDialogue()
    {
        TutorialDialogue dialogue = tutorialDialogue.dialogues[currentDialogueIndex];
        int groupIndex = dialogue.groupIndex;
        
        yield return ShowGroup(groupIndex, DialogueGroup[groupIndex].isActivedOnce);
        TextMeshProUGUI dialogueText = GetDialogueText(groupIndex);

        string[] dialogues = dialogue.dialogues;
        foreach(string text in dialogues)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeText(dialogueText, text, 0.05f));
            while (isTyping)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    StopCoroutine(typingCoroutine);
                    dialogueText.text = text;
                    isTyping = false;
                }
                yield return null;
            }

            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            yield return null;
        }
    }

    private IEnumerator TypeText(TextMeshProUGUI textComponent, string fullText, float typingSpeed)
    {
        isTyping = true;
        textComponent.text = "";
        foreach(char c in fullText)
        {
            if(c == '\\')
            {
                textComponent.text += '\n';
                continue;
            }

            if(c == 'n')
            {
                continue;
            }
            
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private IEnumerator ShowGroup(int groupIndex, bool nowait = true)
    {
        if(nowait)
        {
            if(groupIndex == 0)
            {
                DialogueGroup[0].SetActive(true);
                DialogueGroup[1].SetActive(false);
            }
            else
            {
                DialogueGroup[0].SetActive(false);
                DialogueGroup[1].SetActive(true);
            }
        }
        else
        {
            if (groupIndex == 0)
            {
                DialogueGroup[0].groupObject.SetActive(true);
                DialogueGroup[1].groupObject.SetActive(false);
            }
            else
            {
                DialogueGroup[0].groupObject.SetActive(false);
                DialogueGroup[1].groupObject.SetActive(true);
            }

            DialogueGroup[groupIndex].isActivedOnce = true;
            DialogueGroup[groupIndex].personImage.SetActive(true);
            yield return new WaitForSeconds(1.0f);
            DialogueGroup[groupIndex].textFrameImage.SetActive(true);
        }
    }

    private IEnumerator FadeGroup(int groupIndex, float fadeDuration)
    {
        CanvasGroup canvasGroup = DialogueGroup[groupIndex].groupObject.GetComponent<CanvasGroup>();
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0.0f;
        while (elapsedTime <= fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, 0.0f, elapsedTime / fadeDuration);
            canvasGroup.alpha = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0.0f;
        yield return null;
    }

    private TextMeshProUGUI GetDialogueText(int groupIndex)
    {
        if(groupIndex == 0)
        {
            return DialogueGroup[0].dialogueText;
        }
        else
        {
            return DialogueGroup[1].dialogueText;
        }
    }

}
