Shader "Custom/PowerArrow" {
Properties {
 _MainTex ("Main", 2D) = "white" {}
 _Ramp ("Ramp", 2D) = "white" {}
 _Power ("Power", Float) = 0
}
SubShader {
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  ZWrite Off
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  sampler2D _MainTex;
  sampler2D _Ramp;
  float _Power;
  struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
  v2f vert (appdata_base v) { v2f o; o.pos = mul(UNITY_MATRIX_MVP, v.vertex); o.uv = v.texcoord.xy; return o; }
  fixed4 frag (v2f i) : COLOR {
   fixed4 ramp = tex2D(_Ramp, i.uv * _Power);
   float u1 = (1.0 - _Power) + i.uv.x * _Power;
   return tex2D(_MainTex, float2(u1, i.uv.y)) * ramp;
  }
  ENDCG
 }
}
}
