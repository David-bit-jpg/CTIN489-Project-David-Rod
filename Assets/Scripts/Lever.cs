using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    bool isTurnedOn = false;
    bool processing = false;
    public Quaternion closedRotation, openRotation, targetRotation, curRotation, startRotation;
    float timer = 0.0f;
    [SerializeField]GameObject lever;

    public bool GetIsOn() { return !processing && isTurnedOn; }
    private void Start()
    {
        closedRotation = Quaternion.Euler(120, -lever.transform.localEulerAngles.y, lever.transform.localEulerAngles.z);
        openRotation = Quaternion.Euler(220, -lever.transform.localEulerAngles.y, lever.transform.localEulerAngles.z);
        startRotation = openRotation;
        targetRotation = openRotation;
    }

    private void Update()
    {
        lever.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, timer);
        curRotation = lever.transform.rotation;
        processing = curRotation != targetRotation;
        if (processing)
        {
            timer += Time.deltaTime;
        }
        else
        {
            isTurnedOn = curRotation == openRotation;
        }
    }

    public void Interact()
    {
        if (isTurnedOn && !processing)
        {
            timer = 0;
            startRotation = openRotation;
            Close();
        }
        else if(!processing)
        {
            timer = 0;
            startRotation = closedRotation;
            Open();
        }
    }

    public void Open()
    {
        targetRotation = openRotation;
    }

    public void Close()
    {
        targetRotation = closedRotation;
    }
}
