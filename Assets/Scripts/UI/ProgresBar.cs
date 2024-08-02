using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ProgressBarController : NetworkBehaviour
{
    public Slider slider;
    public TaskManager taskManager;

    private void Start()
    {
        if (taskManager != null)
        {
            // MaxValue olarak TaskManager'daki taskArray uzunluğunu kullanıyoruz
            slider.maxValue = TaskManager.taskArray.Length;
            taskManager.totalTaskCount.OnValueChanged += OnTaskCountChanged;
        }
    }

    private void OnDestroy()
    {
        if (taskManager != null)
        {
            taskManager.totalTaskCount.OnValueChanged -= OnTaskCountChanged;
        }
    }

    private void OnTaskCountChanged(int previousValue, int newValue)
    {
        slider.value = newValue;
    }
}