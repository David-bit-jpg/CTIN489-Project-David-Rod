using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public enum TaskType
{
    BalloonTask,
    CaptuerTask,
    PhotagraphyTask,
    Exit
}

[Serializable]
public class Task
{
    public string TaskDescription;
    public TaskType Type;
    protected TaskManager taskManager;

    public Task(string taskDescription, TaskType type)
    {
        TaskDescription = taskDescription;
        taskManager = TaskManager.Instance;
        Type = type;
    }

    public void taskFinish()
    {
        switch (Type)
        {
            case TaskType.BalloonTask:
                //TODO: do something when task is finished
                Debug.Log("Ballon Task is finished. " + TaskDescription);
                break;
            case TaskType.CaptuerTask:
                //TODO: do something when task is finished
                Debug.Log("Captuer Task is finished. " + TaskDescription);
                break;
            case TaskType.PhotagraphyTask:
                //TODO: do something when task is finished
                Debug.Log("Photagraphy Task is finished. " + TaskDescription);
                break;
            case TaskType.Exit:
                //TODO: do something when task is finished
                Debug.Log("Exit Task is finished. " + TaskDescription);
                break;
        }
    }

}
