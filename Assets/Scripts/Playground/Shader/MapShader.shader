Shader "Unlit/MapShader"
{
    Properties
    {
        _BorderSize ("BorderSize", float) = 0.1

        _BorderCol ("BorderColor", Color) = (0, 0, 0, 1)
        _PassableCol ("PassableColor", Color) = (1, 1, 1, 1)
        _XPassableCol ("ImpassableColor", Color) = (0.5, 0.5, 0.5, 1)
        _PathCol ("PathColor", Color) = (0.8, 0.5, 0.4, 1)
        _StartCol ("StartColor", Color) = (0, 1, 0, 1)
        _EndCol ("EndColor", Color) = (1, 0, 0, 1)
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

            StructuredBuffer<int2> _PathBuffer;

            uint _PathIndex;

            uint _PathCount;

            uint _ShowPath;

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
            half4 _PathCol;
            half4 _StartCol;
            half4 _EndCol;

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

                if (_ShowPath == 1)
                {
                    int2 startIndex = _PathBuffer[_PathIndex];
                    int2 endIndex = _PathBuffer[_PathCount - 1];

                    if (startIndex.x == intVec.x && startIndex.y == intVec.y)
                    {
                        return S_Player;
                    }

                    if (endIndex.x == intVec.x && endIndex.y == intVec.y)
                    {
                        return S_End;
                    }


                    for (uint i = _PathIndex + 1; i < _PathCount - 1; i++)
                    {
                        int2 mapPathIndex = _PathBuffer[i];

                        if (mapPathIndex.x == intVec.x && mapPathIndex.y == intVec.y)
                        {
                            return S_Path;
                        }
                    }
                }

                int bufferIndex = intVec.x + intVec.y * _SizeX.x;
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
                else if (state == S_Path)
                {
                    col = _PathCol;
                }
                else if (state == S_Player)
                {
                    col = _StartCol;
                }
                else if (state == S_End)
                {
                    col = _EndCol;
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
