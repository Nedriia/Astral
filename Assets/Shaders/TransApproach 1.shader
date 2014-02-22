Shader "TransApproach/Diffuse" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _Opacity ("Opacity", Range(0.1, 1.0)) = 0.3
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
 fixed4 _Color;
 uniform float _Opacity;

 struct Input {
	float2 uv_MainTex;
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
   o.Alpha = IN.fooAlpha;
 }
 ENDCG
}

Fallback "Forward"
}