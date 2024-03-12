using System;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [ExecuteInEditMode]
    public class MLSStaticRenderer : MLSObject 
    {        
        [SerializeField]
        public string parentScene;
        [SerializeField]
        public Mesh defaultMesh;
        [SerializeField]
        public Transform defaultTransform;
        public MaterialPropertyBlock propertyBlock;        

        private void OnEnable()
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            Terrain terrain = gameObject.GetComponent<Terrain>();

            propertyBlock = new MaterialPropertyBlock();

            if (meshRenderer != null)
            {
                meshRenderer.GetPropertyBlock(propertyBlock);
            }
            else if (terrain != null)
            {
                terrain.GetSplatMaterialPropertyBlock(propertyBlock);
            }

            if (string.IsNullOrEmpty(scriptId))
            {
                parentScene = gameObject.scene.name;
                //UpdateGUID();
            }
        }
    }
}
