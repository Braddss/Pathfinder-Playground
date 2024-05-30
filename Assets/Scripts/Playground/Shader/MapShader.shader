Shader "Unlit/MapShader"
{
    Properties
    {
        _BorderSize ("BorderSize", float) = 0.1

        _BorderCol ("BorderColor", Color) = (0, 0, 0, 1)
        _PassableCol ("PassableColor", Color) = (1, 1, 1, 1)
        _XPassableCol ("ImpassableColor", Color) = (0.5, 0.5, 0.5, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            StructuredBuffer<int> _MapBuffer;

            StructuredBuffer<int> _PathBuffer;

            uint _PathIndex;

            uint _PathCount;

            static const int S_XPassable    = 0x00000000u;
            static const int S_Passable     = 0x00000001u;
            static const int S_Player       = 0x00000002u;
            static const int S_End          = 0x00000003u;
            static const int S_Path         = 0x00000004u;
            static const int S_Border       = 0x000000010u;


            int _SizeX;
            int _SizeY;

            half _BorderSize;

            half4 _BorderCol;
            half4 _PassableCol;
            half4 _XPassableCol;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 vertPos : vertPos;
            };

            float2 NormPos(float3 planePos)
            {
                return float2(planePos.x, planePos.z) / 10 + float2(0.5, 0.5);
            }

            int GetState(float2 inputPos)
            {
                float2 min = float2(0.01, 0.01);
                float2 max = float2(0.99, 0.99);

                if (inputPos.x < min.x || inputPos.y < min.y || inputPos.x > max.x || inputPos.y > max.y)
                {
                    return S_Border;
                }

                inputPos = ((inputPos - min) / (max - min)) * int2(_SizeX, _SizeY);

                float2 modPos = inputPos % float2(1, 1);
               
                float borderSize = _BorderSize / 2;

                if (modPos.x < borderSize || modPos.y < borderSize || modPos.x > 1 - borderSize || modPos.y > 1 - borderSize)
                {
                    return S_Border;
                }

                int2 intVec = inputPos;
                int bufferIndex = intVec.x + intVec.y * _SizeX.x;

                //
                for (uint i = _PathIndex; i < _PathCount; i++)
                {
                    int mapPathIndex = _PathBuffer[i];

                    if (bufferIndex == mapPathIndex)
                    {
                        return S_Path;
                    }
                }
                
                //

                

                int bufferResult = _MapBuffer[bufferIndex];


                if (bufferResult == S_Passable)
                {
                    return S_Passable;
                }
                else if (bufferResult == S_XPassable)
                {
                    return S_XPassable;
                }

                return S_Passable;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertPos = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pos = NormPos(i.vertPos);

                int state = GetState(pos);

                fixed4 col = _BorderCol;


                if (state == S_Border)
                {
                    col = _BorderCol;
                }
                else if (state == S_Passable)
                {
                    col = _PassableCol;
                }
                else if (state == S_XPassable)
                {
                    col = _XPassableCol;
                }

                //fixed4 col = fixed4(pos.x, 1, pos.y, 1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);



                return col;
            }
            ENDCG
        }
    }
}
