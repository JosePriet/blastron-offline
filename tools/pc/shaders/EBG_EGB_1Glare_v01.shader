Shader "EBG/EGB_1Glare_v01" {
Properties {
 _MainTex ("Decal 0---------------------------------------------------------------------------------------------------", 2D) = "white" {}
 _Mask0 ("Mask 0 ", 2D) = "black" {}
 _Color0 ("Color 0 ", Color) = (1,1,1,1)
 _Vector0 ("Vector 0 XY", Vector) = (1,0,1,1)
 _Speed0 ("Distance 0 ", Float) = 1
 _Scale0 ("Scale Decal 0 XY", Vector) = (1,1,1,1)
 _Offset0 ("Offset Decal 0 XY", Vector) = (0,0,1,1)
}
SubShader {
 Tags { "QUEUE"="Transparent" "RenderType"="Transparent" }
 Pass {
  Name "2DECALS"
  Tags { "LIGHTMODE"="Always" }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  sampler2D _MainTex;
  sampler2D _Mask0;
  float4 _MainTex_ST;
  fixed4 _Color0;
  float4 _Vector0;
  float _Speed0;
  float4 _Scale0;
  float4 _Offset0;
  struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
  v2f vert (appdata_base v) { v2f o; o.pos = mul(UNITY_MATRIX_MVP, v.vertex); o.uv = TRANSFORM_TEX(v.texcoord, _MainTex); return o; }
  fixed4 frag (v2f i) : COLOR {
   float2 p = i.uv * _Scale0.xy + _Vector0.xy * _Speed0 + _Offset0.xy;
   return fixed4(_Color0.rgb, tex2D(_MainTex, p).a * tex2D(_Mask0, i.uv).a * _Color0.a);
  }
  ENDCG
 }
}
Fallback "Diffuse"
}
