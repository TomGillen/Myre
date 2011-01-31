#include "EncodeNormals.fxh"
#include "GammaCorrection.fxh"
#include "FullScreenQuad.fxh"

#define BIAS 0.00008 //0.8

float3 Colour;
float3 LightPosition;
float3 LightDirection;
float Angle;
float Range;
float LightFalloffFactor;
float2 ShadowMapSize;
float3 CameraPosition : CAMERAPOSITION;
bool EnableProjectiveTexturing;
bool EnableShadows;

float FarClip : FARCLIP;
float4x4 WorldViewProjection : WORLDVIEWPROJECTION;
float4x4 WorldView : WORLDVIEW;
float4x4 CameraViewToLightProjection;
float4x4 CameraViewToLightView;
float LightFarClip;

texture Depth : GBUFFER_DEPTH;
sampler depthSampler = sampler_state
{
	Texture = (Depth);
	MinFilter = Point;
	MipFilter = None;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Normals : GBUFFER_NORMALS;
sampler normalSampler = sampler_state
{
	Texture = (Normals);
	MinFilter = Point;
	MipFilter = None;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Diffuse : GBUFFER_DIFFUSE;
sampler diffuseSampler = sampler_state
{
	Texture = (Diffuse);
	MinFilter = Point;
	MipFilter = None;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Mask;
sampler maskSampler = sampler_state
{
	Texture = (Mask);
	MinFilter = Linear;
	MipFilter = None;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture ShadowMap;
sampler shadowSampler = sampler_state
{
	Texture = (ShadowMap);
	MinFilter = Point;
	MipFilter = None;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

// Calculates the shadow occlusion using bilinear PCF
float CalcShadowTermPCF(float fLightDepth, float2 vTexCoord)
{
    float fShadowTerm = 0.0f;

    // transform to texel space
    float2 vShadowMapCoord = ShadowMapSize * vTexCoord;
    
    // Determine the lerp amounts           
    float2 vLerps = frac(vShadowMapCoord);

    // read in bilerp stamp, doing the shadow checks
    float fSamples[4];
    
    fSamples[0] = (tex2D(shadowSampler, vTexCoord).x + BIAS < fLightDepth) ? 0.0f: 1.0f;  
    fSamples[1] = (tex2D(shadowSampler, vTexCoord + float2(1.0/ShadowMapSize.x, 0)).x + BIAS < fLightDepth) ? 0.0f: 1.0f;  
    fSamples[2] = (tex2D(shadowSampler, vTexCoord + float2(0, 1.0/ShadowMapSize.y)).x + BIAS < fLightDepth) ? 0.0f: 1.0f;  
    fSamples[3] = (tex2D(shadowSampler, vTexCoord + float2(1.0/ShadowMapSize.x, 1.0/ShadowMapSize.y)).x + BIAS < fLightDepth) ? 0.0f: 1.0f;  
    
    // lerp between the shadow values to calculate our light amount
    fShadowTerm = lerp( lerp( fSamples[0], fSamples[1], vLerps.x ),
                              lerp( fSamples[2], fSamples[3], vLerps.x ),
                              vLerps.y );                              
                                
    return fShadowTerm;                                 
}

// Calculates the shadow term using PCF with edge tap smoothing
float CalcShadowTermSoftPCF(float fLightDepth, float2 vTexCoord, int iSqrtSamples)
{
    float fShadowTerm = 0.0f;  
        
    float fRadius = (iSqrtSamples - 1.0f) / 2;        
    for (float y = -fRadius; y <= fRadius; y++)
    {
        for (float x = -fRadius; x <= fRadius; x++)
        {
            float2 vOffset = 0;
            vOffset = float2(x, y);                
            vOffset /= ShadowMapSize;
            float2 vSamplePoint = vTexCoord + vOffset;            
            float fDepth = tex2D(shadowSampler, vSamplePoint).x;
            float fSample = (fLightDepth <= fDepth + BIAS);
            
            // Edge tap smoothing
            float xWeight = 1;
            float yWeight = 1;
            
            if (x == -fRadius)
                xWeight = 1 - frac(vTexCoord.x * ShadowMapSize.x);
            else if (x == fRadius)
                xWeight = frac(vTexCoord.x * ShadowMapSize.x);
                
            if (y == -fRadius)
                yWeight = 1 - frac(vTexCoord.y * ShadowMapSize.y);
            else if (y == fRadius)
                yWeight = frac(vTexCoord.y * ShadowMapSize.y);
                
            fShadowTerm += fSample * xWeight * yWeight;
        }                                            
    }        
    
    fShadowTerm /= (iSqrtSamples * iSqrtSamples );
    
    return fShadowTerm;
}


float4 CalculateLighting(float2 texCoord, float3 viewPosition)
{
	float3 surfaceToLight = LightPosition - viewPosition;
	float distance = length(surfaceToLight);	
	float3 L = surfaceToLight / distance;

	if (dot(L, LightDirection) > Angle)
	{
		float4 sampledNormals = tex2D(normalSampler, texCoord);
		float3 normal = DecodeNormal(sampledNormals.xy);
		float specularIntensity = sampledNormals.z;

		float4 sampledDiffuse = tex2D(diffuseSampler, texCoord);
		float3 diffuse = GammaToLinear(sampledDiffuse.xyz);
		float specularPower = sampledDiffuse.w * 255;

		float attenuation = saturate(LightFalloffFactor / (distance * distance));

		float3 V = normalize(CameraPosition - viewPosition);
		float3 R = normalize(reflect(-L, normal));

		float NdL = max(dot(normal, L), 0);
		float RdV = max(dot(R, V), 0);

		float3 light = Colour * attenuation;

		float4 projectedTexCoord;
		if (EnableProjectiveTexturing || EnableShadows)
		{
			projectedTexCoord = mul(float4(viewPosition, 1), CameraViewToLightProjection);
			projectedTexCoord /= projectedTexCoord.w;
			projectedTexCoord.x = (1 + projectedTexCoord.x) / 2;
			projectedTexCoord.y = (1 - projectedTexCoord.y) / 2;
		}

		if (EnableProjectiveTexturing)
		{
			light *= tex2D(maskSampler, projectedTexCoord.xy).rgb;
		}

		if (EnableShadows)
		{
			//float4 lightProjectedPosition = mul(viewPosition, CameraViewToLightProjection);
			//float depth = lightProjectedPosition.z / lightProjectedPosition.w;
			//float depth = length(viewPosition - LightPosition);
			//float shadowDepth = tex2D(shadowSampler, projectedTexCoord.xy).x;
			//float4 lightViewPosition = mul(viewPosition, CameraViewToLightView);
			//float depth = -lightViewPosition.z / LightFarClip;

			//light *= step(shadowDepth + 0.001, depth);
			//light *= (depth - 0.01 <= shadowDepth);
			//if (shadowDepth < depth)
			//	light = 0;

			light *= CalcShadowTermSoftPCF(projectedTexCoord.z, projectedTexCoord.xy, 9);
		}
		
		return float4((NdL * diffuse * light) + ((specularIntensity * pow(RdV, specularPower)) * light), 1);
	}
	else
	{
		return float4(0, 0, 0, 0);
	}
}

void GeometryVS(in float3 in_Position : POSITION0,
					  out float4 out_Position : POSITION0,
					  out float3 out_PositionVS : TEXCOORD0,
					  out float4 out_PositionCS : TEXCOORD1)
{
	out_Position = mul(float4(in_Position, 1), WorldViewProjection);
	out_PositionVS = mul(float4(in_Position, 1), WorldView);
	out_PositionCS = out_Position;
}

void GeometryPS(in float3 in_PositionVS : TEXCOORD0,
						 in float4 in_PositionCS : TEXCOORD1,
						 out float4 out_Colour : COLOR0)
{
	float2 screenPos = in_PositionCS.xy / in_PositionCS.w;
	float2 texCoord;
	texCoord.x = (1 + screenPos.x) / 2 + (0.5 / Resolution.x);
	texCoord.y = (1 - screenPos.y) / 2 + (0.5 / Resolution.y);

	float4 sampledDepth = tex2D(depthSampler, texCoord);
	float3 frustumRay = in_PositionVS.xyz * (FarClip/-in_PositionVS.z);
	float3 viewPosition = frustumRay * sampledDepth.x;

	out_Colour = CalculateLighting(texCoord, viewPosition);
}

void QuadPS(in float2 in_TexCoord : TEXCOORD0,
			in float3 in_FrustumRay : TEXCOORD1,
			out float4 out_Colour : COLOR0)
{
	float4 sampledDepth = tex2D(depthSampler, in_TexCoord);
	float3 viewPosition = in_FrustumRay * sampledDepth.x;

	out_Colour = CalculateLighting(in_TexCoord, viewPosition);
}

technique Geometry
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 GeometryVS();
        PixelShader = compile ps_3_0 GeometryPS();
    }
}

technique Quad
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadFrustumCornerVS();
		PixelShader = compile ps_3_0 QuadPS();
	}
}