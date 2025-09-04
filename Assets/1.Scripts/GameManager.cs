using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // �ߺ� ��������
            Destroy(gameObject);
        }
    }
    #endregion

    [Header("Game Settings")]
    [SerializeField] private int initialLives = 4;
    [SerializeField] private StageDatas stages;         // �������� ����
    private StageData curStage;

    [SerializeField] private StageController stageController; // StageController ��ũ��Ʈ ����

    public GameObject[] tempObject;

    [SerializeField] private int currentStageIndex;
    private int lives;
    [field : SerializeField] public bool IsGameActive { get; private set; }

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        currentStageIndex = 0;
        curStage = stages.stageDatas[currentStageIndex];

        lives = initialLives;
        IsGameActive = true;

        //gameplayPanel.SetActive(true);
        //gameOverPanel.SetActive(false);

        LoadStage(currentStageIndex);
    }

    private void LoadStage(int stageIndex)
    {
        stageController.SetupStage(curStage);
    }

    public void OnMineClicked()
    {
        if (!IsGameActive) return;

        lives--;

        tempObject[lives].SetActive(true);

        if (lives <= 0)
        {
            GameOver();
        }
    }

    public void StageClear()
    {
        if (!IsGameActive) return;

        currentStageIndex++;
        LoadStage(currentStageIndex);
    }

    private void GameOver()
    {
        IsGameActive = false;
    }
}