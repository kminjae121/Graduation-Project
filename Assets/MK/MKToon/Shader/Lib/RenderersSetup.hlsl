//////////////////////////////////////////////////////
// MK Toon Renderers Setup					       	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_RENDERERS_SETUP
	#define MK_TOON_RENDERERS_SETUP
	#if SHADER_TARGET >= 45
		#pragma exclude_renderers gles d3d9 d3d11_9x psp2 n3ds wiiu
	#elif SHADER_TARGET >= 35
		#pragma only_renderers glcore gles3
	#else
		#pragma only_renderers gles d3d9 d3d11_9x psp2 n3ds wiiu
	#endif
#endif