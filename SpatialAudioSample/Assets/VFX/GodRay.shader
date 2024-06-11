Shader "Custom/GodRay"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ConeSlopeCosSin("Cone Slope Cos Sin", Vector) = (0,0,0,0)
        _ConeRadius("Cone Radius", Vector) = (0,0,0,0)
        _ConeApexOffsetZ("Cone Apex Offset Z", Float) = 0

        //        _AlphaInside("Alpha Inside", Range(0,1)) = 1
        //        _AlphaOutside("Alpha Outside", Range(0,1)) = 1

        _DistanceFallOff("Distance Fall Off", Vector) = (0,1,1,0)

        _DistanceCamClipping("Camera Clipping Distance", Float) = 0.5
        _FadeOutFactor("FadeOutFactor", Float) = 1

        _AttenuationLerpLinearQuad("Lerp between attenuation linear and quad", Float) = 0.5
        _DepthBlendDistance("Depth Blend Distance", Float) = 2

        _FresnelPow("Fresnel Pow", Range(0,15)) = 1

        _GlareFrontal("Glare Frontal", Range(0,1)) = 0.5
        _GlareBehind("Glare from Behind", Range(0,1)) = 0.5
        _DrawCap("Draw Cap", Float) = 1

        _NoiseVelocityAndScale("Noise Velocity And Scale", Vector) = (0,0,0,0)
        _NoiseParam("Noise Param", Vector) = (0,0,0,0)

        _CameraParams("Camera Params", Vector) = (0,0,0,0)

        _BlendSrcFactor("BlendSrcFactor", Int) = 1 // One
        _BlendDstFactor("BlendDstFactor", Int) = 1 // One

        _DynamicOcclusionClippingPlaneWS("Dynamic Occlusion Clipping Plane WS", Vector) = (0,0,0,0)
        _DynamicOcclusionClippingPlaneProps("Dynamic Occlusion Clipping Plane Props", Float) = 0.25

        _DynamicOcclusionDepthTexture("DynamicOcclusionDepthTexture", 2D) = "white" {}
        _DynamicOcclusionDepthProps("DynamicOcclusionDepthProps", Float) = 0.25

        _LocalForwardDirection("LocalForwardDirection", Vector) = (0,0,1)
        _TiltVector("TiltVector", Vector) = (0,0,0,0)
        _AdditionalClippingPlaneWS("AdditionalClippingPlaneWS", Vector) = (0,0,0,0)
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "DisableBatching" = "True" // disable dynamic batching which doesn't work neither with multiple materials nor material property blocks
        }

        Blend[_BlendSrcFactor][_BlendDstFactor]
        ZWrite Off

        SubShader
        {
            Pass
            {
                Cull Front

                HLSLPROGRAM
                #pragma target 3.0
                #pragma  vertex vert
                #pragma  fragment frag
                #pragma multi_compile_fog
                #pragma multi_compile __ VLB_ALPHA_AS_BLACK
                #pragma multi_compile __ VLB_NOISE_3D
                #pragma multi_compile __ VLB_DEPTH_BLEND
                #pragma multi_compile __ VLB_CLIPPING_PLANE
                #pragma multi_compile __ VLB_COLOR_GRADIENT_MATRIX_HIGH VLB_COLOR_GRADIENT_MATRIX_LOW

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            
                #include "GodRayCode.cginc"

                // 顶点着色器
                CustomVaryings vert(CustomAttributes v)
                {
                    return vertShared(v, v.texcoord.y);
                }

                // 片段着色器
                half4 frag(CustomVaryings i) : SV_TARGET
                {
                    return fragShared(i, i.cameraPosObjectSpace_outsideBeam.w);
                }
                ENDHLSL
            }
        }
    }

    // FallBack "Diffuse"
    //    FallBack "Hidden/Shader Graph/FallbackError"
}