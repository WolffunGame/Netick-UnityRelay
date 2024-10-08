﻿	// Copyright 2016-2023 Kronnect - All Rights Reserved.
	
	#include "UnityCG.cginc"
	#include "BeautifyAdvancedParams.cginc"
	#include "BeautifyOrtho.cginc"

	UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomTex);
	half4 	  _BloomTex_TexelSize;
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomTex1);
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomTex2);
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomTex3);
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomTex4);
	half4     _MainTex_TexelSize;
	half4     _MainTex_ST;
    half4 	  _Bloom;
	half4 	  _BloomWeights;
	half4 	  _BloomWeights2;
    half4 	  _BloomTint;
    half4 	  _AFTint;
	half      _BlurScale;
	half3     _BloomTint0, _BloomTint1, _BloomTint2, _BloomTint3, _BloomTint4, _BloomTint5;

	#if (BEAUTIFY_BLOOM_USE_DEPTH || BEAUTIFY_BLOOM_USE_LAYER) && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
		UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
		half      _BloomDepthThreshold;
	    half      _BloomNearThreshold;
	#endif

	#if BEAUTIFY_BLOOM_USE_LAYER && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
		UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomSourceTex);
		UNITY_DECLARE_DEPTH_TEXTURE(_BloomSourceDepth);
		UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomSourceTexRightEye);
		UNITY_DECLARE_DEPTH_TEXTURE(_BloomSourceDepthRightEye);
		half      _BloomLayerZBias;
	#endif

	// Wrappers for bloom masks
	#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define BEAUTIFY_SAMPLE_MASK(tex,uv) UNITY_SAMPLE_TEX2DARRAY(tex, float3((uv).x, (uv).y, 0))
	    #define BEAUTIFY_DEPTH_MASK(tex,uv) UNITY_SAMPLE_TEX2DARRAY(tex, float3((uv).x, (uv).y, 0)).r
	#else
	    #define BEAUTIFY_SAMPLE_MASK(tex,uv) UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, uv)
		#define BEAUTIFY_DEPTH_MASK(tex,uv) SAMPLE_DEPTH_TEXTURE(tex, uv)
	#endif


    struct appdata {
    	float4 vertex : POSITION;
		half2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
	struct v2f {
	    float4 pos : SV_POSITION;
	    half2 uv: TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};

	struct v2fCross {
	    float4 pos : SV_POSITION;
	    half2 uv: TEXCOORD0;
	    half2 uv1: TEXCOORD1;
	    half2 uv2: TEXCOORD2;
	    half2 uv3: TEXCOORD3;
	    half2 uv4: TEXCOORD4;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};

	#define USES_SINGLE_PASS (UNITY_SINGLE_PASS_STEREO || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED))

	struct v2fLum {
		float4 pos : SV_POSITION;
		half2 uv: TEXCOORD0;
		#if (BEAUTIFY_BLOOM_USE_DEPTH || BEAUTIFY_BLOOM_USE_LAYER) && defined(BEAUTIFY_DEPTH_BASED_SHARPEN) 
			half2 depthUV: TEXCOORD1;
		#endif
		#if BEAUTIFY_BLOOM_USE_LAYER && USES_SINGLE_PASS && defined(BEAUTIFY_DEPTH_BASED_SHARPEN) 
			half2 depthUVNonStereo: TEXCOORD2;
		#endif
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};

	struct v2fCrossLum {
		float4 pos : SV_POSITION;
		half2 uv: TEXCOORD0;
		half2 uv1: TEXCOORD1;
		half2 uv2: TEXCOORD2;
		half2 uv3: TEXCOORD3;
		half2 uv4: TEXCOORD4;
		#if (BEAUTIFY_BLOOM_USE_DEPTH || BEAUTIFY_BLOOM_USE_LAYER) && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
			half2 depthUV: TEXCOORD5;
		#endif
		#if BEAUTIFY_BLOOM_USE_LAYER && USES_SINGLE_PASS && defined(BEAUTIFY_DEPTH_BASED_SHARPEN) 
			half2 depthUVNonStereo: TEXCOORD6;
		#endif
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};


	v2f vert(appdata v) {
    	v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    	o.pos = UnityObjectToClipPos(v.vertex);
    	o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
		
		#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Depth texture is inverted WRT the main texture
    	    o.uv.y = 1.0 - o.uv.y;
    	}
    	#endif    	
    	return o;
	}

	v2fLum vertLum(appdata v) {
		v2fLum o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = UnityObjectToClipPos(v.vertex);

		#if BEAUTIFY_BLOOM_USE_LAYER && USES_SINGLE_PASS && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
			o.uv = v.texcoord;
			o.depthUVNonStereo = v.texcoord;
			o.depthUV = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
		#else
			o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
			#if (BEAUTIFY_BLOOM_USE_DEPTH || BEAUTIFY_BLOOM_USE_LAYER) && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
			o.depthUV = o.uv;
			#endif
		#endif
		#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0) {
				// Depth texture is inverted WRT the main texture
				o.uv.y = 1.0 - o.uv.y;
			}
		#endif
		return o;
	}

	inline half Brightness(half3 c) {
		return max(c.r, max(c.g, c.b));
	}


	inline half3 ColorAboveThreshold(half3 c, half brightness) {
		half threshold = _Bloom.w;

		#if BEAUTIFY_BLOOM_PROP_THRESHOLDING
	        half cs = clamp(brightness - 0.5 * threshold, 0.0, threshold);
			cs = 0.5 * cs * cs / (threshold + 0.0001);
			c *= max(brightness - threshold, cs) / max(brightness, 0.0001);
		#else
			c = max(c - threshold, 0);
		#endif

		return c;
	}

	half4 fragLum (v2fLum i) : SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		#if BEAUTIFY_BLOOM_USE_LAYER && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
            half4 c;
            if (unity_StereoEyeIndex == 0) {
		        c = BEAUTIFY_SAMPLE_MASK(_BloomSourceTex, i.uv);
            } else {
                c = BEAUTIFY_SAMPLE_MASK(_BloomSourceTexRightEye, i.uv);
            }
		#else
		    half4 c = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
		#endif
		c = clamp(c, 0.0.xxxx, _BloomWeights2.zzzz);
   		#if UNITY_COLORSPACE_GAMMA
			c.rgb = GammaToLinearSpace(c.rgb);
		#endif

		#if (BEAUTIFY_BLOOM_USE_DEPTH || BEAUTIFY_BLOOM_USE_LAYER) && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
	        half depth01 = Linear01Depth(BEAUTIFY_SCENE_DEPTH(i.depthUV));
		#endif

		#if BEAUTIFY_BLOOM_USE_DEPTH && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
			c.rgb *= max(0, 1.0 - depth01 * _BloomDepthThreshold);
			c.rgb *= min(1.0, depth01 /  (_BloomNearThreshold / _ProjectionParams.z));
		#endif

		#if BEAUTIFY_BLOOM_USE_LAYER && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
            half depth02;
			#if USES_SINGLE_PASS
                if (unity_StereoEyeIndex == 0) {
                    depth02 = Linear01Depth(BEAUTIFY_DEPTH_MASK(_BloomSourceDepth, i.depthUVNonStereo));
                } else {
                    depth02 = Linear01Depth(BEAUTIFY_DEPTH_MASK(_BloomSourceDepthRightEye, i.depthUVNonStereo));
                }
			#else
                if (unity_StereoEyeIndex == 0) {
    				depth02 = Linear01Depth(BEAUTIFY_DEPTH_MASK(_BloomSourceDepth, i.depthUV));
                } else {
                    depth02 = Linear01Depth(BEAUTIFY_DEPTH_MASK(_BloomSourceDepthRightEye, i.depthUV));
                }
			#endif
			half isTransparent = (depth02 >= 1) && any(c.rgb>0);
			half nonEclipsed = isTransparent || (depth01 > depth02 - _BloomLayerZBias);
			c.rgb *= nonEclipsed;
		#endif

		c.a = Brightness(c.rgb);
		c.rgb = ColorAboveThreshold(c.rgb, c.a);
   		return c;
   	}

   	v2fCross vertCross(appdata v) {
    	v2fCross o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    	o.pos = UnityObjectToClipPos(v.vertex);
		#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Texture is inverted WRT the main texture
    	    v.texcoord.y = 1.0 - v.texcoord.y;
    	}
    	#endif   
    	o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
		half3 offsets = _MainTex_TexelSize.xyx * half3(1,1,-1);
		#if USES_SINGLE_PASS
			offsets.xz *= 2.0;
		#endif
		o.uv1 = UnityStereoScreenSpaceUVAdjust(v.texcoord - offsets.xy, _MainTex_ST);
		o.uv2 = UnityStereoScreenSpaceUVAdjust(v.texcoord - offsets.zy, _MainTex_ST);
		o.uv3 = UnityStereoScreenSpaceUVAdjust(v.texcoord + offsets.zy, _MainTex_ST);
		o.uv4 = UnityStereoScreenSpaceUVAdjust(v.texcoord + offsets.xy, _MainTex_ST);
		return o;
	}

	v2fCrossLum vertCrossLum(appdata v) {
		v2fCrossLum o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = UnityObjectToClipPos(v.vertex);
		#if BEAUTIFY_BLOOM_USE_LAYER && defined(BEAUTIFY_DEPTH_BASED_SHARPEN) && USES_SINGLE_PASS
			o.depthUVNonStereo = v.texcoord;
			o.depthUV = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
			#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0) {
					// Texture is inverted WRT the main texture
					v.texcoord.y = 1.0 - v.texcoord.y;
				}
			#endif   
			o.uv = v.texcoord;
			half3 offsets = _MainTex_TexelSize.xyx * half3(1, 1, -1);
			o.uv1 = v.texcoord - offsets.xy;
			o.uv2 = v.texcoord - offsets.zy;
			o.uv3 = v.texcoord + offsets.zy;
			o.uv4 = v.texcoord + offsets.xy;
		#else
			#if (BEAUTIFY_BLOOM_USE_DEPTH || BEAUTIFY_BLOOM_USE_LAYER) && defined(BEAUTIFY_DEPTH_BASED_SHARPEN) 
				o.depthUV = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
			#endif
			#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0) {
					// Texture is inverted WRT the main texture
					v.texcoord.y = 1.0 - v.texcoord.y;
				}
			#endif   
			o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
			half3 offsets = _MainTex_TexelSize.xyx * half3(1, 1, -1);
			#if USES_SINGLE_PASS
				offsets.xz *= 2.0;
			#endif
			o.uv1 = UnityStereoScreenSpaceUVAdjust(v.texcoord - offsets.xy, _MainTex_ST);
			o.uv2 = UnityStereoScreenSpaceUVAdjust(v.texcoord - offsets.zy, _MainTex_ST);
			o.uv3 = UnityStereoScreenSpaceUVAdjust(v.texcoord + offsets.zy, _MainTex_ST);
			o.uv4 = UnityStereoScreenSpaceUVAdjust(v.texcoord + offsets.xy, _MainTex_ST);
		#endif
		return o;

	}

   	half4 fragLumAntiflicker(v2fCrossLum i) : SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		#if BEAUTIFY_BLOOM_USE_LAYER && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
			half4 c1, c2, c3, c4;
            if (unity_StereoEyeIndex == 0) {
                c1 = BEAUTIFY_SAMPLE_MASK(_BloomSourceTex, i.uv1);
			    c2 = BEAUTIFY_SAMPLE_MASK(_BloomSourceTex, i.uv2);
			    c3 = BEAUTIFY_SAMPLE_MASK(_BloomSourceTex, i.uv3);
			    c4 = BEAUTIFY_SAMPLE_MASK(_BloomSourceTex, i.uv4);
            } else {
                c1 = BEAUTIFY_SAMPLE_MASK(_BloomSourceTexRightEye, i.uv1);
                c2 = BEAUTIFY_SAMPLE_MASK(_BloomSourceTexRightEye, i.uv2);
                c3 = BEAUTIFY_SAMPLE_MASK(_BloomSourceTexRightEye, i.uv3);
                c4 = BEAUTIFY_SAMPLE_MASK(_BloomSourceTexRightEye, i.uv4);
            }
		#else
			half4 c1 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv1);
			half4 c2 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv2);
			half4 c3 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv3);
			half4 c4 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv4);
		#endif

		c1 = clamp(c1, 0.0.xxxx, _BloomWeights2.zzzz);
		c2 = clamp(c2, 0.0.xxxx, _BloomWeights2.zzzz);
		c3 = clamp(c3, 0.0.xxxx, _BloomWeights2.zzzz);
		c4 = clamp(c4, 0.0.xxxx, _BloomWeights2.zzzz);

		#if defined(BEAUTIFY_DEPTH_BASED_SHARPEN)

			#if (BEAUTIFY_BLOOM_USE_DEPTH || BEAUTIFY_BLOOM_USE_LAYER) && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
		        half depth01 = Linear01Depth(BEAUTIFY_SCENE_DEPTH(i.depthUV));
			#endif

			#if BEAUTIFY_BLOOM_USE_DEPTH && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
				half depthAtten = max(0, 1.0 - depth01 * _BloomDepthThreshold);
				c1.rgb *= depthAtten;
				c2.rgb *= depthAtten;
				c3.rgb *= depthAtten;
				c4.rgb *= depthAtten;
				float nearAtten = min(1.0, depth01 /  (_BloomNearThreshold / _ProjectionParams.z));
				c1.rgb *= nearAtten;
				c2.rgb *= nearAtten;
				c3.rgb *= nearAtten;
				c4.rgb *= nearAtten;
			#endif

			#if BEAUTIFY_BLOOM_USE_LAYER && defined(BEAUTIFY_DEPTH_BASED_SHARPEN)
				half depth02;
				#if USES_SINGLE_PASS
					if (unity_StereoEyeIndex == 0) {
						depth02 = Linear01Depth(BEAUTIFY_DEPTH_MASK(_BloomSourceDepth, i.depthUVNonStereo));
					} else {
						depth02 = Linear01Depth(BEAUTIFY_DEPTH_MASK(_BloomSourceDepthRightEye, i.depthUVNonStereo));
					}
				#else
					depth02 = Linear01Depth(BEAUTIFY_DEPTH_MASK(_BloomSourceDepth, i.depthUV));
				#endif
				half isTransparent = (depth02 >= 1) && any(c1.rgb>0);
				half nonEclipsed = isTransparent || (depth01 > depth02 - _BloomLayerZBias );
				c1.rgb *= nonEclipsed;
				c2.rgb *= nonEclipsed;
				c3.rgb *= nonEclipsed;
				c4.rgb *= nonEclipsed;
			#endif

		#endif
		
		c1.a = Brightness(c1.rgb);
		c2.a = Brightness(c2.rgb);
		c3.a = Brightness(c3.rgb);
		c4.a = Brightness(c4.rgb);
	    
	    half w1 = 1.0 / (c1.a + 1.0);
	    half w2 = 1.0 / (c2.a + 1.0);
	    half w3 = 1.0 / (c3.a + 1.0);
	    half w4 = 1.0 / (c4.a + 1.0);

	    half dd  = 1.0 / (w1 + w2 + w3 + w4);
	    c1 = (c1 * w1 + c2 * w2 + c3 * w3 + c4 * w4) * dd;
	    
   		#if UNITY_COLORSPACE_GAMMA
			c1.rgb = GammaToLinearSpace(c1.rgb);
		#endif

		c1.rgb = ColorAboveThreshold(c1.rgb, c1.a);

		c1 = min(c1, _Bloom.y);

   		return c1;
	}
	

	half4 fragBloomCompose (v2f i) : SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 b0 = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _BloomTex  , i.uv );
		half4 b1 = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _BloomTex1 , i.uv );
		half4 b2 = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _BloomTex2 , i.uv );
		half4 b3 = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _BloomTex3 , i.uv );
		half4 b4 = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _BloomTex4 , i.uv );
		half4 b5 = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex   , i.uv );
		b0.rgb *= _BloomTint0;
		b1.rgb *= _BloomTint1;
		b2.rgb *= _BloomTint2;
		b3.rgb *= _BloomTint3;
		b4.rgb *= _BloomTint4;
		b5.rgb *= _BloomTint5;
		half4 pixel = b0 * _BloomWeights.x + b1 * _BloomWeights.y + b2 * _BloomWeights.z + b3 * _BloomWeights.w + b4 * _BloomWeights2.x + b5 * _BloomWeights2.y;
		pixel.rgb = lerp(pixel.rgb, Brightness(pixel.rgb) * _BloomTint.rgb, _BloomTint.a);
		return pixel;
	}


	half4 fragResample(v2fCross i) : SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 c1 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv1);
		half4 c2 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv2);
		half4 c3 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv3);
		half4 c4 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv4);
			    
	    half w1 = 1.0 / (c1.a + 1.0);
	    half w2 = 1.0 / (c2.a + 1.0);
	    half w3 = 1.0 / (c3.a + 1.0);
	    half w4 = 1.0 / (c4.a + 1.0);
	    
	    half dd  = 1.0 / (w1 + w2 + w3 + w4);
	    c1 = (c1 * w1 + c2 * w2 + c3 * w3 + c4 * w4) * dd;
		#if defined(APPLY_TINT_COLOR)
			c1.rgb = lerp(c1.rgb, Brightness(c1.rgb) * _BloomTint.rgb, _BloomTint.a);
		#endif
		return c1;
	}


	half4 fragResampleAF(v2fCross i) : SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 c1 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv1);
		half4 c2 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv2);
		half4 c3 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv3);
		half4 c4 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv4);
			    
	    half w1 = 1.0 / (c1.a + 1.0);
	    half w2 = 1.0 / (c2.a + 1.0);
	    half w3 = 1.0 / (c3.a + 1.0);
	    half w4 = 1.0 / (c4.a + 1.0);
	    
	    half dd  = 1.0 / (w1 + w2 + w3 + w4);
	    c1 = (c1 * w1 + c2 * w2 + c3 * w3 + c4 * w4) * dd;
	    c1.rgb = lerp(c1.rgb, Brightness(c1.rgb) * _AFTint.rgb, _AFTint.a);
	    c1.rgb *= _Bloom.xxx;
	    return c1;
	}

	half4 fragCopy(v2f i) : SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 color = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
		#if defined(APPLY_TINT_COLOR)
			color.rgb = lerp(color.rgb, Brightness(color.rgb) * _BloomTint.rgb, _BloomTint.a);
		#endif
		return color;
	}

	half4 fragDebugBloom (v2f i) : SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_BloomTex, i.uv) * _Bloom.xxxx;
	}
	
	half4 fragResampleFastAF(v2f i) : SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 c = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
	    c.rgb = lerp(c.rgb, Brightness(c.rgb) * _AFTint.rgb, _AFTint.a);
	    c.rgb *= _Bloom.xxx;
	    return c;
	}	
	
	v2fCross vertBlurH(appdata v) {
    	v2fCross o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    	o.pos = UnityObjectToClipPos(v.vertex);
		#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Texture is inverted WRT the main texture
    	    v.texcoord.y = 1.0 - v.texcoord.y;
    	}
    	#endif   
    	o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
		half2 inc = half2(_MainTex_TexelSize.x * 1.3846153846 * _BlurScale, 0);	

    	o.uv1 = UnityStereoScreenSpaceUVAdjust(v.texcoord - inc, _MainTex_ST);	
    	o.uv2 = UnityStereoScreenSpaceUVAdjust(v.texcoord + inc, _MainTex_ST);	
		half2 inc2 = half2(_MainTex_TexelSize.x * 3.2307692308 * _BlurScale, 0);	

		o.uv3 = UnityStereoScreenSpaceUVAdjust(v.texcoord - inc2, _MainTex_ST);
    	o.uv4 = UnityStereoScreenSpaceUVAdjust(v.texcoord + inc2, _MainTex_ST);	
		return o;
	}	
	
	v2fCross vertBlurV(appdata v) {
    	v2fCross o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    	o.pos = UnityObjectToClipPos(v.vertex);
		#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Texture is inverted WRT the main texture
    	    v.texcoord.y = 1.0 - v.texcoord.y;
    	}
    	#endif   
    	o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
    	half2 inc = half2(0, _MainTex_TexelSize.y * 1.3846153846 * _BlurScale);	
    	o.uv1 = UnityStereoScreenSpaceUVAdjust(v.texcoord - inc, _MainTex_ST);	
    	o.uv2 = UnityStereoScreenSpaceUVAdjust(v.texcoord + inc, _MainTex_ST);	
    	half2 inc2 = half2(0, _MainTex_TexelSize.y * 3.2307692308 * _BlurScale);	
    	o.uv3 = UnityStereoScreenSpaceUVAdjust(v.texcoord - inc2, _MainTex_ST);	
    	o.uv4 = UnityStereoScreenSpaceUVAdjust(v.texcoord + inc2, _MainTex_ST);	
    	return o;
	}
	
	half4 fragBlur (v2fCross i): SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 pixel = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv) * 0.2270270270
					+ (UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv1) + UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv2)) * 0.3162162162
					+ (UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv3) + UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv4)) * 0.0702702703;
   		return pixel;
	}	
