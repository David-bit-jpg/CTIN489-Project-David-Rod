using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform doorTransform; 
    public float openAngle = 90.0f;
    public float animationTime = 2.0f;

    public float openAngle2 = 90.0f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpening = false;

    private bool isOpened = false;
    private float currentAnimationTime = 0.0f;

    void Start()
    {
        closedRotation = doorTransform.rotation;
        openRotation = Quaternion.Euler(doorTransform.eulerAngles + Vector3.up * openAngle);
    }

    void Update()
    {
        if (isOpening)
        {
            if (currentAnimationTime < animationTime)
            {
                doorTransform.rotation = Quaternion.Lerp(closedRotation, openRotation, currentAnimationTime / animationTime);
                currentAnimationTime += Time.deltaTime;
                if (currentAnimationTime >= animationTime)
                {
                    isOpened = true;
                }
            }
        }
        else
        {
            if (currentAnimationTime > 0.0f)
            {
                doorTransform.rotation = Quaternion.Lerp(openRotation, closedRotation, (animationTime - currentAnimationTime) / animationTime);
                currentAnimationTime -= Time.deltaTime;
                if (currentAnimationTime <= 0.0f)
                {
                    isOpened = false;
                }
            }
        }
    }

    public void ToggleDoor()
    {
        if (isOpened && !isOpening)
        {
            isOpening = false;
            currentAnimationTime = animationTime - currentAnimationTime;
        }
        else if (!isOpened)
        {
            isOpening = true;
            currentAnimationTime = 0.0f;
        }
    }
}
