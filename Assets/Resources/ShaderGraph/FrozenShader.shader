Shader "Custom/FrozenShader"
{
    Properties
    {
        [MainColor] _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _CenterColor("Center Color", Color) = (0, 0.5, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _RimPower("Rim Power", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha 
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS   : NORMAL;
                float3 positionWS : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS   : NORMAL;
                float3 positionWS : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _RimColor;
                half4 _CenterColor;
                float4 _BaseMap_ST;
                float _RimPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 camPos = GetCameraPositionWS();
                float3 camDir = normalize(camPos - IN.positionWS);

                float rim = 1 - saturate(dot(IN.normalWS , camDir));
                rim = pow(rim, _RimPower);

                // 외곽(rim=1)은 _RimColor, 중앙(rim=0)은 _CenterColor
                half3 rgb = lerp(_CenterColor.rgb, _RimColor.rgb, rim);

                return half4(rgb, 1);
            }
            ENDHLSL
        }
    }
}
