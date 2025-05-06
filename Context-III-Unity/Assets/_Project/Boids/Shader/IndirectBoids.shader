Shader "Unlit/IndirectBoids"
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

            struct Boid
            {
                float3 position;
                float3 direction;
            };

            StructuredBuffer<Boid> _BoidsBuffer;

            float4x4 create_matrix(float3 pos, float3 dir, float3 up) {
                float3 zaxis = normalize(dir);
                float3 xaxis = normalize(cross(up, zaxis));
                float3 yaxis = cross(zaxis, xaxis);
                return float4x4(
                    xaxis.x, yaxis.x, zaxis.x, pos.x,
                    xaxis.y, yaxis.y, zaxis.y, pos.y,
                    xaxis.z, yaxis.z, zaxis.z, pos.z,
                    0, 0, 0, 1
                );
            }

            Interpolators vert (MeshData v, uint svInstanceID : SV_InstanceID)
            {
                uint instanceID = GetIndirectInstanceID(svInstanceID);

                float4x4 boid = create_matrix(
                    _BoidsBuffer[instanceID].position, 
                    _BoidsBuffer[instanceID].direction,
                    float3(0, 1, 0)
                );

                Interpolators o;

                o.vertex = UnityObjectToClipPos(mul(boid, v.vertex));

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