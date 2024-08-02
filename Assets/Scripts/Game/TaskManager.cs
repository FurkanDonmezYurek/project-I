using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TaskManager : NetworkBehaviour
{
    public static GameObject[] taskArray = new GameObject[16];
    public NetworkVariable<int> totalTaskCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
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
            Cursor.lockState = CursorLockMode.Confined;

        }
        else if (obj.activeSelf)
        {
            obj.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
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
            
        }
    }

    public void CompleteTask()
    {
        if (IsServer)
        {
            totalTaskCount.Value = CalculateCompletedTasks();
        }
        else
        {
            UpdateTaskCountServerRpc(CalculateCompletedTasks());
        }
    }

    private int CalculateCompletedTasks()
    {
        // Task'leri hesaplayın ve sonucu döndürün
        return 0; // Örnek değeri değiştirin
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateTaskCountServerRpc(int newTaskCount)
    {
        totalTaskCount.Value = newTaskCount;
    }
    
}
