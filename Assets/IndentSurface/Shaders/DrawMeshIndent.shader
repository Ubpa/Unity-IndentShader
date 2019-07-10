Shader "IndentSurface/DrawMeshIndent" {

	Properties{
		_Scale("Scale", Range(0, 2)) = 1
		_HeightOffset("Stamp Height OFfset", Range(0, 1)) = 0.7
	}

		SubShader{
			Tags {
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
			}
			Lighting Off Cull Off ZTest Always ZWrite Off
			Blend One Zero

			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				uniform sampler2D _SurfaceTex;
				uniform float4 _SourceTexCoords;
				uniform float _Scale;
				uniform float _HeightOffset;

				struct appdata_t {
					float4 pos : POSITION;
					float2 uv_stamp : TEXCOORD0;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv_stamp : TEXCOORD0;
					float4 scrPos : TEXCOORD1;
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.pos);
					o.uv_stamp = v.uv_stamp;
					o.scrPos = ComputeScreenPos(o.pos);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed2 uv_surf = i.scrPos.xy / i.scrPos.w;
					fixed4 surface = tex2D(_SurfaceTex, uv_surf);

					fixed4 stamp = tex2D(_MainTex, i.uv_stamp);
					float buildUp = clamp(stamp.r - _HeightOffset, 0, 1);
					float indent = clamp(stamp.r - _HeightOffset, -1, 0);
					return fixed4(surface.rgb + (stamp.rrr - _HeightOffset) * _Scale * stamp.a, 1);
				}
				ENDCG
			}
	}
}
