Shader "Custom/FlashSprite" 
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Наши новые параметры для вспышки
        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashAmount ("Flash Amount", Range(0,1)) = 0
    }
    SubShader 
    {
        Tags 
        {
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }
        
        // Настройки прозрачности для спрайтов
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Lighting Off

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t 
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f 
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _FlashColor;
            float _FlashAmount;

            v2f vert (appdata_t v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target 
            {
                // Получаем оригинальный цвет пикселя
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
                
                // Плавно смешиваем оригинальный цвет с цветом вспышки
                col.rgb = lerp(col.rgb, _FlashColor.rgb, _FlashAmount);
                
                return col;
            }
            ENDCG
        }
    }
}