using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class TaskUtil
{
    public static Coroutine Delay(MonoBehaviour mon, Action action,float time)
    {
        return mon.StartCoroutine(IEDelay(time, action));
    }

    public static IEnumerator IEDelay(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        if (action != null)
        {
            action.Invoke();
        }
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    public static void RemoveArrayAt<T>(ref T[] arr, int index)
    {
        for (int a = index; a < arr.Length - 1; a++)
        {
            // moving elements downwards, to fill the gap at [index]
            arr[a] = arr[a + 1];
        }
        // finally, let's decrement Array's size by one
        Array.Resize(ref arr, arr.Length - 1);
    }

    public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        //Vector3 P = x * Vector3.Normalize(B - A) + A;
        //return P;
        return (A + x * (B - A));
    }
}