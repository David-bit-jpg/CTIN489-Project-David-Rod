using MimicSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    ParticleSystem particleSystem;
    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
        particleSystem.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Movement mimicMovement = other.GetComponent<Movement>();
        if (mimicMovement)
        {
            mimicMovement.isStop = true;
            particleSystem.Play();
            TaskManager.Instance.RemoveTaskByType(TaskType.CaptuerTask);
        }
    }
}
