Shader "IndentSurface/DrawLineIndent" {

	Properties{
		_Scale("Scale", Range(0, 2)) = 1
		_HeightOffset("Stamp Height OFfset", Range(0, 1)) = 0.7
		_UV01("UV01", Vector) = (0,0,0,0)
		_TexR("Tex R", Float) = 0
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
				uniform float4 _UV01;
				uniform float _TexR;

				struct appdata_t {
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0; // surface texcoord
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.pos);
					o.uv = _SourceTexCoords.xy + v.uv * _SourceTexCoords.zw;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 surface = tex2D(_SurfaceTex, i.uv);

					float2 uv;
					float2 xDir = normalize(_UV01.zw - _UV01.xy);
					float2 yDir = fixed2(-xDir.y, xDir.x);
					float2 rPos = i.uv - _UV01.xy;
					float lenOnX = dot(rPos, xDir);
					float lenOnY = dot(rPos, yDir);
					float dis = length(_UV01.zw - _UV01.xy);

					if(abs(lenOnY) > _TexR || lenOnX < 0 || lenOnX > dis)
						return surface;

					float2 uvSurf = lenOnY * yDir + _UV01.xy;
					uv = ((uvSurf - _UV01.xy) / _TexR + 1) / 2;

					fixed4 stamp = tex2D(_MainTex, uv);
					float buildUp = clamp(stamp.r - _HeightOffset, 0, 1);
					float indent = clamp(stamp.r - _HeightOffset, -1, 0);
					return fixed4(surface.rgb + (stamp.rrr - _HeightOffset) * _Scale * stamp.a, 1);
				}
				ENDCG
			}
	}
}
