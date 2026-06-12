Shader "Custom/BrownShader"
{
    Properties
    {
        // インスペクターから設定できる変数
        _ColorSpace ("Color Space", Color) = (1,1,1,1)
        _ColorDisplace ("Color Displace", Color) = (1,1,1,1)
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _Space("Space", Float) = 0
        _DisplaceSpace("Displace Space", Float) = 0
        _DisplaceValue("Displace Value", Float) = 0
        _DisplaceSpeed("Displace Speed", Float) = 1
        _Frequency("Frequency", Float) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off
        Cull Off

        Pass
        {
            Name "CustomPostProcess"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // ▼修正1: Propertiesと名前を完全に一致させる（古い _Color は削除）
            CBUFFER_START(UnityPerMaterial)
                half4 _ColorSpace;
                half4 _ColorDisplace;
                float _Space;
                float _DisplaceSpace;
                float _DisplaceValue;
                float _DisplaceSpeed;
                float _Frequency;
            CBUFFER_END
            
            // 普通のテクスチャなので _X は外す
            TEXTURE2D(_NoiseTexture);
            SAMPLER(sampler_NoiseTexture);

            half4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                
                // 時間経過でUVを動かす
                float2 uv_freq = fmod((uv + float2(0, _Time.x) * _DisplaceSpeed) * _Frequency, _Space + _DisplaceSpace);
                
                float2 uv_neo;
                half4 color_neo;
                
                if(uv_freq.y > _Space)
                {
                    uv_neo = uv + float2(_DisplaceValue, 0.0);
                    // _ColorDisplace を適用
                    color_neo = half4(_ColorDisplace.r, _ColorDisplace.g, _ColorDisplace.b, 1.0);
                }
                else
                {
                    uv_neo = uv;
                    // ▼修正2: _ColrSpace のスペルミスを修正
                    color_neo = half4(_ColorSpace.r, _ColorSpace.g, _ColorSpace.b, 1.0);
                }

                // カメラの元の画像の色を取得
                half4 cameraColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv_neo);

                // ▼修正3: 存在しない _Color を消し、シンプルに掛け合わせる
                return cameraColor * color_neo;
            }
            ENDHLSL
        }
    }
}