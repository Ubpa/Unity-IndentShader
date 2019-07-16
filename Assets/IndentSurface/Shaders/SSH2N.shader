Shader "IndentSurface/SSH2N"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

			uniform float4x4 _Clip2World;
			sampler2D _CameraDepthTexture;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            sampler2D _MainTex;

            float4 frag (v2f i) : SV_Target
            {
				fixed2 screenPos = i.screenPos.xy / i.screenPos.w;
				//return float4(screenPos, 0, 1);

				fixed2 right = fixed2(1.0 / _ScreenParams.x, 0);
				fixed2 up = fixed2(0, 1.0 / _ScreenParams.y);
				//float2 right = float2(1.0 / 512.0, 0);
				//float2 up = float2(0, 1.0 / 512.0);

				// world position
                float4 lt = tex2D(_MainTex, screenPos - right + up);
				float4  t = tex2D(_MainTex, screenPos         + up);
				float4 rt = tex2D(_MainTex, screenPos + right + up);

				float4  l = tex2D(_MainTex, screenPos - right     );
				float4  m = tex2D(_MainTex, screenPos             );
				float4  r = tex2D(_MainTex, screenPos + right     );

				float4 lb = tex2D(_MainTex, screenPos - right - up);
				float4  b = tex2D(_MainTex, screenPos         - up);
				float4 rb = tex2D(_MainTex, screenPos + right - up);

				// 由于 Bilinear Filter，invalid texel 的 w 小于 1
				if (lt.a < 1 ||
					t.a < 1 ||
					rt.a < 1 ||
					l.a < 1 ||
					r.a < 1 ||
					lb.a < 1 ||
					b.a < 1 ||
					rb.a < 1)
					discard;

				// Sobel
				float3 dx = (lt + l * 2.0 + lb) - (rt + r * 2.0 + rb);
				float3 dz = (lt + t * 2.0 + rt) - (lb + b * 2.0 + rb);

				return float4((normalize(cross(dx, dz)) + 1) / 2, 1);
            }
            ENDCG
        }
    }
}
