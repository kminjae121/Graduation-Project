//////////////////////////////////////////////////////
// MK Toon Depth Normals Data				       	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_DEPTH_NORMALS_IO
	#define MK_TOON_DEPTH_NORMALS_IO

	#include_with_pragmas "../Core.hlsl"
	
	/////////////////////////////////////////////////////////////////////////////////////////////
	// INPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexInputDepthNormals
	{
		float4 vertex : POSITION;
		half3 normal : NORMAL;
		#if defined(MK_TCM) || defined(MK_TCD)
			#if defined(MK_SECONDARY_UV_SET)
				float2 texcoord0 : TEXCOORD0;
				float2 texcoord7 : TEXCOORD7;
			#else
				float2 texcoord0 : TEXCOORD0;
			#endif
		#endif

		#if defined(MK_PARALLAX) || defined(MK_TBN) || defined(MK_TANGENT_VERTEX)
			half4 tangent : TANGENT;
		#endif

		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	/////////////////////////////////////////////////////////////////////////////////////////////
	// OUTPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexOutputDepthNormals
	{
		float4 svPositionClip : SV_POSITION;
		#if defined(MK_TCM) || defined(MK_TCD)
			float4 uv : TEXCOORD0;
		#endif

		#if defined(MK_PARALLAX)
			half3 viewTangent : TEXCOORD1;
		#endif
		half3 normalWorld : TEXCOORD2;
		#ifdef MK_LIT
			#if defined(MK_TBN)
				half3 tangentWorld : TEXCOORD3;
				half3 bitangentWorld : TEXCOORD4;
			#endif
		#endif

		#ifdef MK_BARYCENTRIC_POS_CLIP
			float4 positionClip : TEXCOORD5;
		#endif
		#ifdef MK_POS_NULL_CLIP
			float4 nullClip : TEXCOORD6;
		#endif

		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};
#endif