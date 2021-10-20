Shader "Unlit/FMChromaKey"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _thresh ("Threshold", Range (0, 16)) = 0.8
        _slope ("Slope", Range (0, 1)) = 0.2
        _keyingColor ("Key Colour", Color) = (0,1,0,1)
	}
	SubShader
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			//#include "UnityCG.cginc"

			sampler2D _MainTex;
			float3 _keyingColor;
            float _thresh;
            float _slope;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				float d = abs(length(abs(_keyingColor.rgb -  col.rgb)));
				float edge0 = _thresh * (1.0 - _slope);

				col.a = smoothstep(edge0, _thresh, d);
				return col;
			}
			ENDCG
		}
	}
}
