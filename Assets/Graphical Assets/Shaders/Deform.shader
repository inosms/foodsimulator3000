Shader "Custom/Deform" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_SkinWidth("Skin Width", Range(0.01,0.5)) = 0.1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#include "UnityCG.cginc"

		// Shader model 5 required for Compute Buffers
		#pragma target 5.0
		
		#if defined(SHADER_API_D3D11) | defined(SHADER_API_GLES3)
		uniform StructuredBuffer<float3> cagePoints;//in object space
		uniform StructuredBuffer<float> weights;//weights of cagePoints for each vertex
		#endif


		struct vertexIn
		{
			uint id : SV_VertexID;
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			fixed4 color : COLOR;
		};

		float _SkinWidth;

		void vert (inout vertexIn v) 
		{
			#if defined(SHADER_API_D3D11) | defined(SHADER_API_GLES3)
			//Get the buffers' dimensions
			uint cagePoints_count;
			uint weights_count;
			uint stride;
			cagePoints.GetDimensions(cagePoints_count, stride);
			weights.GetDimensions(weights_count, stride);
			
			float4 newPos = float4(0, 0, 0, 1.0f);

			for(int i = 0; i < cagePoints_count; i++)
			{
				float weight = weights[v.id * cagePoints_count + i];
				newPos.xy += weight * cagePoints[i].xy;
			}

			v.vertex.xy = newPos.xy;
			v.vertex.xy *= (1.0f + _SkinWidth);
			#endif
		}
		
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
