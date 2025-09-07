using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance 
    { 
        get
        {
            if(instance == null)
            {
                GameObject obj = new GameObject("DataManager");
                instance = obj.AddComponent<DataManager>();
            }
            DontDestroyOnLoad(instance.gameObject);
            return instance;
        }
    }

    public int CurrentWorldLevel = 4;
    public const int ENABLE_FAIL_COUNT = 4;

    private bool isSlidingLocked = true;
    public bool IsSlidingLocked
    {
        get => isSlidingLocked;
        set
        {
            isSlidingLocked = value;
            if(!isSlidingLocked)
            {
                OnSlidingUnlocked?.Invoke();
            }
        }
    }

    public event Action OnSlidingUnlocked;

    public bool IsTextClicked = false;
    public bool IsMemoButtonClicked = false;
    public bool DoSliding => DoLeftSliding && DoRightSliding;
    public bool DoLeftSliding = false;
    public bool DoRightSliding = false;
}
