using MimicSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    ParticleSystem particleSystem;
    Movement mimicMovement;
    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
        particleSystem.Stop();
        mimicMovement = FindObjectOfType<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mimicMovement == null)
        {
            return;
        }
        if(Vector3.Distance(mimicMovement.transform.position, transform.position) <= 2.0f && mimicMovement.isChasing)
        {
            mimicMovement.isStop = true;
            if (particleSystem.isStopped)
            {
                particleSystem.Play();
            }
            TaskManager.Instance.RemoveTaskByType(TaskType.CaptuerTask);
            LevelManager.Instance.ShowLevelEnd();
        }
        else
        {
            if (!particleSystem.isStopped)
            {
                particleSystem.Stop();
            }
        }
    }
}