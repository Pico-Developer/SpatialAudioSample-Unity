#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
// -------------------------------------
// 结构体定义
// -------------------------------------
struct CustomAttributes
{
    float4 positionOS :POSITION;
    float3 normal :NORMAL;
    float2 texcoord : TEXCOORD0;
    float3 uv2: TEXCOORD2;
    float4 color: COLOR;
};

struct CustomVaryings
{
    float3 posObjectSpace : TEXCOORD0;
    float3 posObjectSpaceNonSkewed : TEXCOORD8;
    float4 posWorldSpace : TEXCOORD1;
    float4 posViewSpace_extraData : TEXCOORD2;
    float4 cameraPosObjectSpace_outsideBeam : TEXCOORD3;
    // float4 uvwNoise_intensity : TEXCOORD5;

    float4 positionCS : SV_POSITION;
    float4 color: COLOR;
    float3 positionWS: NORMAL2;
    float4 positionOS :POSITION2;
};

#define VLBObjectToViewPos(pos)                     (mul(UNITY_MATRIX_V, mul(UNITY_MATRIX_M, float4(pos.xyz, 1.0))).xyz)
// -------------------------------------
// 属性定义
// -------------------------------------
UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float4x4, _ColorGradientMatrix)
UNITY_DEFINE_INSTANCED_PROP(float4, _ColorFlat)

// UNITY_DEFINE_INSTANCED_PROP(half, _AlphaInside)
// UNITY_DEFINE_INSTANCED_PROP(half, _AlphaOutside)
UNITY_DEFINE_INSTANCED_PROP(float2, _ConeSlopeCosSin) // between -1 and +1
UNITY_DEFINE_INSTANCED_PROP(float2, _ConeRadius) // x = start radius ; y = end radius
UNITY_DEFINE_INSTANCED_PROP(float, _ConeApexOffsetZ) // > 0
UNITY_DEFINE_INSTANCED_PROP(float, _AttenuationLerpLinearQuad)
UNITY_DEFINE_INSTANCED_PROP(float3, _DistanceFallOff) // fallOffStart, fallOffEnd, maxGeometryDistance
UNITY_DEFINE_INSTANCED_PROP(float, _DistanceCamClipping)
UNITY_DEFINE_INSTANCED_PROP(float, _FadeOutFactor)
UNITY_DEFINE_INSTANCED_PROP(float, _FresnelPow) // must be != 0 to avoid infinite fresnel
UNITY_DEFINE_INSTANCED_PROP(float, _GlareFrontal)
UNITY_DEFINE_INSTANCED_PROP(float, _GlareBehind)
UNITY_DEFINE_INSTANCED_PROP(float, _DrawCap)
UNITY_DEFINE_INSTANCED_PROP(float4, _CameraParams)

// if VLB_OCCLUSION_CLIPPING_PLANE
UNITY_DEFINE_INSTANCED_PROP(float4, _DynamicOcclusionClippingPlaneWS)
UNITY_DEFINE_INSTANCED_PROP(float, _DynamicOcclusionClippingPlaneProps)
// elif VLB_OCCLUSION_DEPTH_TEXTURE
UNITY_DEFINE_INSTANCED_PROP(float, _DynamicOcclusionDepthProps)
// endif

// if VLB_DEPTH_BLEND
UNITY_DEFINE_INSTANCED_PROP(float, _DepthBlendDistance)
// endif

// if VLB_NOISE_3D
UNITY_DEFINE_INSTANCED_PROP(float4, _NoiseVelocityAndScale)
UNITY_DEFINE_INSTANCED_PROP(float2, _NoiseParam)
// endif

// if VLB_MESH_SKEWING
UNITY_DEFINE_INSTANCED_PROP(float3, _LocalForwardDirection)
// endif

UNITY_DEFINE_INSTANCED_PROP(float2, _TiltVector)
UNITY_DEFINE_INSTANCED_PROP(float4, _AdditionalClippingPlaneWS)
UNITY_INSTANCING_BUFFER_END(Props)


