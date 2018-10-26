Shader "Hidden/TracingTest"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "CGINC/ContactLibrary.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };
            float4x4 _InvVP;
            TextureCube<half3> _EnvMap; SamplerState sampler_EnvMap;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                float4 worldPos = mul(_InvVP, v.vertex);
                o.worldPos = worldPos.xyz / worldPos.w;
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float t = 999999;
                float2 uv = 0;
                float3 dir = normalize(i.worldPos - _WorldSpaceCameraPos);
                float3 orig = _WorldSpaceCameraPos;
                float3 finalDir = dir;
                Point p;
                if(GetContactPixel(orig, dir, p) > 0.5){
                    finalDir = normalize(reflect(dir, p.normal));
                    return float4(_EnvMap.Sample(sampler_EnvMap, finalDir),1);
                }else
                {
                    return 0.3;
                }
            }
            ENDCG
        }
    }
}
