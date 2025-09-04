using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    private static readonly System.Random rng = new System.Random();
    // O(n) 시간으로 리스트를 섞어준다.
    public static List<T> Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = list[n];
            list[n] = list[k];
            list[k] = temp;
        }
        return list;
    }
}
