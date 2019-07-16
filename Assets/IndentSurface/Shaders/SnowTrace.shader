Shader "IndentSurface/SnowTrace"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		_SSN("SSN", 2D) = "white" {}

		_SpecGlossMap("Specular", 2D) = "white" {}

		_BumpScale("Bump Scale", Float) = 1.0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}
			LOD 100

			Pass
			{
				Tags { "LightMode" = "ForwardBase" }

				CGPROGRAM

				#pragma multi_compile_fwdbase

				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "AutoLight.cginc"

				sampler2D _MainTex;
				sampler2D _SSN;
				sampler2D _SpecGlossMap;

				float4 _MainTex_ST;
				float4 _SpecGlossMap_ST;

				float _BumpScale;

				struct appdata
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;

					float3 worldPos : TEXCOORD0;
					float3 worldNormal : TEXCOORD1;
					float2 uv_MainTex : TEXCOORD2;
					float2 uv_SpecGlossMap : TEXCOORD3;
					float4 screenPos : TEXCOORD4;
				};

				v2f vert(appdata v)
				{
					v2f o;

					o.pos = UnityObjectToClipPos(v.vertex);

					o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

					o.worldNormal = UnityObjectToWorldNormal(v.normal);

					o.uv_MainTex = TRANSFORM_TEX(v.uv, _MainTex);

					o.uv_SpecGlossMap = TRANSFORM_TEX(v.uv, _SpecGlossMap);

					o.screenPos = ComputeScreenPos(o.pos);

					return o;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed2 screenPos = IN.screenPos.xy / IN.screenPos.w;
					
					fixed4 val = tex2D(_SSN, screenPos);
					fixed3 worldNormal;
					// 由于 Bilinear Filter，invalid texel 的 w 小于 1
					if (val.w < 1)
						worldNormal = normalize(IN.worldNormal);
					else
						worldNormal = 2 * val.xyz - 1; // decode

					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(IN.worldPos));
					fixed3 viewDir = normalize(UnityWorldSpaceViewDir(IN.worldPos));

					// Lambertian diffuse
					float cosTheta = max(0, dot(worldNormal, lightDir));
					float3 albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
					fixed3 diffuse = albedo * cosTheta;

					// Bline-Phong specular
					float3 halfDir = normalize(viewDir + lightDir);
					float4 KsGloss = tex2D(_SpecGlossMap, IN.uv_SpecGlossMap);
					float3 Ks = KsGloss.rgb;
					float gloss = KsGloss.a;
					float shiness = 32.0 * gloss;
					fixed3 specular = Ks * pow(max(0, dot(worldNormal, halfDir)), shiness);

					// ambient
					fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;

					fixed3 rst = ambient + (diffuse + specular) * _LightColor0.rgb;

					return fixed4(rst, 1);
				}
				ENDCG
			}
		}
}