// -------------------------------------
// 工具函数
// -------------------------------------
#define matWorldToObject unity_WorldToObject
#define VLBWorldToViewPos(pos)                      (mul(unity_MatrixV, float4(pos.xyz, 1.0)).xyz)
inline float invLerp(float a, float b, float t) { return (t - a) / (b - a); }
inline float invLerpClamped(float a, float b, float t) { return saturate(invLerp(a, b, t)); }
inline float DistanceToPlane(float3 pos, float3 normal, float d) { return dot(normal, pos) + d; }
inline float3 UnityWorldToObjectPos(in float3 pos) { return mul(matWorldToObject, float4(pos, 1.0)).xyz; }
inline float4 VLBObjectToWorldPos(in float4 pos) { return mul(unity_ObjectToWorld, pos); }
inline float isEqualOrGreater(float a, float b) { return step(b, a); }

inline float4 VLBObjectToClipPos(in float3 pos)
{
    return mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(pos.xyz, 1.0)));
}

// camera
inline float3 __VLBWorldToObjectPos(in float3 pos) { return mul(unity_WorldToObject, float4(pos, 1.0)).xyz; }

inline float3 VLBGetCameraPositionObjectSpace(float3 scaleObjectSpace)
{
    return __VLBWorldToObjectPos(_WorldSpaceCameraPos).xyz * scaleObjectSpace;
}

inline float ComputeAttenuation(float pixDistZ, float fallOffStart, float fallOffEnd, float lerpLinearQuad)
{
    float distFromSourceNormalized = invLerpClamped(fallOffStart, fallOffEnd, pixDistZ);

    // Almost simple linear attenuation between Fade Start and Fade End: Use smoothstep for a better fall to zero rendering
    float attLinear = smoothstep(0, 1, 1 - distFromSourceNormalized);

    // Unity's custom quadratic attenuation https://forum.unity.com/threads/light-attentuation-equation.16006/
    float attQuad = 1.0 / (1.0 + 25.0 * distFromSourceNormalized * distFromSourceNormalized);

    const float kAttQuadStartToFallToZero = 0.8;
    attQuad *= saturate(smoothstep(1.0, kAttQuadStartToFallToZero, distFromSourceNormalized));
    // Near the light's range (fade end) we fade to 0 (because quadratic formula never falls to 0)

    return lerp(attLinear, attQuad, lerpLinearQuad);
}

// 获取倾斜距离影响因素
inline float GetTiltDistanceFactor(float3 posObjectSpace)
{
    float2 tiltVector = UNITY_ACCESS_INSTANCED_PROP(Props, _TiltVector);
    float pixDistFromSource = abs(posObjectSpace.z);
    return pixDistFromSource + posObjectSpace.x * tiltVector.x + posObjectSpace.y * tiltVector.y;
}

inline float ComputeFadeWithCamera(float3 posViewSpace, float enabled)
{
    float distCamToPixWS = abs(posViewSpace.z);
    // only check Z axis (instead of length(posViewSpace.xyz)) to have smoother transition with near plane (which is not curved)
    float camFadeDistStart = _ProjectionParams.y; // cam near place
    float camFadeDistEnd = camFadeDistStart + UNITY_ACCESS_INSTANCED_PROP(Props, _DistanceCamClipping);
    float fadeWhenTooClose = smoothstep(0, 1, invLerpClamped(camFadeDistStart, camFadeDistEnd, distCamToPixWS));

    // fade out according to camera's near plane
    return lerp(1, fadeWhenTooClose, enabled);
}

