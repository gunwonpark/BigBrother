using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    private static readonly System.Random rng = new System.Random();
    // O(n) �ð����� ����Ʈ�� �����ش�.
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
