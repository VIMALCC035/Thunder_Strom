Shader "Custom/VRRainShader"
{
    Properties
    {
        _BaseMap ("Rain Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _Intensity ("Glow Intensity", Range(0,5)) = 1
        _Alpha ("Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _TintColor;
            float _Intensity;
            float _Alpha;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS =
                    TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        IN.uv);

                tex.rgb *= _TintColor.rgb * _Intensity;
                tex.a *= _Alpha;

                return tex;
            }
            ENDHLSL
        }
    }
}