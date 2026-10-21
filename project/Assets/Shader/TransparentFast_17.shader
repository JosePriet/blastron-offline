Shader "Custom/TransparentFast" {
Properties {
 _MainTex ("Main", 2D) = "white" {}
}
SubShader {
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  ZTest Always
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  sampler2D _MainTex;
  struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
  v2f vert (appdata_base v) { v2f o; o.pos = mul(UNITY_MATRIX_MVP, v.vertex); o.uv = v.texcoord.xy; return o; }
  fixed4 frag (v2f i) : COLOR { return tex2D(_MainTex, i.uv); }
  ENDCG
 }
}
Fallback "Unlit/Transparent"
}
