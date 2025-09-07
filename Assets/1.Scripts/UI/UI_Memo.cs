using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Memo : MonoBehaviour
{
    [SerializeField] private Button memo1Button;
    [SerializeField] private Button memo2Button;
    [SerializeField] private GameObject memo1Object;
    [SerializeField] private GameObject memo2Object;

    private void Start()
    {
        memo1Button.onClick.AddListener(OnMemo1ButtonClicked);
        memo2Button.onClick.AddListener(OnMemo2ButtonClicked);        
    }

    private void OnMemo1ButtonClicked()
    {
        memo1Object.SetActive(false);
        memo2Object.SetActive(true);
    }

    private void OnMemo2ButtonClicked()
    {
        memo1Object.SetActive(true);
        memo2Object.SetActive(false);
    }   
}
