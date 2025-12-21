Shader "Custom/RadialGradientURP"
{
    Properties
    {
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _ColorInner ("Inner Color", Color) = (1,1,1,1)
        _ColorOuter ("Outer Color", Color) = (0,0,0,1)
        _Radius ("Radius", Float) = 0.75
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP core
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // homogeneous clip space
                float2 uv          : TEXCOORD0;
            };

            float4 _Center;
            float4 _ColorInner;
            float4 _ColorOuter;
            float _Radius;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float d = distance(IN.uv, _Center.xy) / _Radius;
                d = saturate(d);
                return lerp(_ColorInner, _ColorOuter, d);
            }

            ENDHLSL
        }
    }
}
