Shader "IndentSurface/SSH" {

	Properties{
		_Scale("Scale", Range(0, 2)) = 1
		_HeightOffset("Stamp Height OFfset", Range(0, 1)) = 0.7
	}

		SubShader{
			Tags {
				"RenderType" = "Opaque"
				"PreviewType" = "Plane"
			}
			Lighting Off Cull Off ZTest Always ZWrite Off

			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				uniform sampler2D _StampTex;
				uniform float _Scale;
				uniform float _HeightOffset;

				struct appdata_t {
					float4 pos : POSITION;
					float2 uv_stamp : TEXCOORD0;
					float3 normal : NORMAL;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float3 worldPos : TEXCOORD0;
					float2 uv_stamp : TEXCOORD1;
					float3 normal : TEXCOORD2;
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.pos);
					o.worldPos = mul(unity_ObjectToWorld, v.pos);
					o.uv_stamp = v.uv_stamp;
					o.normal = UnityObjectToWorldNormal(v.normal);
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					float4 stamp = tex2D(_StampTex, i.uv_stamp);
					float offset = (stamp.r - _HeightOffset) * _Scale * stamp.a;
					float3 worldPos = i.worldPos + normalize(i.normal) * offset;
					return float4(worldPos, 1); // 1 : valid, 0 : invalid
				}
				ENDCG
			}
	}
}
