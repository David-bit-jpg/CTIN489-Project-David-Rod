using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

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
        UpdateTaskText();
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
    public void RemoveTaskByType(TaskType taskType)
    {
        var toRemove = new List<Task>();

        foreach (var task in tasks)
        {
            if(task.Type == taskType)
            {
                toRemove.Add(task);
            }
        }

        if(toRemove.Count <= 0)
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

    void UpdateTaskText()
    {
        //first clear the task text 
        taskTexts.text = "";

        foreach (var task in tasks)
        {
            string taskString = task.TaskDescription;

            if(task.Type == TaskType.BalloonTask)
            {
                taskString = taskString + ": " + balloonSpawnerGood.spawnNum;
            }

            taskString += "\n";
            taskTexts.text += taskString;
        }
    }
}
