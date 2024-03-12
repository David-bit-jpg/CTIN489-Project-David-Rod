#ifndef MLS_BLENDING_STANDARD
#define MLS_BLENDING_STANDARD

// Lightmaps Processing
float _MLS_Lightmaps_Blend_Factor;

UNITY_DECLARE_TEX2D(_MLS_Lightmap_Color_Blend_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_Color_Blend_To);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_Dir_Blend_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_Dir_Blend_To);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_ShadowMask_Blend_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_Lightmap_ShadowMask_Blend_To);
UNITY_DECLARE_TEX2D(_MLS_BakeryRNM0_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM0_To);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM1_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM1_To);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM2_From);
UNITY_DECLARE_TEX2D_NOSAMPLER(_MLS_BakeryRNM2_To);

// Reflections Prcessing
float _MLS_Reflections_Blend_Factor;
int _MLS_ReflectionsFlag;

UNITY_DECLARE_TEXCUBE(_MLS_Reflection_Blend_From_0);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_Reflection_Blend_To_0);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_Reflection_Blend_From_1);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_Reflection_Blend_To_1);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_SkyReflection_Blend_From);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_SkyReflection_Blend_To);

// Sky Cubemap Processing
float _MLS_Sky_Cubemap_Blend_Factor;
float _MLS_Sky_Blend_From_Rotation;
float _MLS_Sky_Blend_To_Rotation;
float _MLS_Sky_Blend_From_Exposure;
float _MLS_Sky_Blend_To_Exposure;
half4 _MLS_Sky_Blend_From_Tint;
half4 _MLS_Sky_Blend_To_Tint;

UNITY_DECLARE_TEXCUBE(_MLS_Sky_Cubemap_Blend_From);
UNITY_DECLARE_TEXCUBE_NOSAMPLER(_MLS_Sky_Cubemap_Blend_To);

// General 
int _MLS_ENABLE_LIGHTMAPS_BLENDING;
int _MLS_ENABLE_REFLECTIONS_BLENDING;
int _MLS_ENABLE_SKY_CUBEMAPS_BLENDING;
int _MLS_No_Decode;

float4 BlendTwoTextures(int lightmapType, float2 uv, float shadow)
{
    half4 textureFrom;
    half4 textureTo;
    float4 result;

    switch (lightmapType)
    {
    case 0:
        textureFrom = UNITY_SAMPLE_TEX2D(_MLS_Lightmap_Color_Blend_From, uv.xy);
        textureTo = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_Lightmap_Color_Blend_To, _MLS_Lightmap_Color_Blend_From, uv.xy);
        break;
    case 1:
        textureFrom = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_Lightmap_Dir_Blend_From, _MLS_Lightmap_Color_Blend_From, uv.xy);
        textureTo = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_Lightmap_Dir_Blend_To, _MLS_Lightmap_Color_Blend_From, uv.xy);
        break;
    case 2:
        textureFrom = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_Lightmap_ShadowMask_Blend_From, _MLS_Lightmap_Color_Blend_From, uv.xy);
        textureTo = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_Lightmap_ShadowMask_Blend_To, _MLS_Lightmap_Color_Blend_From, uv.xy);
        break;
    }

    if (shadow >= 0)
    {
        return lerp(textureFrom, textureTo, _MLS_Lightmaps_Blend_Factor);

    }
    else
    {
        return lerp(textureFrom, textureTo, _MLS_Lightmaps_Blend_Factor);
    }
}

float4 BlendBakeryRNM(int lightmapType, float2 uv)
{
    half4 textureFrom;
    half4 textureTo;
    float4 result;

    switch (lightmapType)
    {
    case 0:
        textureFrom = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_BakeryRNM0_From, _MLS_BakeryRNM0_From, uv.xy);
        textureTo = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_BakeryRNM0_To, _MLS_BakeryRNM0_From, uv.xy);
        break;
    case 1:
        textureFrom = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_BakeryRNM1_From, _MLS_BakeryRNM0_From, uv.xy);
        textureTo = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_BakeryRNM1_To, _MLS_BakeryRNM0_From, uv.xy);
        break;
    case 2:
        textureFrom = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_BakeryRNM2_From, _MLS_BakeryRNM0_From, uv.xy);
        textureTo = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_BakeryRNM2_To, _MLS_BakeryRNM0_From, uv.xy);
        break;
    case 3:
        textureFrom = UNITY_SAMPLE_TEX2D(_MLS_Lightmap_Color_Blend_From, uv.xy);
        textureTo = UNITY_SAMPLE_TEX2D_SAMPLER(_MLS_Lightmap_Color_Blend_To, _MLS_Lightmap_Color_Blend_From, uv.xy);
        break;
    }

    return lerp(textureFrom, textureTo, _MLS_Lightmaps_Blend_Factor);
}

