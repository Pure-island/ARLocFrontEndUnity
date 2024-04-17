Shader "Custom/VertexColor" 
{
	SubShader 
	{		
		CGPROGRAM
 
		#pragma surface surf Lambert vertex:vert
		#pragma target 3.0
 
 		#include "UnityCG.cginc"
            
		struct Input {
			float4 vertColor;
		};
 
		void vert(inout appdata_full v, out Input o)
		{
			o.vertColor = v.color;
		}
 
		half3 surf (Input IN, inout SurfaceOutput o)
		{
			o.Albedo = IN.vertColor.rgb;
			half3 col = o.Albedo;
			return col;
		}
		ENDCG
	}
}