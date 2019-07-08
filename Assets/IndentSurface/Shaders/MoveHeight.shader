Shader "IndentSurface/MoveHeight" {

	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_uOffset("uOffset", Float) = 0.0
		_vOffset("vOffset", Float) = 0.0
		_default("default", Float) = 0.7
	}

		SubShader{
		Cull Off ZWrite Off ZTest Always

			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				uniform float _uOffset;
				uniform float _vOffset;
				uniform float _default;

				struct appdata {
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float2 surfUV : TEXCOORD0;
				}; 

				v2f vert(appdata v)
				{
					v2f o;

					//UNITY_SETUP_INSTANCE_ID(v);// ?
					//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);// ?

					o.pos = UnityObjectToClipPos(v.pos);
					o.surfUV = v.uv + float2(_uOffset, _vOffset), _MainTex;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					if (i.surfUV.x < 0 || i.surfUV.x > 1 || i.surfUV.y < 0 || i.surfUV.y > 1)
						return fixed4(_default, _default, _default, 1);

					return tex2D(_MainTex, i.surfUV);
				}
				ENDCG
			}
	}
}
