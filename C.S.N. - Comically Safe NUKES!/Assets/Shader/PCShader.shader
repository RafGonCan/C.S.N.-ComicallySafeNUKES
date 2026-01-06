Shader "Custom/SimpleTerminalFlicker"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FlickerSpeed ("Flicker Speed", Float) = 8.0
        _FlickerAmount ("Flicker Amount", Range(0, 0.5)) = 0.2
        _Scanlines ("Scanlines", Range(0, 1)) = 0.3
        _ScreenColor ("Screen Color", Color) = (0.15, 0.70, 0.15, 1.0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
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
            
            sampler2D _MainTex;
            float _FlickerSpeed;
            float _FlickerAmount;
            float _Scanlines;
            float4 _ScreenColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Apply green tint
                col.rgb *= _ScreenColor.rgb;
                
                // FLICKER EFFECT
                float time = _Time.y * _FlickerSpeed;
                
                // Multiple sine waves for organic flicker
                float flicker = sin(time) * 0.5 + 0.5;
                flicker *= sin(time * 2.3) * 0.3 + 0.7;
                flicker *= sin(time * 0.7) * 0.4 + 0.6;
                
                // Random flicker bursts
                float randomFlicker = rand(float2(time, i.uv.y));
                if (randomFlicker > 0.97) {
                    flicker *= 0.5 + 0.5 * sin(time * 60.0);
                }
                
                // Apply flicker
                col.rgb *= 1.0 - (_FlickerAmount * (1.0 - flicker));
                
                // Scanlines
                float scanline = sin(i.uv.y * 500.0 + time * 2.0) * 0.5 + 0.5;
                col.rgb *= 1.0 - (scanline * _Scanlines * 0.3);
                
                return col;
            }
            ENDCG
        }
    }
}