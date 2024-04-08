using UnityEngine;

public class Stand : MonoBehaviour
{
    public Vase targetVase;
    public Vector3 positionOffset = new Vector3(0, 1100.0f, 0);

    void Update()
    {
        SetTargetPosition();
    }

    public void SetGameObject(Vase vase)
    {
        targetVase = vase;
    }

    public void SetTargetPosition()
    {
        if (targetVase != null)
        {
            Vector3 targetPosition = transform.position + positionOffset;

            targetVase.transform.position = targetPosition;
        }
        else
        {
            Debug.Log("Target Vase is not set in the inspector!");
        }
    }
}
