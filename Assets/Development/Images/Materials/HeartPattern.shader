Shader "Custom/ScrollingHeart_Fixed"
{
    Properties
    {
        _MainTex ("Heart Texture", 2D) = "white" {}
        _ScrollX ("X Speed", Float) = 0.2
        _ScrollY ("Y Speed", Float) = 0.2
        _Tiling ("Tiling", Float) = 5.0
        _HeartScale ("Heart Scale", Range(0.1, 2.0)) = 1.0
        _Rotation ("Rotation (Degrees)", Range(0, 360)) = 45.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            // _MainTex_TexelSize.z는 가로 픽셀 수, .w는 세로 픽셀 수입니다.
            float4 _MainTex_TexelSize; 
            float _ScrollX, _ScrollY, _Tiling, _HeartScale, _Rotation;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _Tiling + float2(_Time.y * _ScrollX, _Time.y * _ScrollY);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
            // 1. 행마다 0.5씩 밀어서 엇갈리게 만들기 (45도 배열 느낌)
            float row = floor(i.uv.y);
            // fmod는 나머지를 구함. 행 번호를 2로 나누어 0 또는 1에 따라 가로로 0.5 이동
            i.uv.x += fmod(row, 2.0) * 0.5;

            // 2. 개별 타일 중심(0,0) 정렬
            float2 uv = frac(i.uv) - 0.5;

            // 3. 가로세로 비율 보정 (찌그러짐 방지)
            float aspect = _MainTex_TexelSize.z / _MainTex_TexelSize.w;
            uv.x *= aspect;

            // 4. 회전 행렬 계산
            float rad = _Rotation * 0.0174533;
            float s = sin(rad);
            float c = cos(rad);
            float2x2 rotMat = float2x2(c, -s, s, c);
            
            // 5. 회전 적용 및 스케일 조정
            float2 rotatedUV = mul(rotMat, uv) / _HeartScale;

            // 6. 보정했던 비율 복원
            rotatedUV.x /= aspect;
            rotatedUV += 0.5; 

            // 7. 범위 밖 제거
            if (rotatedUV.x < 0 || rotatedUV.x > 1 || rotatedUV.y < 0 || rotatedUV.y > 1) {
                discard;
            }

            return tex2D(_MainTex, rotatedUV);
        }
            ENDCG
        }
    }
}