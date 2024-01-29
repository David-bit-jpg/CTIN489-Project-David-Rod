using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWalls : MonoBehaviour
{
    [SerializeField] Vector3 dest;
    Vector3 origin;
    public bool isTriggered = false;
    float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        origin = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTriggered && timer < 1)
        {
            timer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(origin, dest, timer);
        }
    }
}
