Shader "Unlit/Shader1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct MeshData
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
            };

            StructuredBuffer<float3> _Positions;

            Interpolators vert (MeshData v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);

                Interpolators o;

                uint instanceID = GetIndirectInstanceID(svInstanceID);

                o.vertex = UnityObjectToClipPos(v.vertex + float4(_Positions[instanceID], 1));

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}