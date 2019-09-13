Shader "Klei/PostFX/GridCompositor" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass {
			GpuProgramID 37371
			Program "vp" {
				SubProgram "d3d11 " {
					"!!vs_4_0
					//
					// Generated by Microsoft (R) D3D Shader Disassembler
					//
					//
					// Input signature:
					//
					// Name                 Index   Mask Register SysValue  Format   Used
					// -------------------- ----- ------ -------- -------- ------- ------
					// POSITION                 0   xyzw        0     NONE   float   xyz 
					// TEXCOORD                 0   xy          1     NONE   float   xy  
					//
					//
					// Output signature:
					//
					// Name                 Index   Mask Register SysValue  Format   Used
					// -------------------- ----- ------ -------- -------- ------- ------
					// SV_Position              0   xyzw        0      POS   float   xyzw
					// TEXCOORD                 0   xy          1     NONE   float   xy  
					//
					vs_4_0
					dcl_constantbuffer CB0[4], immediateIndexed
					dcl_constantbuffer CB1[21], immediateIndexed
					dcl_input v0.xyz
					dcl_input v1.xy
					dcl_output_siv o0.xyzw, position
					dcl_output o1.xy
					dcl_temps 2
					mul r0.xyzw, v0.yyyy, cb0[1].xyzw
					mad r0.xyzw, cb0[0].xyzw, v0.xxxx, r0.xyzw
					mad r0.xyzw, cb0[2].xyzw, v0.zzzz, r0.xyzw
					add r0.xyzw, r0.xyzw, cb0[3].xyzw
					mul r1.xyzw, r0.yyyy, cb1[18].xyzw
					mad r1.xyzw, cb1[17].xyzw, r0.xxxx, r1.xyzw
					mad r1.xyzw, cb1[19].xyzw, r0.zzzz, r1.xyzw
					mad o0.xyzw, cb1[20].xyzw, r0.wwww, r1.xyzw
					mov o1.xy, v1.xyxx
					ret 
					// Approximately 0 instruction slots used"
				}
			}
			Program "fp" {
				SubProgram "d3d11 " {
					"!!ps_4_0
					//
					// Generated by Microsoft (R) D3D Shader Disassembler
					//
					//
					// Input signature:
					//
					// Name                 Index   Mask Register SysValue  Format   Used
					// -------------------- ----- ------ -------- -------- ------- ------
					// SV_Position              0   xyzw        0      POS   float       
					// TEXCOORD                 0   xy          1     NONE   float   xy  
					//
					//
					// Output signature:
					//
					// Name                 Index   Mask Register SysValue  Format   Used
					// -------------------- ----- ------ -------- -------- ------- ------
					// SV_Target                0   xyzw        0   TARGET   float   xyzw
					//
					ps_4_0
					dcl_constantbuffer CB0[7], immediateIndexed
					dcl_sampler s0, mode_default
					dcl_resource_texture2d (float,float,float,float) t0
					dcl_input_ps linear v1.xy
					dcl_output o0.xyzw
					dcl_temps 2
					add r0.xy, -v1.xyxx, l(1.000000, 1.000000, 0.000000, 0.000000)
					mad r0.xy, cb0[2].zwzz, r0.xyxx, cb0[2].xyxx
					mul r0.zw, r0.xxxy, cb0[3].zzzw
					ge r0.zw, r0.zzzw, -r0.zzzw
					movc r0.zw, r0.zzzw, cb0[3].zzzw, -cb0[3].zzzw
					div r1.xy, l(1.000000, 1.000000, 1.000000, 1.000000), r0.zwzz
					mul r1.xy, r0.xyxx, r1.xyxx
					mad r0.xy, r0.xyxx, cb0[3].xyxx, -cb0[6].xyxx
					dp2 r0.x, r0.xyxx, r0.xyxx
					sqrt r0.x, r0.x
					div r0.x, r0.x, cb0[4].y
					min r0.x, r0.x, l(1.000000)
					add r0.x, -r0.x, l(1.000000)
					frc r1.xy, r1.xyxx
					mul r0.yz, r0.zzwz, r1.xxyx
					div r0.yz, r0.yyzy, cb0[3].zzwz
					add r0.yz, r0.yyzy, l(0.000000, -0.500000, -0.500000, 0.000000)
					add r0.yz, r0.yyzy, r0.yyzy
					add r0.w, -cb0[4].x, l(1.000000)
					add_sat r0.yz, -r0.wwww, |r0.yyzy|
					mad r1.xy, r0.yzyy, l(-2.000000, -2.000000, 0.000000, 0.000000), l(3.000000, 3.000000, 0.000000, 0.000000)
					mul r0.yz, r0.yyzy, r0.yyzy
					mul r0.yz, r0.yyzy, r1.xxyx
					max r0.y, r0.z, r0.y
					mul r0.yzw, r0.yyyy, cb0[5].xxyz
					add r1.x, -cb0[4].z, cb0[4].w
					mad r0.x, r0.x, r1.x, cb0[4].z
					sample r1.xyzw, v1.xyxx, t0.xyzw, s0
					mad o0.xyz, r0.yzwy, r0.xxxx, r1.xyzx
					mov o0.w, r1.w
					ret 
					// Approximately 0 instruction slots used"
				}
			}
		}
	}
}