using UnityEngine;

namespace FPSController
{
    public enum TransformTarget
    {
        Position,
        Rotation,
        Both
    }

    [CreateAssetMenu(fileName = "PerlinNoiseData", menuName = "FirstPersonController/Data/PerlinNoiseData", order = 2)]
    public class PerlinNoiseData : ScriptableObject
    {
        #region Variables
            //Whether the camera rotation, position, or both change.
            public TransformTarget transformTarget;

            [Space]
            //How much the camera will be affected.
            public float amplitude;
            //How often the camera will be affected.
            public float frequency;
        #endregion    
    }
}