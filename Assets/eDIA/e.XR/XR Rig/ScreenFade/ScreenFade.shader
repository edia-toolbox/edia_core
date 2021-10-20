Shader "Custom/ScreenFade"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_FadeColor ("Color", Color) = (0,0,0,0)
		_Intensity ("Intensity", float) = 0
	}

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct AppToVertex
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexToFragment
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			VertexToFragment vert (AppToVertex v)
			{
				VertexToFragment o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _MaskTex;
			half4 _FadeColor;
			half _Intensity;

			fixed3 frag(VertexToFragment i) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, i.uv);
				fixed3 lerpedColor = lerp(color.rgb, _FadeColor.rgb, _Intensity);
				return lerpedColor;
			}
			ENDCG
		}
	}
}