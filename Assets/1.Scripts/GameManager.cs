using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EyeComponent
{
    public SpriteRenderer eyeFrame;
    public SpriteRenderer eyeBackground;
    public SpriteRenderer pupil;
}

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    [Header("Game Settings")]
    [SerializeField] private int initialLives = 4;
    [SerializeField] private StageDatas stages;         // 스테이지 정보
    [SerializeField] private int currentStageIndex;

    [SerializeField] private StageController stageController; // StageController 스크립트 참조
    [SerializeField] private EyeComponent[] eyeObjects = new EyeComponent[2];
    [SerializeField] private FullEyeController fullEyeController;


    [Header("UI Refs")]
    [SerializeField] private GameObject gameOverPanel;

    [field : SerializeField] public bool IsGameActive { get; private set; }

    private Coroutine eyeAnimationCoroutine;

    private int lives;
    private StageData curStage;

    private Queue<IEnumerator> animationQueue = new Queue<IEnumerator>();
    private bool isProcessingQueue = false;

    void Start()
    {
        StartGame();
        fullEyeController.OnSeeAnimationEnd += BlinkEye;
        fullEyeController.OnBlinkAnimationEnd += ShowGameOverPanel;
    }

    private void OnDestroy()
    {
        fullEyeController.OnSeeAnimationEnd -= BlinkEye;
        fullEyeController.OnBlinkAnimationEnd -= ShowGameOverPanel;
    }

    public void StartGame()
    {
        currentStageIndex = 0;
        curStage = stages.stageDatas[currentStageIndex];

        lives = initialLives;
        IsGameActive = true;

        ResetEyes();

        LoadStage(currentStageIndex);
    }

    private void ResetEyes()
    {
        foreach (var eye in eyeObjects)
        {
            if (eye.eyeFrame) eye.eyeFrame.material.SetFloat("_FillAmount", 0);
            if (eye.eyeBackground) eye.eyeBackground.material.SetFloat("_FillAmount", 0);
            if (eye.pupil) eye.pupil.material.SetFloat("_FillAmount", 0);
        }
    }

    private void LoadStage(int stageIndex)
    {
        stageController.SetupStage(curStage);
    }

    public void OnMineClicked()
    {
        if (!IsGameActive) return;

        lives--;
        int deathCount = initialLives - lives;

        IEnumerator animationToPlay = null;
        switch (deathCount)
        {
            case 1:
                animationToPlay = AnimateEyeFill("eyeFrame", 0.6f, 0.5f);
                break;
            case 2:
                animationToPlay = AnimateEyeFill("eyeFrame", 1.0f, 0.5f);
                break;
            case 3:
                animationToPlay = AnimateEyeFill("eyeBackground", 1.0f, 0.5f);
                break;
            case 4:
                animationToPlay = AnimateEyeFill("pupil", 1.0f, 0.5f);
                break;
        }

        if (animationToPlay != null)
        {
            animationQueue.Enqueue(animationToPlay);
        }

        IEnumerator gameOver = null;

        if (lives <= 0)
        {
            gameOver = GameOver();
        }

        if (gameOver != null)
        {
            animationQueue.Enqueue(gameOver);
        }

        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessAnimationQueue());
        }

    }

    private IEnumerator ProcessAnimationQueue()
    {
        isProcessingQueue = true;

        // 큐에 처리할 애니메이션이 남아있는 동안 계속 반복
        while (animationQueue.Count > 0)
        {
            // 큐에서 가장 먼저 들어온 애니메이션을 꺼내서
            IEnumerator currentAnimation = animationQueue.Dequeue();
            // 해당 애니메이션이 끝날 때까지 기다립니다.
            yield return StartCoroutine(currentAnimation);
        }

        // 큐의 모든 작업이 끝나면 플래그를 false로 변경
        isProcessingQueue = false;
    }

    private IEnumerator AnimateEyeFill(string targetPart, float targetFill, float duration)
    {
        float timer = 0f;

        List<Material> materialsToAnimate = new List<Material>();
        List<float> startFills = new List<float>();

        foreach (var eye in eyeObjects)
        {
            SpriteRenderer targetRenderer = null;
            switch (targetPart)
            {
                case "eyeFrame": targetRenderer = eye.eyeFrame; break;
                case "eyeBackground": targetRenderer = eye.eyeBackground; break;
                case "pupil": targetRenderer = eye.pupil; break;
            }

            if (targetRenderer != null)
            {
                materialsToAnimate.Add(targetRenderer.material);
                startFills.Add(targetRenderer.material.GetFloat("_FillAmount"));
            }
        }

        // 애니메이션 메인 루프
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // 매 프레임마다 저장해둔 "모든" 머티리얼의 값을 업데이트합니다.
            for (int i = 0; i < materialsToAnimate.Count; i++)
            {
                float newFill = Mathf.Lerp(startFills[i], targetFill, progress);
                materialsToAnimate[i].SetFloat("_FillAmount", newFill);
            }

            yield return null;
        }

        // 애니메이션 종료 후 모든 머티리얼의 값을 목표값으로 정확히 맞춰줍니다.
        foreach (var mat in materialsToAnimate)
        {
            mat.SetFloat("_FillAmount", targetFill);
        }
    }

    public void StageClear()
    {
        if (!IsGameActive) return;

        currentStageIndex++;
        LoadStage(currentStageIndex);
    }

    private IEnumerator GameOver()
    {
        yield return null;
        Debug.Log("Game Over");

        IsGameActive = false;
        fullEyeController.FullEyeSee();
    }

    private void BlinkEye()
    {
        StartCoroutine(WaitAndBlink(0.5f));
    }

    private IEnumerator WaitAndBlink(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Blink");
        fullEyeController.FullEyeBlink();
    }
    private void ShowGameOverPanel()
    {
        StartCoroutine(WaitAndShowGameOverPanel(0.5f));
    }

    private IEnumerator WaitAndShowGameOverPanel(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        gameOverPanel.SetActive(true);
    }

}