Shader "IndentSurface/DentableSurface_NormalMap_vf"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}

        _BumpMap("Bumpmap", 2D) = "bump" {}
		_BumpScale("Bump Scale", Float) = 1.0

        _IndentNormalMap("Indent Normal Map", 2D) = "bump" {}
        _SpecGlossMap("Specular", 2D) = "white" {}

		// xzwh
		_IndentNormalMapOffset("IndentNormalMapOffset", Vector) = (0,0,0,0)
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

			#include "Lighting.cginc"
			#include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 worldPos : TEXCOORD0;
                // these three vectors will hold a 3x3 rotation matrix
                // that transforms from tangent to world space
                half3 tspace0 : TEXCOORD1; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD2; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD3; // tangent.z, bitangent.z, normal.z

                float2 uv_MainTex : TEXCOORD4;
                float2 uv_BumpMap : TEXCOORD5;
                float2 uv_IndentNormalMap : TEXCOORD6;
                float2 uv_SpecGlossMap : TEXCOORD7;

                float4 pos : SV_POSITION;

				SHADOW_COORDS(8)
            };

            sampler2D _MainTex;
            sampler2D _BumpMap;
            sampler2D _IndentNormalMap;
            sampler2D _SpecGlossMap;

            float4 _MainTex_ST;
            float4 _BumpMap_ST;
            float4 _IndentNormalMap_ST;
            float4 _SpecGlossMap_ST;

			uniform float4 _IndentNormalMapOffset;
			float _BumpScale;

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                half3 wNormal = UnityObjectToWorldNormal(v.normal);
                half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
                // compute bitangent from cross product of normal and tangent
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);

                o.uv_MainTex = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_BumpMap = TRANSFORM_TEX(v.uv, _BumpMap);

				o.uv_IndentNormalMap = (o.worldPos.xz - _IndentNormalMapOffset.xy) / _IndentNormalMapOffset.zw + 0.5;

                o.uv_SpecGlossMap = TRANSFORM_TEX(v.uv, _SpecGlossMap);

				TRANSFER_SHADOW(o);

                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // normal
                float3 bumpNormal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap)).rgb;
				bumpNormal = normalize(float3(_BumpScale * bumpNormal.xy, bumpNormal.z));

				float3 indentNormal = float3(0, 0, 1);
				if(IN.uv_IndentNormalMap.x>0 && IN.uv_IndentNormalMap.x<1 && IN.uv_IndentNormalMap.y>0 && IN.uv_IndentNormalMap.y<1)
					indentNormal = UnpackNormal(tex2D(_IndentNormalMap, IN.uv_IndentNormalMap)).rgb;

                float3 tNormal = normalize(bumpNormal + indentNormal - float3(0, 0, 1));

                float3 worldNormal;
                worldNormal.x = dot(IN.tspace0, tNormal);
                worldNormal.y = dot(IN.tspace1, tNormal);
                worldNormal.z = dot(IN.tspace2, tNormal);
                worldNormal = normalize(worldNormal);

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
