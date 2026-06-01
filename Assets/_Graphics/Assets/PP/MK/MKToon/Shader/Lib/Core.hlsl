//////////////////////////////////////////////////////
// MK Toon Core						       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_CORE
	#define MK_TOON_CORE

	#ifdef _MK_SURFACE_TYPE_TRANSPARENT
		//Fix up unity internal directives
		#ifndef _SURFACE_TYPE_TRANSPARENT
			#define _SURFACE_TYPE_TRANSPARENT
		#endif
	#endif

	#if defined(MK_URP)
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#if UNITY_VERSION >= 202220 && defined(LOD_FADE_CROSSFADE)
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
		#endif
	#elif defined(MK_LWRP)
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
	#else
		#include "UnityCG.cginc"
	#endif

	#if defined(MK_URP) && defined(MK_PARTICLES) && UNITY_VERSION >= 202020
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ParticlesInstancing.hlsl"
	#endif
	#if UNITY_VERSION < 202020 && defined(MK_PARTICLES)
		void ParticleInstancingSetup() {}
	#endif

	//CurvedWorldSupport
	#ifndef MK_META_PASS
	//CurvedWorldDefinitions
	#endif
	//

	struct MKInputDataWrapper
	{
		float2 normalizedScreenSpaceUV;
		float3 positionWS;
	};

	struct MKFragmentOutput
	{
		half4 svTarget0 : SV_Target0;
		#if UNITY_VERSION >= 202220 && defined(_WRITE_RENDERING_LAYERS)
			#if UNITY_VERSION >= 60020000
				uint svTarget1 : SV_Target1;
			#else //UNITY_VERSION >= 202220
				float4 svTarget1 : SV_Target1;
			#endif
		#endif
	};

	#include "Config.hlsl"
	#include "Pipeline.hlsl"
	#include "Uniform.hlsl"
	#include "Common.hlsl"

#endif