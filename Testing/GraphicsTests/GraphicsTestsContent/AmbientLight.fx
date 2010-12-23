#include "EncodeNormals.fxh"
#include "GammaCorrection.fxh"
#include "FullScreenQuad.fxh"

float3 Up;
float3 SkyColour;
float3 GroundColour;

texture Depth : GBUFFER_DEPTH;
sampler depthSampler = sampler_state
{
	Texture = (Depth);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
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

texture Diffuse : GBUFFER_DIFFUSE;
sampler diffuseSampler = sampler_state
{
	Texture = (Diffuse);
	MinFilter = Point;
	MipFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture SSAO;
sampler ssaoSampler = sampler_state
{
	Texture = SSAO;
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
};

float3 ReadSsao(float2 texCoord)
{
	const float2 vec[3] = {
		float2(1,1),
		float2(1,0),
		float2(0,1),
	};
	  
	float3 ao = tex2D(ssaoSampler, texCoord).x;
	for (int k=0;k<3;k++){
		float2 tcoord = texCoord + float2(vec[k].x / Resolution.x, vec[k].y / Resolution.y);
		ao += tex2D(ssaoSampler, tcoord).rgb;
	}

	return ao / 4;
}

void PixelShaderFunction(in float2 in_TexCoord : TEXCOORD0,
						 in float3 in_FrustumRay : TEXCOORD1,
						 out float4 out_Colour : COLOR0,
						 uniform bool enableSsao)
{
	float4 sampledNormals = tex2D(normalSampler, in_TexCoord);
	float3 normal = DecodeNormal(sampledNormals.xy);

	float alpha = dot(Up, normal) / 2 + 0.5;
	float3 colour = lerp(GroundColour, SkyColour, alpha);

	if (enableSsao)
		colour *= ReadSsao(in_TexCoord);

	float3 diffuse = tex2D(diffuseSampler, in_TexCoord).rgb;
	colour *= GammaToLinear(diffuse);

	out_Colour = float4(colour, 1);
}

technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 FullScreenQuadFrustumCornerVS();
        PixelShader = compile ps_3_0 PixelShaderFunction(false);
    }
}

technique AmbientSSAO
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 FullScreenQuadFrustumCornerVS();
        PixelShader = compile ps_3_0 PixelShaderFunction(true);
    }
}
