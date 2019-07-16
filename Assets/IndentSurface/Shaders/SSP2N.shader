Shader "IndentSurface/SSP2N"
{
    Properties
    {
        _SSP ("Texture", 2D) = "white" {}
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

            sampler2D _SSP;

            float4 frag (v2f i) : SV_Target
            {
				fixed2 screenPos = i.screenPos.xy / i.screenPos.w;

				fixed2 right = fixed2(1.0 / _ScreenParams.x, 0);
				fixed2 up = fixed2(0, 1.0 / _ScreenParams.y);

				// world position
                float4 lt = tex2D(_SSP, screenPos - right + up);
				float4  t = tex2D(_SSP, screenPos         + up);
				float4 rt = tex2D(_SSP, screenPos + right + up);

				float4  l = tex2D(_SSP, screenPos - right     );
				float4  m = tex2D(_SSP, screenPos             );
				float4  r = tex2D(_SSP, screenPos + right     );

				float4 lb = tex2D(_SSP, screenPos - right - up);
				float4  b = tex2D(_SSP, screenPos         - up);
				float4 rb = tex2D(_SSP, screenPos + right - up);

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

				return float4((normalize(cross(dx, dz)) + 1) / 2, 1); // 1 : valid, 0 : invalid
            }
            ENDCG
        }
    }
}
