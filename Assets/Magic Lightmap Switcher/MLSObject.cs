using System;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [ExecuteInEditMode]
    public class MLSObject : MonoBehaviour
    {
        [SerializeField]
        public string scriptId;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateGUID()
        {
            scriptId = Guid.NewGuid().ToString();
        }
    }
}
