using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_GameOver : MonoBehaviour
{
    [SerializeField] private Button restartButton;

    private void Start()
    {
        restartButton.onClick.AddListener(OnClickRestart);
    }

    private void OnClickRestart()
    {
        SceneManager.LoadScene("MainScene");
    }
}
