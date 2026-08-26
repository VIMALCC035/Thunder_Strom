Shader "Custom/BoxFogVR_URP_3ColorGradient"
{
    Properties
    {
        [Header(Fog Colors)]
        _FogColorA ("Low Density Color", Color) = (0,0,1,0)
        _FogColorB ("Medium Density Color", Color) = (1,1,0,0.5)
        _FogColorC ("High Density Color", Color) = (1,0,0,1)

        [Header(Fog Settings)]
        _Density ("Density", Range(0,5)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "Forward"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 localPos : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _FogColorA;
            float4 _FogColorB;
            float4 _FogColorC;
            float _Density;

            Varyings vert(Attributes v)
            {
                Varyings o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);

                // Local position inside the box
                o.localPos = v.positionOS.xyz;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Distance from center
                float dist = length(i.localPos);

                // Fog strength
                float fog = saturate(1.0 - dist * _Density);

                half4 col;

                // 3-color gradient
                if (fog < 0.5)
                {
                    col = lerp(_FogColorA, _FogColorB, fog * 2.0);
                }
                else
                {
                    col = lerp(_FogColorB, _FogColorC, (fog - 0.5) * 2.0);
                }

                // Fade alpha with fog amount
                col.a *= fog;

                return col;
            }

            ENDHLSL
        }
    }

    FallBack Off
}