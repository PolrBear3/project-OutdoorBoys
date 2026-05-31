Shader "Unlit/Background"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        _PixelSize ("Pixel Size", Float) = 8
        _Speed ("Animation Speed", Float) = 0.4
        _UVScale ("UV Scale", Float) = 1.0
        _Brightness ("Brightness", Float) = 1.0

        _ColorA ("Color A", Color) = (0.212, 0.0, 0.212, 1)
        _ColorB ("Color B", Color) = (0.8, 0.25, 0.5, 1)
        _ColorC ("Color C", Color) = (1.0, 0.85, 0.95, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                fixed4 color      : COLOR;
                UNITY_FOG_COORDS(2)
                float4 vertex     : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _PixelSize;
            float _Speed;
            float _UVScale;
            float _Brightness;

            float4 _ColorA;
            float4 _ColorB;
            float4 _ColorC;

            float4 colormap(float x)
            {
                x = saturate(x);

                if (x < 0.5)
                    return lerp(_ColorA, _ColorB, x / 0.5);
                else
                    return lerp(_ColorB, _ColorC, (x - 0.5) / 0.5);
            }

            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 ip = floor(p);
                float2 u = frac(p);
                u = u * u * (3.0 - 2.0 * u);

                float res = lerp(
                    lerp(rand(ip), rand(ip + float2(1.0, 0.0)), u.x),
                    lerp(rand(ip + float2(0.0, 1.0)), rand(ip + float2(1.0, 1.0)), u.x),
                    u.y
                );

                return res * res;
            }

            float2 rotateMul(float2 p, float scale)
            {
                float2x2 mtx = float2x2(0.80, 0.60, -0.60, 0.80);
                return mul(mtx, p) * scale;
            }

            float fbm(float2 p)
            {
                float f = 0.0;
                float t = _Time.y * _Speed;

                f += 0.500000 * noise(p + t);       p = rotateMul(p, 2.02);
                f += 0.031250 * noise(p);           p = rotateMul(p, 2.01);
                f += 0.250000 * noise(p);           p = rotateMul(p, 2.03);
                f += 0.125000 * noise(p);           p = rotateMul(p, 2.01);
                f += 0.062500 * noise(p);           p = rotateMul(p, 2.04);
                f += 0.015625 * noise(p + sin(t));

                return f / 0.96875;
            }

            float pattern(float2 p)
            {
                return fbm(p + fbm(p + fbm(p)));
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 spriteTex = tex2D(_MainTex, i.uv);

                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 fragCoord = screenUV * _ScreenParams.xy;

                float2 pixelCoord = floor(fragCoord / _PixelSize) * _PixelSize + _PixelSize * 0.5;
                float2 uv = (pixelCoord / _ScreenParams.x) * _UVScale;

                float shade = pattern(uv);
                fixed4 col = colormap(shade) * _Brightness;

                // keep sprite shape
                col.a *= spriteTex.a;

                // optional: keep sprite tint/color support
                col *= i.color;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}