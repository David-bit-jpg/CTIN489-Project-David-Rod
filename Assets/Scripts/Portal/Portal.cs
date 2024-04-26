using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    [SerializeField]
    public Portal OtherPortal;
    [SerializeField] Collider wallCollider;

    private List<PortalableObject> portalObjects = new List<PortalableObject>();

    // Components.
    public Renderer Renderer { get; private set; }
    private new BoxCollider collider;

    PlayerMovement mPlayer;

    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
        Renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        mPlayer = FindObjectOfType<PlayerMovement>();
    }

    private void Update()
    {

        for (int i = 0; i < portalObjects.Count; ++i)
        {
            Vector3 objPos = transform.InverseTransformPoint(portalObjects[i].transform.position);
            float angle =  Mathf.Acos(Vector3.Dot(mPlayer.GetMoveDirection(), transform.forward));
            angle *=  Mathf.Rad2Deg;
            Debug.Log(angle);
            if (objPos.z > 0.0f && angle >-90.0f && angle < 90.0f)
            {
                portalObjects[i].Warp();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var obj = other.GetComponent<PortalableObject>();
        if (obj != null)
        {
            portalObjects.Add(obj);
            obj.SetIsInPortal(this, OtherPortal, wallCollider);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var obj = other.GetComponent<PortalableObject>();

        if (portalObjects.Contains(obj))
        {
            portalObjects.Remove(obj);
            obj.ExitPortal(wallCollider);
        }
    }




}
