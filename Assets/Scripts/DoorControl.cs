using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class DoorController : MonoBehaviour
{
    public NavMeshLink navMeshLink; 
    public Transform doorTransform;
    public float openAngle = 90.0f;
    public float closeAngle = 0.0f;
    public float animationTime = 2.0f;
    public bool isProcessing = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    public bool isOpening = false;

    public bool isOpened = false;
    private float currentAnimationTime = 0.0f;

    public AudioSource AudioSource;
    void Start()
    {
        //AudioSource = gameObject.GetComponent<AudioSource>();
        closedRotation = Quaternion.Euler(doorTransform.localEulerAngles.x, closeAngle, doorTransform.localEulerAngles.z);
        openRotation = Quaternion.Euler(doorTransform.localEulerAngles.x, openAngle, doorTransform.localEulerAngles.z);
        if (navMeshLink != null)
        {
            navMeshLink.enabled = false;
        }
    }

    void Update()
    {
        if (!isProcessing)
        {
            return;
        }

        if (!isOpened)
        {
            if (currentAnimationTime < animationTime)
            {
                doorTransform.localRotation = Quaternion.Lerp(closedRotation, openRotation, currentAnimationTime / animationTime);
                currentAnimationTime += Time.deltaTime;
                if (currentAnimationTime >= animationTime)
                {
                    isOpened = true;
                    isProcessing = false;
                    doorTransform.localRotation = openRotation;
                }
            }
        }
        else
        {
            if (currentAnimationTime < animationTime)
            {
                doorTransform.localRotation = Quaternion.Lerp(openRotation, closedRotation, currentAnimationTime / animationTime);
                currentAnimationTime += Time.deltaTime;
                if (currentAnimationTime >= animationTime)
                {
                    isOpened = false;
                    isProcessing = false;
                    doorTransform.localRotation = closedRotation;
                }
            }
        }
    }

    public void ToggleDoor()
    {
        if (isProcessing)
        {
            return;
        }

        float targetAngle;
        if (isOpened)
        {
            targetAngle = closeAngle;
        }
        else
        {
            targetAngle = openAngle;
        }

        AudioSource.Play();

        isProcessing = true;
        currentAnimationTime = 0.0f;

        if (navMeshLink != null)
        {
            bool shouldLinkBeEnabled = !isOpened && !navMeshLink.enabled;
            bool shouldLinkBeDisabled = isOpened && navMeshLink.enabled;
            // if(shouldLinkBeEnabled)
            // {
            //     Debug.Log("Should be Enabled");
            // }
            // else if(shouldLinkBeDisabled)
            // {
            //     Debug.Log("Should be Disabled");
            // }
            // else
            // {
            //     Debug.Log("ERRORRR");
            // }
            if (shouldLinkBeEnabled)
            {
                navMeshLink.enabled = true;
            }
            else if (shouldLinkBeDisabled)
            {
                navMeshLink.enabled = false;
            }
            // navMeshLink.UpdateLink();
        }

        /*if (isProcessing)
        {
            float yRotation = doorTransform.localEulerAngles.y % 360;
            yRotation = (yRotation > 180) ? yRotation - 360 : yRotation;

            if (Mathf.Abs(yRotation - targetAngle) < 5.0f)
            {
                isProcessing = true;
                currentAnimationTime = 0.0f;
            }
            
            
        }*/
    }

    void Unlock()
    {

    }

}