inline float ComputeBoostFactor(float pixDistFromSource, float outsideBeam, float isCap)
{
    pixDistFromSource = max(pixDistFromSource, 0.001); // prevent 1st segment from being boosted when boostFactor is 0
    float glareFrontal = UNITY_ACCESS_INSTANCED_PROP(Props, _GlareFrontal);
    float insideBoostDistance = glareFrontal * UNITY_ACCESS_INSTANCED_PROP(Props, _DistanceFallOff).y;
    float boostFactor = 1 - smoothstep(0, 0 + insideBoostDistance + 0.001, pixDistFromSource);
    // 0 = no boost ; 1 = max boost
    boostFactor = lerp(boostFactor, 0, outsideBeam); // no boost for outside pass

    float4 cameraParams = UNITY_ACCESS_INSTANCED_PROP(Props, _CameraParams);
    float cameraIsInsideBeamFactor = saturate(cameraParams.w); // _CameraParams.w is (-1 ; 1) 
    boostFactor = cameraIsInsideBeamFactor * boostFactor; // no boost for outside pass

    boostFactor = lerp(boostFactor, 1, isCap); // cap is always at max boost
    return boostFactor;
}

// -------------------------------------
// 顶点着色器
// -------------------------------------
CustomVaryings vertShared(CustomAttributes v, float outsideBeam)
{
    CustomVaryings o = (CustomVaryings)0;
    float4 vertexOS = v.positionOS;
    vertexOS.z *= vertexOS.z;

    float2 coneRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _ConeRadius);
    float maxRadius = max(coneRadius.x, coneRadius.y);
    float normalizedRadiusStart = coneRadius.x / maxRadius;
    float normalizedRadiusEnd = coneRadius.y / maxRadius;
    vertexOS.xy *= lerp(normalizedRadiusStart, normalizedRadiusEnd, vertexOS.z);

    float3 scaleObjectSpace = float3(maxRadius, maxRadius, UNITY_ACCESS_INSTANCED_PROP(Props, _DistanceFallOff).z);

    o.posObjectSpaceNonSkewed = vertexOS.xyz * scaleObjectSpace;

    float isCap = v.texcoord.x;
    float pixDistFromSource = length(o.posObjectSpace.z);
    float boostFactor = ComputeBoostFactor(pixDistFromSource, outsideBeam, isCap);

    // maxGeometryDistance
    o.color = v.color;
    // o.uv = vertexOS.xz * 0.12;
    // o.normal = TransformObjectToWorldNormal(v.normal);
    o.positionOS = vertexOS.rgba;
    // o.positionCS = TransformObjectToHClip(vertexOS).rgba;
    o.posWorldSpace = VLBObjectToWorldPos(vertexOS);
    o.positionCS = VLBObjectToClipPos(vertexOS.xyz);

    // float3 posViewSpace = VLBObjectToViewPos(vertexOS);
    #if defined(VLBWorldToViewPos)
    float3 posViewSpace = VLBWorldToViewPos(o.posWorldSpace.xyz);
    #elif defined(VLBObjectToViewPos)
    float3 posViewSpace = VLBObjectToViewPos(vertexOS);
    #endif


    o.posObjectSpace = vertexOS.xyz * scaleObjectSpace;

    float extraData = boostFactor;
    o.posViewSpace_extraData = float4(posViewSpace.xyz, extraData);
    float3 cameraPosObjectSpace = VLBGetCameraPositionObjectSpace(scaleObjectSpace);
    o.cameraPosObjectSpace_outsideBeam = float4(
        cameraPosObjectSpace,
        outsideBeam);
    // o.uvwNoise_intensity.a = intensity;

    // o.color = ComputeColor(pixDistFromSourceTilted, outsideBeam);
    return o;
}

inline float ComputeInOutBlending(float vecCamToPixDotZ, float outsideBeam)
{
    // smooth blend between inside and outside geometry depending of View Direction
    const float kFaceLightSmoothingLimit = 1;
    float factorFaceLightSourcePerPixN = saturate(smoothstep(kFaceLightSmoothingLimit, -kFaceLightSmoothingLimit,
                                                             vecCamToPixDotZ)); // smoother transition

    return lerp(factorFaceLightSourcePerPixN, 1 - factorFaceLightSourcePerPixN, outsideBeam);
}

