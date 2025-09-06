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

    public int CurrentWorldLevel = 1;    
}
