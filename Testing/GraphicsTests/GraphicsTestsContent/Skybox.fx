#include "GammaCorrection.fxh"

//======================================================================
//
//	HDRSample
//
//		by MJP
//		09/20/08
//
//======================================================================
//
//	File:		Skybox.fx
//
//	Desc:		Renders a skybox using a cube map.
//
//======================================================================

float4x4  View : VIEW;
float4x4  Projection : PROJECTION;

float Brightness;

texture EnvironmentMap;
samplerCUBE SkyboxSampler = sampler_state
{
    Texture = <EnvironmentMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
    MaxAnisotropy = 1;
};

struct VS_INPUT
{
	float4 position : POSITION0;
    float3 texel0 : TEXCOORD0;
};

struct VS_OUTPUT
{
    float4 position : POSITION0;
    float3 texel0 : TEXCOORD0;
};



void SkyboxVS(	in float3 in_vPositionOS	: POSITION,
				in float3 in_vTexCoord		: TEXCOORD0,
				out float4 out_vPositionCS	: POSITION,
				out float3 out_vTexCoord	: TEXCOORD0 )
{
	float4 vPositionVS = float4(mul(in_vPositionOS * 3.0f, (float3x3)View), 1.0f);
	out_vPositionCS = mul(vPositionVS, Projection);
    out_vTexCoord = in_vTexCoord;
}

float4 SkyboxPS( in float3 in_vTexCoord	: TEXCOORD0,
				 uniform bool gammaCorrect)	: COLOR0
{	
	float3 vColor = texCUBE(SkyboxSampler, in_vTexCoord).rgb * Brightness;

	if (gammaCorrect)
		vColor = GammaToLinear(vColor);

	return float4(vColor, 1);
}

technique Skybox
{
    pass p0
    {    
        VertexShader = compile vs_3_0 SkyboxVS();
        PixelShader = compile ps_3_0 SkyboxPS(false);
    }
}

technique SkyboxGammaCorrect
{
	pass p0
    {    
        VertexShader = compile vs_3_0 SkyboxVS();
        PixelShader = compile ps_3_0 SkyboxPS(true);
    }
}