// boostFactor is normalized
float ComputeFresnel(float3 posObjectSpace, float3 vecCamToPixOSN, float outsideBeam, float boostFactor)
{
    // outsideBeam = 1;
    // 计算法线
    float2 cosSinFlat = normalize(posObjectSpace.xy);
    float2 coneSlopeCosSin = UNITY_ACCESS_INSTANCED_PROP(Props, _ConeSlopeCosSin);
    float3 normalObjectSpace = (float3(cosSinFlat.x * coneSlopeCosSin.x, cosSinFlat.y * coneSlopeCosSin.x,
                                       -coneSlopeCosSin.y));
    normalObjectSpace *= (outsideBeam * 2 - 1); // = outsideBeam ? 1 : -1;

    // 实际菲涅尔因子
    float fresnelReal = dot(normalObjectSpace, -vecCamToPixOSN);

    // 通过投影viewDir矢量来计算菲涅耳系数以支持长光束
    // compute a fresnel factor to support long beams by projecting the viewDir vector
    // on the virtual plane formed by the normal and tangent
    float coneApexOffsetZ = UNITY_ACCESS_INSTANCED_PROP(Props, _ConeApexOffsetZ);
    float3 tangentPlaneNormal = normalize(posObjectSpace.xyz + float3(0, 0, coneApexOffsetZ));
    float distToPlane = dot(-vecCamToPixOSN, tangentPlaneNormal);
    float3 vec2D = normalize(-vecCamToPixOSN - distToPlane * tangentPlaneNormal);
    float fresnelProjOnTangentPlane = dot(normalObjectSpace, vec2D);

    // blend between the 2 fresnels
    float3 localForwardDirN = UNITY_ACCESS_INSTANCED_PROP(Props, _LocalForwardDirection);
    float vecCamToPixDotZ = dot(vecCamToPixOSN, localForwardDirN);
    float factorNearAxisZ = abs(vecCamToPixDotZ); // factorNearAxisZ is normalized

    float fresnel = lerp(fresnelProjOnTangentPlane, fresnelReal, factorNearAxisZ);

    float fresnelPow = UNITY_ACCESS_INSTANCED_PROP(Props, _FresnelPow);

    // Lerp the fresnel pow to the glare factor according to how far we are from the axis Z
    // 根据我们离Z轴的距离，将菲涅耳功率与眩光系数相乘
    const float kMaxGlarePow = 1.5;
    float glareFrontal = UNITY_ACCESS_INSTANCED_PROP(Props, _GlareFrontal);
    float glareBehind = UNITY_ACCESS_INSTANCED_PROP(Props, _GlareBehind);
    float glareFactor = kMaxGlarePow * (1 - lerp(glareFrontal, glareBehind, outsideBeam));
    fresnelPow = lerp(fresnelPow, min(fresnelPow, glareFactor), factorNearAxisZ);

    // Pow the fresnel
    fresnel = smoothstep(0, 1, fresnel);
    fresnel = (1 - isEqualOrGreater(-fresnel, 0)) * // fix edges artefacts on android ES2
        (pow(fresnel, fresnelPow));


    // Boost distance inside
    float boostFresnel = lerp(fresnel, 1 + 0.001, boostFactor);
    fresnel = lerp(boostFresnel, fresnel, outsideBeam); // no boosted fresnel if outside

    // We do not have to treat cap a special way, since boostFactor is already set to 1 for cap via ComputeBoostFactor

    return fresnel;
}

