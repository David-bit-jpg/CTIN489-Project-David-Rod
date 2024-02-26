using UnityEngine;
using UnityEngine.AI;
public class NavMeshController : MonoBehaviour
{
    public NavMeshData navMeshData;

    private void Start()
    {
        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        if(surface != null)
        {
            navMeshData = new NavMeshData();
            surface.navMeshData = navMeshData;
        }
    }

    public void UpdateNavMesh()
    {
        if(navMeshData == null)
        {
            return;
        }

        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        if(surface != null)
        {
            surface.UpdateNavMesh(navMeshData);
        }
    }
}
