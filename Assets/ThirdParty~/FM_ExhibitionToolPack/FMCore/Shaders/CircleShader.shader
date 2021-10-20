Shader "Unlit/CircleShader"
{
    Properties
    {
        _BoundColor("Bound Color", Color) = (0,0.5843137254901961,1,1)
        _BgColor("Background Color", Color) = (0.1176470588235294,0,0.5882352941176471,1)
        _circleSizePercent("Circle Size Percent", Range(0, 100)) = 50
        _circleSizePercent2("Circle Size Percent2", Range(0, 100)) = 0
        _border("Anti Alias Border Threshold", Range(0.00001, 5)) = 0.01
    }
        SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            float _border;

            fixed4 _BoundColor;
            fixed4 _BgColor;
            float _circleSizePercent;
            float _circleSizePercent2;

            struct v2f
            {
                float2 uv : TEXCOORD0;
            };

            v2f vert(
                float4 vertex : POSITION, // vertex position input
                float2 uv : TEXCOORD0, // texture coordinate input
                out float4 outpos : SV_POSITION // clip space position output
            )
            {
                v2f o;
                o.uv = uv;
                if(o.uv.x<0){
                    o.uv.x = 1-o.uv.x;
                }
                if(o.uv.y<0){
                    o.uv.y = 1-o.uv.y;
                }
                
                
                outpos = UnityObjectToClipPos(vertex);
                return o;
            }

            float2 antialias(float radius, float borderSize, float dist)
            {
                float t = smoothstep(radius + borderSize, radius - borderSize, dist);
                return t;
            }

            fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
                float4 col;
                float2 center = _ScreenParams.xy / 2;
                
                float2 uv = i.uv;
                //uv.x -= 0.5;
                //uv.y -= 0.5;
                center = float2(0.5,0.5);
                
                //float maxradius = length(center);

                //float radius = maxradius*(_circleSizePercent / 100);
                float radius = _circleSizePercent/200;
                float radius2 = _circleSizePercent2/200;
                
                //float dis = distance(screenPos.xy, center);
                float dis = distance(uv.xy, center);

                if (dis > radius) {
                    float aliasVal = antialias(radius, _border, dis);
                    col = lerp(_BoundColor, _BgColor, aliasVal); //NOT needed but incluse just incase
                }
                else {
                    float aliasVal = antialias(radius, _border, dis);
                    col = lerp(_BoundColor, _BgColor, aliasVal);
                    
                    if(dis< radius2){
                        col.a = 0;
                    }
                }
                
                
                
                return col;

            }
            ENDCG
        }
    }
}