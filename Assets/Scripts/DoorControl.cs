using UnityEngine;
using UnityEngine.AI;
public class DoorController : MonoBehaviour
{
    public NavMeshLink navMeshLink; 
    public Transform doorTransform;
    public float openAngle = 90.0f;
    public float closeAngle = 0.0f;
    public float animationTime = 2.0f;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpening = false;

    private bool isOpened = false;
    private float currentAnimationTime = 0.0f;
    NavMeshController navMeshController;
    void Start()
    {
        closedRotation = Quaternion.Euler(doorTransform.localEulerAngles.x, closeAngle, doorTransform.localEulerAngles.z);
        openRotation = Quaternion.Euler(doorTransform.localEulerAngles.x, openAngle, doorTransform.localEulerAngles.z);
        navMeshController = FindObjectOfType<NavMeshController>();
        if (navMeshLink != null)
        {
            navMeshLink.enabled = false;
        }
    }

    void Update()
    {
        if (isOpening)
        {
            if (currentAnimationTime < animationTime)
            {
                doorTransform.localRotation = Quaternion.Lerp(closedRotation, openRotation, currentAnimationTime / animationTime);
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
                doorTransform.localRotation = Quaternion.Lerp(openRotation, closedRotation, (animationTime - currentAnimationTime) / animationTime);
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
        float yRotation = doorTransform.localEulerAngles.y % 360;
        yRotation = (yRotation > 180) ? yRotation - 360 : yRotation;
        if (Mathf.Abs(yRotation - closeAngle) < 1.0f)
        {
            isOpening = true;
            currentAnimationTime = 0.0f;
            // navMeshController.UpdateNavMesh();
        }
        else if (Mathf.Abs(yRotation - openAngle) < 1.0f)
        {
            isOpening = false;
            currentAnimationTime = animationTime - currentAnimationTime;
            // navMeshController.UpdateNavMesh();
        }
        if (navMeshLink != null)
        {
            navMeshLink.enabled = isOpening;
        }
        navMeshLink.UpdateLink();
    }
}
