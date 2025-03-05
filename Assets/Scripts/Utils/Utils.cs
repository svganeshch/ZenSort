using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static void ShuffleList<T>(ref List<T> list)
    {
        Random.InitState((int)System.DateTime.Now.Ticks);

        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
