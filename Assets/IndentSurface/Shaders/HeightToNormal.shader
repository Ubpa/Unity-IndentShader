// https://github.com/cpetry/NormalMap-Online/blob/gh-pages/javascripts/shader/NormalMapShader.js

Shader "IndentSurface/HeightToNormal"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Width("Width", Float) = 1
		_Height("Height", Float) = 1
		_Strength("Strength", Range(0.01,5.0)) = 2.5
		_Level("Level", Range(0,10)) = 5
		//_dz("dz", Range(0.01, 20.00)) = 1.00
	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			uniform float _Width;
			uniform float _Height;
			uniform float _Strength;
			uniform float _Level;
			//uniform float _dz;

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 step = float2(1/_Width, 1/_Height);
				float2 vUv = IN.uv;

				// 纹理坐标
				float2 tlv = float2(vUv.x - step.x, vUv.y + step.y);
				float2 lv = float2(vUv.x - step.x, vUv.y);
				float2 blv = float2(vUv.x - step.x, vUv.y - step.y);
				float2 tv = float2(vUv.x 		  , vUv.y + step.y);
				float2 bv = float2(vUv.x 		  , vUv.y - step.y);
				float2 trv = float2(vUv.x + step.x, vUv.y + step.y);
				float2 rv = float2(vUv.x + step.x, vUv.y);
				float2 brv = float2(vUv.x + step.x, vUv.y - step.y);

				// 边界处理
//				tlv = float2(tlv.x >= 0.0 ? tlv.x : (1.0 + tlv.x), 	tlv.y >= 0.0 ? tlv.y : (1.0 + tlv.y));
//				tlv = float2(tlv.x < 1.0 ? tlv.x : (tlv.x - 1.0), 	tlv.y < 1.0 ? tlv.y : (tlv.y - 1.0));
//				lv = float2(lv.x >= 0.0 ? lv.x : (1.0 + lv.x),  	lv.y >= 0.0 ? lv.y : (1.0 + lv.y));
//				lv = float2(lv.x < 1.0 ? lv.x : (lv.x - 1.0),   lv.y < 1.0 ? lv.y : (lv.y - 1.0));
//				blv = float2(blv.x >= 0.0 ? blv.x : (1.0 + blv.x), 	blv.y >= 0.0 ? blv.y : (1.0 + blv.y));
//				blv = float2(blv.x < 1.0 ? blv.x : (blv.x - 1.0), 	blv.y < 1.0 ? blv.y : (blv.y - 1.0));
//				tv = float2(tv.x >= 0.0 ? tv.x : (1.0 + tv.x),  	tv.y >= 0.0 ? tv.y : (1.0 + tv.y));
//				tv = float2(tv.x < 1.0 ? tv.x : (tv.x - 1.0),   tv.y < 1.0 ? tv.y : (tv.y - 1.0));
//				bv = float2(bv.x >= 0.0 ? bv.x : (1.0 + bv.x),  	bv.y >= 0.0 ? bv.y : (1.0 + bv.y));
//				bv = float2(bv.x < 1.0 ? bv.x : (bv.x - 1.0),   bv.y < 1.0 ? bv.y : (bv.y - 1.0));
//				trv = float2(trv.x >= 0.0 ? trv.x : (1.0 + trv.x), 	trv.y >= 0.0 ? trv.y : (1.0 + trv.y));
//				trv = float2(trv.x < 1.0 ? trv.x : (trv.x - 1.0), 	trv.y < 1.0 ? trv.y : (trv.y - 1.0));
//				rv = float2(rv.x >= 0.0 ? rv.x : (1.0 + rv.x),  	rv.y >= 0.0 ? rv.y : (1.0 + rv.y));
//				rv = float2(rv.x < 1.0 ? rv.x : (rv.x - 1.0),   rv.y < 1.0 ? rv.y : (rv.y - 1.0));
//				brv = float2(brv.x >= 0.0 ? brv.x : (1.0 + brv.x), 	brv.y >= 0.0 ? brv.y : (1.0 + brv.y));
//				brv = float2(brv.x < 1.0 ? brv.x : (brv.x - 1.0), 	brv.y < 1.0 ? brv.y : (brv.y - 1.0));

				// 采样
				float tl = abs(tex2D(_MainTex, tlv).r);
				float l = abs(tex2D(_MainTex, lv).r);
				float bl = abs(tex2D(_MainTex, blv).r);
				float t = abs(tex2D(_MainTex, tv).r);
				float b = abs(tex2D(_MainTex, bv).r);
				float tr = abs(tex2D(_MainTex, trv).r);
				float r = abs(tex2D(_MainTex, rv).r);
				float br = abs(tex2D(_MainTex, brv).r);

				float dx = 0.0, dy = 0.0;

				float dz = 1.0 / _Strength * (1.0 + exp2(_Level));

				// Sobel
				dx = tl + l * 2.0 + bl - tr - r * 2.0 - br;
				dx = -dx;
				dy = tl + t * 2.0 + tr - bl - b * 2.0 - br;

				// Scharr
				// dx = tl * 3.0 + l * 10.0 + bl * 3.0 - tr * 3.0 - r * 10.0 - br * 3.0;
				// dy = tl * 3.0 + t * 10.0 + tr * 3.0 - bl * 3.0 - b * 10.0 - br * 3.0;

				float4 normal = float4(normalize(float3(dx, dy, dz)), tex2D(_MainTex, vUv).a);

				return fixed4(normal.xy * 0.5 + 0.5, normal.zw);
			}
			ENDCG
		}
		}
}
