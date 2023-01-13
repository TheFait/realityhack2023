/******************************************************************************
 * File: StencilEffect.shader
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 * 
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

Shader "Snapdragon Spaces/Stencil Effect" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType" = "Opaque"}
 
        Stencil {
            Ref 1
            Comp equal
        }
 
        CGPROGRAM
        #pragma surface surf Lambert
 
        sampler2D _MainTex;
 
        struct Input {
            float2 uv_MainTexture;
        };

        fixed4 _Color;
 
        void surf(Input input, inout SurfaceOutput output) {
            fixed4 c = tex2D (_MainTex, input.uv_MainTexture) * _Color;
            output.Albedo = c.rgb;
            output.Alpha = c.a;
        }
        ENDCG
    }
}
