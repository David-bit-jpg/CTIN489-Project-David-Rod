using UnityEngine;

public class Stand : MonoBehaviour
{
    public Vase targetGameObject;


    void Start()
    {
        SetTargetPosition();
    }
    public void SetGameObject(Vase gameObject)
    {
        targetGameObject = gameObject;
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
