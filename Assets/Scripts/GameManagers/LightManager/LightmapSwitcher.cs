using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LightmapSet
{
    public Texture2D[] colorMaps;
    public Texture2D[] dirMaps; // Only necessary if using directional lightmaps
}

public class LightmapSwitcher : MonoBehaviour
{
    public LightmapSet dayLightmaps;
    public LightmapSet nightLightmaps;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SwitchToDay();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SwitchToNight();
        }
    }

    private void ApplyLightmapSet(LightmapSet set)
    {
        var lightmaps = new LightmapData[set.colorMaps.Length];
        for (int i = 0; i < set.colorMaps.Length; i++)
        {
            lightmaps[i] = new LightmapData
            {
                lightmapColor = set.colorMaps[i]
            };

            // If using directional lightmaps, also assign them here
            if (set.dirMaps != null && set.dirMaps.Length > i)
            {
                lightmaps[i].lightmapDir = set.dirMaps[i];
            }
        }

        LightmapSettings.lightmaps = lightmaps;
    }

    // Example method calls
    public void SwitchToDay()
    {
        ApplyLightmapSet(dayLightmaps);
    }

    public void SwitchToNight()
    {
        ApplyLightmapSet(nightLightmaps);
    }
}