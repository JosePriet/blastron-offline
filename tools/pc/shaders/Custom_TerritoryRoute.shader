Shader "Custom/TerritoryRoute" {
Properties {
 _MainTex ("Main", 2D) = "white" {}
 _Color ("Main Color", Color) = (0,1,0.9922,1)
}
SubShader {
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 CGINCLUDE
 #include "UnityCG.cginc"
 sampler2D _MainTex;
 fixed4 _Color;
 struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
 v2f vert (appdata_full v) { v2f o; o.pos = mul(UNITY_MATRIX_MVP, v.vertex); o.uv = float2(v.texcoord.x - _Time.y, v.texcoord.y); o.color = v.color; return o; }
 ENDCG
 Pass {
  ZTest Always
  ZWrite Off
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  fixed4 frag (v2f i) : COLOR { return tex2D(_MainTex, i.uv) * i.color * _Color; }
  ENDCG
 }
 Pass {
  ZWrite Off
  Cull Front
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  fixed4 frag (v2f i) : COLOR { return tex2D(_MainTex, i.uv) * i.color * fixed4(1, 0, 0, 1); }
  ENDCG
 }
}
}
