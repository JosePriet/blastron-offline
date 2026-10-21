Shader "Custom/SolidColour" {
Properties {
 _Color ("Main Color", Color) = (0,1,0,1)
}
SubShader { 
 Pass {
  Color [_Color]
 }
}
}