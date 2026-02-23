Shader "CircusCompany/Ghost Shader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		_RimPower ("Rim Power", Float) = 3.0
		_RimAlpha ("Rim Alpha", Float) = 1.0
	}

	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass {
			ZWrite On
			ColorMask 0
		}

		LOD 200

		CGPROGRAM
		
		#pragma surface surf Standard fullforwardshadows alpha:blend
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 viewDir;
		};

		sampler2D _MainTex;
		
		half _Glossiness;
		half _Metallic;
		half4 _Color;
		sampler2D _BumpMap;
		half4 _RimColor;
		float _RimPower, _RimAlpha;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
			o.Emission=_RimColor.rgb * pow (rim, _RimPower);
			rim=1.0f- _RimAlpha*dot(o.Normal,IN.viewDir);
			if(rim>0){
				o.Alpha=_Color.a*rim;
			}else{
				o.Alpha=0.0f;
			}
		}
		ENDCG

	}
	Fallback "Legacy Shaders/Transparent/VertexLit"
}
