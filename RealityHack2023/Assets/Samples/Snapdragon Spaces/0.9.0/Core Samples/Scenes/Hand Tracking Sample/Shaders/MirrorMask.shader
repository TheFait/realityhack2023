/******************************************************************************
 * File: MirrorMask.shader
 * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 * 
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

Shader "Snapdragon Spaces/Mirror Mask" {
    SubShader {
        Tags { "Queue" = "Geometry-1" }

        ColorMask 0
        ZWrite Off

        Stencil {
            Ref 1
            Comp always
            Pass replace
        }

        CGPROGRAM
        #pragma surface surf Lambert
 
        sampler2D _AlbedoTexture;
 
        struct Input {
            float2 uv_MainTexture;
        };
 
        void surf (Input input, inout SurfaceOutput output) {
            output.Albedo = tex2D(_AlbedoTexture, input.uv_MainTexture).rgb;
        }
        ENDCG
    }
}
