Shader "TransApproach/BumpedSpecular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_Opacity ("Opacity", Range(0.1, 1.0)) = 0.5
}
SubShader {
 Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
 Cull Off
 //AlphaTest Less 0.7
 Fog { Mode Off }
 Blend SrcAlpha OneMinusSrcAlpha
 LOD 200
 
 CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
#pragma exclude_renderers gles
#pragma vertex vert
#pragma surface surf Lambert

sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color;
half _Shininess;
uniform float _Opacity;

 struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float3 fooAlpha;
 };

 void vert (inout appdata_full v, out Input o) {
   // Transform to camera space
   float3 foo = mul(UNITY_MATRIX_MVP, v.vertex);
   o.fooAlpha = foo.z * _Opacity;
 }

 void surf (Input IN, inout SurfaceOutput o) {
   fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
   o.Albedo = c.rgba;
   o.Gloss = c.a;
   o.Alpha = IN.fooAlpha;
   o.Specular = _Shininess;
   o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
 }
 ENDCG
}

Fallback "Forward"
}