float4 BlendBakeryRNMSampler(int lightmapType, float2 uv, SamplerState samplerState)
{
    half4 textureFrom;
    half4 textureTo;

    switch (lightmapType)
    {
    case 0:
        textureFrom = _MLS_BakeryRNM0_From.Sample(samplerState, uv.xy);
        textureTo = _MLS_BakeryRNM0_To.Sample(samplerState, uv.xy);
        break;
    case 1:
        textureFrom = _MLS_BakeryRNM1_From.Sample(samplerState, uv.xy);
        textureTo = _MLS_BakeryRNM1_To.Sample(samplerState, uv.xy);
        break;
    case 2:
        textureFrom = _MLS_BakeryRNM2_From.Sample(samplerState, uv.xy);
        textureTo = _MLS_BakeryRNM2_To.Sample(samplerState, uv.xy);
        break;
    }

    return lerp(textureFrom, textureTo, _MLS_Lightmaps_Blend_Factor);
}

float4 BlendTwoCubeTextures(int probeIndex, float3 reflection, half mip)
{
    float4 textureFrom;
    float4 textureTo;
    float blendFactor;

    switch (probeIndex)
    {
    case 0:
        textureFrom = UNITY_SAMPLE_TEXCUBE_LOD(_MLS_Reflection_Blend_From_0, reflection, mip);
        textureTo = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(_MLS_Reflection_Blend_To_0, _MLS_Reflection_Blend_From_0, reflection, mip);
        blendFactor = _MLS_Reflections_Blend_Factor;
        break;
    case 1:
        textureFrom = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(_MLS_Reflection_Blend_From_1, _MLS_Reflection_Blend_From_0, reflection, mip);
        textureTo = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(_MLS_Reflection_Blend_To_1, _MLS_Reflection_Blend_From_0, reflection, mip);
        blendFactor = _MLS_Reflections_Blend_Factor;
        break;
    case 2:
        textureFrom = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(_MLS_SkyReflection_Blend_From, _MLS_Reflection_Blend_From_0, reflection, mip);
        textureTo = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(_MLS_SkyReflection_Blend_To, _MLS_Reflection_Blend_From_0, reflection, mip);
        blendFactor = _MLS_Reflections_Blend_Factor;
        break;
    case 3:
        textureFrom = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(_MLS_Sky_Cubemap_Blend_From, _MLS_Sky_Cubemap_Blend_From, reflection, mip);
        textureTo = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(_MLS_Sky_Cubemap_Blend_To, _MLS_Sky_Cubemap_Blend_From, reflection, mip);
        blendFactor = _MLS_Sky_Cubemap_Blend_Factor;
        break;
    }

    return lerp(textureFrom, textureTo, blendFactor);
}

float4 BlendTwoSkyCubeTextures(float3 reflection_from, float3 reflection_to, half mip)
{
    float4 textureFrom;
    float4 textureTo;
    float blendFactor;

    textureFrom = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(_MLS_Sky_Cubemap_Blend_From, _MLS_Sky_Cubemap_Blend_From, reflection_from, mip);
    textureTo = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(_MLS_Sky_Cubemap_Blend_To, _MLS_Sky_Cubemap_Blend_From, reflection_to, mip);

    return lerp(textureFrom, textureTo, _MLS_Sky_Cubemap_Blend_Factor);
}

half BlendSkyExposure()
{
    return lerp(_MLS_Sky_Blend_From_Exposure, _MLS_Sky_Blend_To_Exposure, _MLS_Sky_Cubemap_Blend_Factor);
}

half4 BlendSkyTint()
{
    return lerp(_MLS_Sky_Blend_From_Tint, _MLS_Sky_Blend_To_Tint, _MLS_Sky_Cubemap_Blend_Factor);
}
#endif