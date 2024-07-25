using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static GameObject[] taskArray = new GameObject[4];

    private void Start()
    {
        for (int i = 0; i < taskArray.Length; i++)
        {
            taskArray[i] = transform.GetChild(i).gameObject;
        }
        taskArray.ToList().ForEach(i => Debug.Log(i.ToString()));
    }

    public static void RunTask(GameObject taskObject)
    {
        GameObject obj = Array.Find(taskArray, elm => elm.gameObject.name == taskObject.name);

        if (obj != null && !obj.activeSelf)
        {
            obj.SetActive(true);
        }
        else if (obj.activeSelf)
        {
            obj.SetActive(false);
        }
    }
}
