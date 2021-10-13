Shader "Unlit/multiTiles4x1"
{
    Properties
    {
        _ScaleX ("ScaleX", float) = 1
        _ScaleY ("ScaleY", float) = 1
        _OffsetX ("OffsetX", float) = 0
        _OffsetY ("OffsetY", float) = 0
        _FlipX ("FlipX", Range(0,1)) = 0
        _FlipY ("FlipY", Range(0,1)) = 0
        _Alpha ("Alpha", Range(0,1)) = 1
        
        _Tile1 ("Tile1", 2D) = "white" {}
        _Tile2 ("Tile2", 2D) = "white" {}
        _Tile3 ("Tile3", 2D) = "white" {}
        _Tile4 ("Tile4", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True"}
        //ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Off

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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _Tile1;
            sampler2D _Tile2;
            sampler2D _Tile3;
            sampler2D _Tile4;
            float _Alpha;
            float _ScaleX;
            float _ScaleY;
            float _OffsetX;
            float _OffsetY;
            float _FlipX;
            float _FlipY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                #if UNITY_UV_STARTS_AT_TOP
                if(_ProjectionParams.x>0){
                }
                #else
                if(_ProjectionParams.x<0){
                    o.uv.y = 1-o.uv.y;
                }
                #endif
                
                if(_FlipX){
                    o.uv.x = 1-o.uv.x;
                }
                if(_FlipY){
                    o.uv.y = 1-o.uv.y;
                }

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4(1,0,0,1);
                float2 uv = i.uv;
                
                
                uv.x=uv.x%1;
                uv.y=uv.y%1;
                
                
                if(uv.x<0){
                    uv.x = 1-uv.x;
                }
                if(uv.y<0){
                    uv.y = 1-uv.y;
                }
                
                if(_OffsetX<0){
                    _OffsetX = 1-_OffsetX;
                }
                if(_OffsetY<0){
                    _OffsetY = 1-_OffsetY;
                }
                
                uv.x = (uv.x + _OffsetX)*_ScaleX;
                uv.y = (uv.y + _OffsetY)*_ScaleY;
                
                uv.x=uv.x%1;
                uv.y=uv.y%1;

                float cutNum = 4.0;
                float gap = 1.0/cutNum;
                
                if(uv.x<gap){
                    uv.x = uv.x * cutNum;
                    col = tex2D(_Tile1, uv);
                }
                else if(uv.x<gap*2){
                    uv.x = (uv.x-gap) * cutNum;
                    col = tex2D(_Tile2, uv);
                }
                else if(uv.x<gap*3){
                    uv.x = (uv.x-gap*2) * cutNum;
                    col = tex2D(_Tile3, uv);
                }
                else if(uv.x<gap*4){
                    uv.x = (uv.x-gap*3) * cutNum;
                    col = tex2D(_Tile4, uv);
                }

                col.a = _Alpha;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
