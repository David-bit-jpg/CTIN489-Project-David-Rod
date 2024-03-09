using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public enum TaskType
{
    BalloonTask,
    CaptuerTask,
    PhotagraphyTask
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
    public void removeTask() { 
        taskManager.RemoveTask(this);
    }

}
