Shader "Custom/Territory" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _Color ("Main Color", Color) = (1,1,1,1)
}
SubShader {
 LOD 200
 Tags { "RenderType"="Opaque" }
 CGPROGRAM
 #pragma surface surf Lambert
 sampler2D _MainTex;
 fixed4 _Color;
 struct Input { float2 uv_MainTex; float4 color : COLOR; };
 void surf (Input IN, inout SurfaceOutput o) {
  fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
  o.Albedo = c.rgb * IN.color.rgb * _Color.rgb;
  o.Alpha = c.a * _Color.a;
 }
 ENDCG
}
}
