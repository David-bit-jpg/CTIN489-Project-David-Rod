using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskManager : MonoBehaviour
{
    [SerializeField] TextMeshPro taskTexts;
    private List<Task> tasks = new List<Task>();
    public List<Task> GetAllTasks() { return tasks; }
    public static TaskManager Instance;
    BalloonSpawnerGood balloonSpawnerGood;
    private void Awake()
    {
        //make this a singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        balloonSpawnerGood = FindObjectOfType<BalloonSpawnerGood>();
        InstantiateTasks();
        InitTaskText();
    }

    public void InstantiateTasks()
    {
        balloonSpawnerGood = FindObjectOfType<BalloonSpawnerGood>();
        if (balloonSpawnerGood)
        {
            Task balloonTask = new Task(balloonSpawnerGood.TaskDescription, TaskType.BalloonTask);
            TaskManager.Instance.AddTask(balloonTask);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddTask(Task task)
    {
        tasks.Add(task);
        UpdateTaskText();
    }

    public void RemoveTask(Task task)
    {
        task.taskFinish();
        tasks.Remove(task);
        UpdateTaskText();
    }

    public void ClearTasks()
    {
        tasks.Clear();
        UpdateTaskText();
    }
    public void RemoveTaskByType(TaskType taskType)
    {
        var toRemove = new List<Task>();

        foreach (var task in tasks)
        {
            if (task.Type == taskType)
            {
                toRemove.Add(task);
            }
        }

        if (toRemove.Count <= 0)
        {
            Debug.LogWarning("No tasks of type: " + taskType + " Found");
            return;
        }

        foreach (var item in toRemove)
        {
            item.taskFinish();
            tasks.Remove(item);
        }

        UpdateTaskText();
    }

    public void InitTaskText()
    {
        //first clear the task text 
        taskTexts.text = "";

        foreach (var task in tasks)
        {
            string taskString = task.TaskDescription;
            switch (task.Type)
            {
                case TaskType.BalloonTask:
                    taskString = taskString + ": " + balloonSpawnerGood.spawnNum;
                    break;
                case TaskType.CaptuerTask:
                    if (SceneManager.GetActiveScene().buildIndex == 0)
                    {
                        taskString = "Capture the mimic";
                    }
                    break;
                case TaskType.PhotagraphyTask:
                    break;
                case TaskType.Exit:
                    break;
            }


            taskString += "\n";
            taskTexts.text += taskString;
        }
    }

    public void UpdateTaskText()
    {
        //first clear the task text 
        taskTexts.text = "";

        foreach (var task in tasks)
        {
            string taskString = task.TaskDescription;
            switch (task.Type)
            {
                case TaskType.BalloonTask:
                    taskString = taskString + ": " + balloonSpawnerGood.balloonCount;
                    break;
                case TaskType.CaptuerTask:
                    if (SceneManager.GetActiveScene().buildIndex == 0)
                    {
                        taskString = "Capture the mimic";
                    }
                    break;
                case TaskType.PhotagraphyTask:
                    break;
                case TaskType.Exit:
                    break;
            }


            taskString += "\n";
            taskTexts.text += taskString;
        }
    }
}
