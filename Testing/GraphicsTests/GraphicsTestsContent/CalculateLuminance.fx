#include "FullScreenQuad.fxh"

float TimeDelta : TIMEDELTA;
float AdaptionRate : HDR_ADAPTIONRATE;

texture Texture;
sampler textureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

texture PreviousLuminance;
sampler luminanceSampler = sampler_state
{
	Texture = (PreviousLuminance);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
};

void PixelShaderFunction(in float3 in_Position : POSITION0,
						 out float4 out_Position : POSITION0)
{
	out_Position = float4(in_Position, 1);
}

float4 LuminancePS(in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
	float3 colour = tex2D(textureSampler, in_TexCoord).rgb;
	float luminance = max(dot(colour, float3(0.299f, 0.587f, 0.114f)), 0.0001f);
	luminance = log(luminance);
	return float4(luminance, luminance, luminance, 1);
}

float4 AdaptLuminancePS() : COLOR0 //in float2 in_TexCoord : TEXCOORD0) : COLOR0
{
    float lastLuminance = tex2D(luminanceSampler, float2(0.5, 0.5)).x;
    float currentLuminance = exp(tex2D(textureSampler, float2(0.5, 0.5)).x);
    
    // Adapt the luminance using Pattanaik's technique    
    float adaptedLum = lastLuminance + (currentLuminance - lastLuminance) * (1 - exp(-TimeDelta * AdaptionRate));
	
    return float4(adaptedLum, adaptedLum, adaptedLum, 1);
}

technique ExtractLuminance
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 FullScreenQuadVS();
		PixelShader = compile ps_3_0 LuminancePS();
	}
}

technique AdaptLuminance
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 PixelShaderFunction(); //FullScreenQuadVS();
		PixelShader = compile ps_3_0 AdaptLuminancePS();
	}
}