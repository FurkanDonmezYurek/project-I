using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TaskManager : NetworkBehaviour
{
    public static GameObject[] taskArray = new GameObject[16];
    public NetworkVariable<int> totalTaskCount = new NetworkVariable<int>(0);

    private void Start()
    {
        for (int i = 0; i < taskArray.Length; i++)
        {
            taskArray[i] = transform.GetChild(i).gameObject;
        }
        taskArray.ToList().ForEach(i => Debug.Log(i.ToString()));
    }

    public static void RunTask(GameObject taskObject, bool taskStarted)
    {
        GameObject obj = Array.Find(taskArray, elm => elm.gameObject.name == taskObject.name);

        if (obj != null && !obj.activeSelf && taskStarted)
        {
            obj.SetActive(true);
        }
        else if (obj.activeSelf)
        {
            obj.SetActive(false);
        }
    }

    public void GameEnded()
    {
        if (IsHost)
        {
            totalTaskCount.Value++;
        }
        else
        {
            SendTaskCountServerRpc();
        }
    }

    [ServerRpc]
    void SendTaskCountServerRpc()
    {
        totalTaskCount.Value++;
    }
}
