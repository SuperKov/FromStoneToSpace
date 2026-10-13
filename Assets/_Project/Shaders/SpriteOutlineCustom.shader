Shader "Custom/SpriteOutlineCustom"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        
        // --- Настройки обводки ---
        [HDR] _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
        Outline ("Outline", Float) = 0 // Тумблер из твоего скрипта (0 или 1)
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha // Стандартный блендинг для 2D спрайтов

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            // Подключаем наши свойства из блока Properties
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float Outline;

            fixed4 frag(v2f IN) : SV_Target
            {
                // Читаем цвет и альфу самого спрайта
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Если обводка выключена, просто отдаем спрайт (оптимизация)
                if (Outline == 0) 
                {
                    c.rgb *= c.a;
                    return c;
                }

                // --- Алгоритм 8-сторонней обводки ---
                float width = _OutlineWidth;

                // Считываем альфу соседей (Крест + Диагонали)
                float aUp    = tex2D(_MainTex, IN.texcoord + float2(0, width)).a;
                float aDown  = tex2D(_MainTex, IN.texcoord + float2(0, -width)).a;
                float aRight = tex2D(_MainTex, IN.texcoord + float2(width, 0)).a;
                float aLeft  = tex2D(_MainTex, IN.texcoord + float2(-width, 0)).a;
                
                float aUR = tex2D(_MainTex, IN.texcoord + float2(width, width)).a;
                float aUL = tex2D(_MainTex, IN.texcoord + float2(-width, width)).a;
                float aDR = tex2D(_MainTex, IN.texcoord + float2(width, -width)).a;
                float aDL = tex2D(_MainTex, IN.texcoord + float2(-width, -width)).a;

                // Находим максимальную прозрачность вокруг текущего пикселя
                float maxAlpha = max(max(max(aUp, aDown), max(aRight, aLeft)), 
                                 max(max(aUR, aUL), max(aDR, aDL)));

                // Маска: рисуем обводку только там, где есть сосед, но НЕТ самого спрайта
                float outlineAlpha = maxAlpha * (1.0 - c.a);
                
                // Применяем цвет и яркость обводки
                fixed4 finalOutline = _OutlineColor;
                finalOutline.a *= outlineAlpha;
                finalOutline.rgb *= finalOutline.a;

                c.rgb *= c.a;
                
                // Складываем спрайт с обводкой
                return c + finalOutline;
            }
        ENDCG
        }
    }
}