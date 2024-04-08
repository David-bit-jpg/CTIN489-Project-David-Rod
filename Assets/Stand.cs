using UnityEngine;

public class Stand : MonoBehaviour
{
    public Vase targetVase;
    private string targetName;
    bool isCorrect = true;
    void Update()
    {
        SetTargetPosition();
    }

    public void SetGameObject(Vase vase)
    {
        targetVase = vase;
    }
    public bool IsCorrect()
    {
        if(true)
        {

        }
        return isCorrect;
    }

    public void SetTargetPosition()
    {
        if (targetVase != null)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, 1.8f, transform.position.z);

            targetVase.transform.position = targetPosition;
        }
        else
        {
            Debug.Log("Target Vase is not set in the inspector!");
        }
    }
}
