using System;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [ExecuteInEditMode]
    public class MLSDynamicRenderer : MLSObject
    {
        public MagicLightmapSwitcher.AffectedObject affectableObject;
        private string parentScene;
        private bool added;

        private void OnEnable()
        {
            if (parentScene != gameObject.scene.name)
            {
                parentScene = gameObject.scene.name;
                UpdateGUID();
            }
        }

        private void Update()
        {
            if (!added)
            {
                if (MagicLightmapSwitcher.OnDynamicRendererAdded != null)
                {
                    added = true;

                    MagicLightmapSwitcher.OnDynamicRendererAdded.Invoke(gameObject, this);
                }
            }
        }

        private void OnDestroy()
        {
            MagicLightmapSwitcher.OnDynamicRendererRemoved.Invoke(gameObject, affectableObject);
        }
    }
}