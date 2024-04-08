using UnityEngine;

public class SetPosition : MonoBehaviour
{
    public GameObject targetGameObject;

    void Start()
    {
        SetTargetPosition();
    }

    public void SetTargetPosition()
    {
        if (targetGameObject != null)
        {
            targetGameObject.transform.position = new Vector3(0, 1100, 0);
        }
        else
        {
            Debug.LogError("Target GameObject is not set in the inspector!");
        }
    }
}
