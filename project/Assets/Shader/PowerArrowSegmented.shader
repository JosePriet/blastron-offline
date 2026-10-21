Shader "Custom/PowerArrowSeg" {
Properties {
 _MainTex ("Main", 2D) = "white" {}
 _Ramp ("Ramp", 2D) = "white" {}
 _Scales ("Scales", Vector) = (1.5,2.82,0,0)
}
SubShader {
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  ZTest Always
  ZWrite Off
  Cull Off
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  sampler2D _MainTex;
  sampler2D _Ramp;
  float4 _Scales;
  struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
  v2f vert (appdata_base v) { v2f o; o.pos = mul(UNITY_MATRIX_MVP, v.vertex); o.uv = v.texcoord.xy; return o; }
  fixed4 frag (v2f i) : COLOR {
   fixed4 col = tex2D(_MainTex, i.uv);
   float2 r = float2(((1.0 / unity_Scale.w) - _Scales.x) / (_Scales.y - _Scales.x), 0.5);
   return col * tex2D(_Ramp, r);
  }
  ENDCG
 }
}
}
