Shader "Custom/AimAssist" {
Properties {
 _MainTex ("Main", 2D) = "white" {}
 _Color ("Main Color", Color) = (0,1,0.9922,1)
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
  fixed4 _Color;
  struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
  v2f vert (appdata_base v) { v2f o; o.pos = mul(UNITY_MATRIX_MVP, v.vertex); o.uv = v.texcoord.xy; return o; }
  fixed4 frag (v2f i) : COLOR {
   fixed4 col = tex2D(_MainTex, i.uv);
   col.a = col.a * (1.0 / unity_Scale.w);
   return col * _Color;
  }
  ENDCG
 }
}
}
