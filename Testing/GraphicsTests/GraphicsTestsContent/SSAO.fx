// adapted from a RenderMonkey implementation
// by ArKano22, http://www.gamedev.net/community/forums/topic.asp?topic_id=556187

#include "FullScreenQuad.fxh"
#include "EncodeNormals.fxh"

texture Depth : GBUFFER_DEPTH_DOWNSAMPLE;
sampler depthSampler = sampler_state
{
	Texture = (Depth);
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Normals : GBUFFER_NORMALS;
sampler normalSampler = sampler_state
{
	Texture = (Normals);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Random;
sampler randomSampler = sampler_state
{
	Texture = (Random);
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture Lighting : PREVIOUSLIGHTBUFFER;
sampler lightingSampler = sampler_state
{
	Texture = (Lighting);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

float SampleRadius : SSAO_RADIUS;
float Intensity : SSAO_INTENSITY;
float Scale : SSAO_SCALE;

//float DetailSampleRadius : SSAO_DETAILRADIUS = 2.3;
//float DetailIntensity : SSAO_DETAILINTENSITY = 15;
//float DetailScale : SSAO_DETAILSCALE = 1.5;


struct PS_INPUT
{
	float2 uv : TEXCOORD0;
};

struct PS_OUTPUT
{
	float4 color : COLOR0;
};

float3 GetPosition(in float2 uv)
{
	float3 frustumCorner = FrustumCorners[0];
	float x = lerp(frustumCorner.x, -frustumCorner.x, uv.x);
	float y = lerp(frustumCorner.y, -frustumCorner.y, uv.y);

	return tex2D(depthSampler, uv).r * float3(x, y, frustumCorner.z);
}

float3 GetNormal(in float2 uv)
{
	return DecodeNormal(tex2D(normalSampler, uv).xy);
}

float2 GetRandom(in float2 uv)
{
	return normalize(tex2D(randomSampler, Resolution * uv / 64).xy * 2.0f - 1.0f);
}


float DoAmbientOcclusion(in float2 tcoord, in float2 uv, in float3 p, in float3 cnorm)
{
	float2 coord = tcoord + uv;
	float3 diff = GetPosition(coord) - p;
	const float3 v = normalize(diff);
	const float  d = length(diff) * Scale;
	return max(0.0, dot(cnorm,v)) * (1.0/(1.0+d)) * Intensity;
}

float DoAmbientOcclusionDetail(in float2 tcoord, in float2 uv, in float3 p, in float3 cnorm)
{
	float3 diff = GetPosition(tcoord + uv) - p;
	const float3 v = normalize(diff);
	const float  d = length(diff) * DetailScale;
	return max(0.0, dot(cnorm,v)) * (1.0/(1.0+d)) * DetailIntensity;
}

PS_OUTPUT main(PS_INPUT i)
{
	PS_OUTPUT o = (PS_OUTPUT)0;
 
	o.color.rgb = 1.0f;
	const float2 vec[4] = {
		float2(1,0),float2(-1,0),
		float2(0,1),float2(0,-1)
	};

	float3 p = GetPosition(i.uv);
	float3 n = GetNormal(i.uv);
	float2 rand = GetRandom(i.uv);

	float radius = SampleRadius / p.z;
	float detailRadius = DetailSampleRadius / p.z;

	float ao = 0.0f;
	float detailAO = 0.0f;
	int iterations = 4;
	[loop]
	for (int j = 0; j < iterations; ++j)
	{
		float2 coord1 = reflect(vec[j], rand) * radius;
		float2 coord2 = float2(coord1.x*0.707 - coord1.y*0.707, coord1.x*0.707 + coord1.y*0.707);
		ao += DoAmbientOcclusion(i.uv,coord1*0.25, p, n);
		ao += DoAmbientOcclusion(i.uv,coord2*0.5, p, n);
		ao += DoAmbientOcclusion(i.uv,coord1*0.75, p, n);
		ao += DoAmbientOcclusion(i.uv,coord2, p, n); 
	}

	ao /= (float)iterations * 4.0;
	o.color.rgb = 1 - ao;
	return o;
}

technique Technique1
{
	Pass pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 main();
	}
}







/*
#include "FullScreenQuad.fxh"
#include "EncodeNormals.fxh"

float SampleRadius = 1;
float MinThreshold = 0;
float MaxThreshold = 0.9;
float Power = 2;
float Intensity = 1;

float4x4 Projection : PROJECTION;
float FarClip : FARCLIP;
float3 Offsets[8];

texture Depth : GBUFFER_DEPTH;//_DOWNSAMPLE;
sampler depthSampler = sampler_state
{
	Texture = (Depth);
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Normals : GBUFFER_NORMALS;
sampler normalSampler = sampler_state
{
	Texture = (Normals);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture Random;
sampler randomSampler = sampler_state
{
	Texture = (Random);
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

float3 GetPosition(in float2 uv)
{
	float3 frustumCorner = FrustumCorners[0];
	float x = lerp(frustumCorner.x, -frustumCorner.x, uv.x);
	float y = lerp(frustumCorner.y, -frustumCorner.y, uv.y);

	return tex2D(depthSampler, uv).r * float3(x, y, frustumCorner.z);
}

float3 GetNormal(in float2 uv)
{
  return DecodeNormal(tex2D(normalSampler, uv).xy);
}

float3 GetRandom(in float2 uv)
{
	return normalize(tex2D(randomSampler, Resolution * uv / 64).xyz * 2.0f - 1.0f);
}

void AmbientOcclusionPS(in float2 in_TexCoord : TEXCOORD0,
						out float4 out_Colour : COLOR0)
{
	float3 position = GetPosition(in_TexCoord);
	float3 normal = GetNormal(in_TexCoord);
	float3 random = GetRandom(in_TexCoord);

	float totalOcclusion = 0;
	float weights = 0;
	for (int i = 0; i < 8; i++)
	{
		float3 offset = reflect(Offsets[i] * SampleRadius, random);
		if (dot(offset, normal) < 0)
			offset = -offset;

		float3 samplePosition = position + offset;
		float4 projected = mul(float4(samplePosition, 1), Projection);
		float2 texCoord = float2(projected.x, -projected.y) / projected.w;
		texCoord = texCoord * 2 + 0.5;

		float sampleDepth = -samplePosition.z / FarClip;
		float sceneDepth = tex2D(depthSampler, texCoord).x;

		float delta = sceneDepth - sampleDepth;

		float occlusion = saturate(delta - MinThreshold) / (MaxThreshold - MinThreshold);
		occlusion = pow(occlusion, Power);

		totalOcclusion += occlusion;
		weights++;
	}

	totalOcclusion /= weights;

	out_Colour = 1 - totalOcclusion;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 AmbientOcclusionPS();
	}
}
*/
