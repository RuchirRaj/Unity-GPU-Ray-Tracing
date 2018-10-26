Shader "Hidden/Depth"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma target 5.0
            #include "UnityCG.cginc"
            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            float frag (float4 vertex : SV_POSITION) : SV_Target
            {
                return vertex.z;
            }
            ENDCG
        }
    }
}
