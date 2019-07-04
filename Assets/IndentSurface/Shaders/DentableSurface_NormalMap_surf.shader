Shader "IndentSurface/DentableSurface_NormalMap_surf"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_SpecGlossMap("Specular", 2D) = "white" {}
		_IndentNormalMap("Indent Normal Map", 2D) = "bump" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _SpecGlossMap;
		sampler2D _IndentNormalMap;

        struct Input
        {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_IndentNormalMap;
        };

        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

			// specualr and smoothness
			float4 specGloss = tex2D(_SpecGlossMap, IN.uv_MainTex);
			o.Specular = specGloss.rgb;
			o.Smoothness = specGloss.a;

            o.Alpha = c.a;

			// blend bumpmap and indent normal map
			float3 bumpNormal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap)).rgb;
			float3 indentNormal = UnpackNormal(tex2D(_IndentNormalMap, IN.uv_IndentNormalMap)).rgb;
			o.Normal = normalize(bumpNormal + indentNormal - float3(0,0,1));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
