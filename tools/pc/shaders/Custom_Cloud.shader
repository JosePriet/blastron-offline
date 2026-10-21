Shader "Custom/Cloud" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
SubShader {
 LOD 200
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask 0
 }
 Pass {
  Blend SrcAlpha OneMinusSrcAlpha
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  sampler2D _MainTex;
  fixed4 _Color;
  struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
  v2f vert (appdata_full v) { v2f o; o.pos = mul(UNITY_MATRIX_MVP, v.vertex); o.uv = float2(v.texcoord.x - _Time.x, v.texcoord.y); o.color = v.color; return o; }
  fixed4 frag (v2f i) : COLOR { return tex2D(_MainTex, i.uv) * i.color * _Color; }
  ENDCG
 }
}
}