// Vector Camera to current Pixel, in object space and normalized
inline float3 ComputeVectorCamToPixOSN(float3 pixPosOS, float3 cameraPosOS)
{
    float3 vecCamToPixOSN = normalize(pixPosOS - cameraPosOS);

    // Deal with ortho camera:
    // With ortho camera, we don't want to change the fresnel according to camera position.
    // So instead of computing the proper vector "Camera to Pixel", we take account of the "Camera Forward" vector (which is not dependant on the pixel position)
    float4 cameraParams = UNITY_ACCESS_INSTANCED_PROP(Props, _CameraParams);
    float3 vecCamForwardOSN = cameraParams.xyz;

    #if VLB_MESH_SKEWING
    vecCamForwardOSN = normalize(UnskewVectorOS(vecCamForwardOSN));
    #endif // VLB_MESH_SKEWING

    return lerp(vecCamToPixOSN, vecCamForwardOSN, unity_OrthoParams.w);
}

// -------------------------------------
// 片段着色器
// -------------------------------------
half4 fragShared(CustomVaryings i, float outsideBeam) : SV_TARGET
{
    float intensity = 1;
    float isCap = i.posViewSpace_extraData.w;
    // intensity *= isEqualOrGreater(UNITY_ACCESS_INSTANCED_PROP(Props, _DrawCap), isCap - 0.00001);
    // float isCap = v.texcoord.x;
    // intensity *= 1 - outsideBeam * isCap;

    float boostFactor = 0;

    float3 cameraPosObjectSpace = i.cameraPosObjectSpace_outsideBeam.xyz;

    float3 vecCamToPixOSN = ComputeVectorCamToPixOSN(i.posObjectSpaceNonSkewed.xyz, cameraPosObjectSpace);

    float pixDistFromSourceTilted = GetTiltDistanceFactor(i.posObjectSpace);
    // float boostFactor = i.posViewSpace_extraData.w;
    float fadeWithCameraEnabled = 1 - max(boostFactor,
                                          // do not fade according to camera when we are in boost zone, to keep boost effect
                                          unity_OrthoParams.w);
    float3 posViewSpace = i.posViewSpace_extraData.xyz;
    // intensity *= ComputeFadeWithCamera(posViewSpace, fadeWithCameraEnabled);
    float vecCamToPixDotZ = dot(vecCamToPixOSN, float3(0, 0, 1));

    // fresnel
    intensity *= ComputeFresnel(i.posObjectSpaceNonSkewed, vecCamToPixOSN, outsideBeam, boostFactor);
    // intensity *= UNITY_ACCESS_INSTANCED_PROP(Props, _FadeOutFactor);

    // fade out
    float3 distancesFallOff = UNITY_ACCESS_INSTANCED_PROP(Props, _DistanceFallOff);
    intensity *= ComputeAttenuation(pixDistFromSourceTilted, distancesFallOff.x, distancesFallOff.y,
    UNITY_ACCESS_INSTANCED_PROP(Props, _AttenuationLerpLinearQuad));

    // intensity = 1.0;
    intensity *= ComputeInOutBlending(vecCamToPixDotZ, outsideBeam);
    half4 color2 = _Color;
    // 遍阅越小，中心越大
    // float4 c2 = half4(0.1,0,0, 1);
    // float4 c2 = half4(isCap,0,0, 1);
    // float baseCapColor = 0;
    // if (!outsideBeam)
    // {
    //     baseCapColor = (1 - i.positionOS.z) * 0.3;
    // }

    // float4 capColor = half4(baseCapColor, baseCapColor, baseCapColor, 0);
    // float4 finalColor = color2.xyzw * intensity;
    float4 c2 = color2 * intensity;
    // finalColor.x = max(c2.x, baseCapColor);
    // finalColor.y = max(c2.y, baseCapColor);
    // finalColor.z = max(c2.z, baseCapColor);
    // finalColor.w = intensity;
    // float4 c2 = color2 * intensity; // + half4(i.posObjectSpaceNonSkewed.x, i.posObjectSpaceNonSkewed.x,
    // i.posObjectSpaceNonSkewed.x, 1) * (1 - outsideBeam) * intensity;

    // 应用雾
    // float fogCoord = ComputeFogFactor(i.positionCS.z);
    // MixFog(c2, fogCoord);
    return c2;
}
