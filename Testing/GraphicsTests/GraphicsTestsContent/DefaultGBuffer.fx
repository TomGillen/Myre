#include "EncodeNormals.fxh"

float4x4 WorldView : WORLDVIEW;
float4x4 Projection : PROJECTION;
float FarClip : FARCLIP;

texture DiffuseMap;
texture NormalMap;
texture SpecularMap;

sampler diffuseSampler = sampler_state
{
	Texture = (DiffuseMap);
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

sampler normalSampler = sampler_state
{
	Texture = (NormalMap);
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

sampler specularSampler = sampler_state
{
	Texture = (SpecularMap);
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : NORMAL;
	float3 Binormal : BINORMAL;
	float3 Tangent : TANGENT;
};

struct VertexShaderOutput
{
    float4 PositionCS : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float Depth : TEXCOORD1;
	float3x3 TangentToView : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 viewPosition = mul(input.Position, WorldView);
    
	output.PositionCS = mul(viewPosition, Projection);
	output.Depth = -viewPosition.z / FarClip;
	output.TexCoord = input.TexCoord;

	output.TangentToView[0] = mul(input.Tangent, WorldView);
	output.TangentToView[1] = mul(input.Binormal, WorldView);
	output.TangentToView[2] = mul(input.Normal, WorldView);

    return output;
}

void PixelShaderFunction(uniform bool ClipAlpha,
						 in VertexShaderOutput input,
						 out float4 out_depth : COLOR0,
						 out float4 out_normal : COLOR1,
						 out float4 out_diffuse : COLOR2)
{
    float4 diffuseSample = tex2D(diffuseSampler, input.TexCoord);

	if (ClipAlpha)
	{
		clip(diffuseSample.a < 0.5 ? -1 : 1);
	}

	float4 normalSample = tex2D(normalSampler, input.TexCoord);
	float4 specularSample = tex2D(specularSampler, input.TexCoord);

	float3 normal = normalSample.xyz * 2 - 1;
	normal = mul(normal, input.TangentToView);
	normal = normalize(normal);

	out_depth = float4(input.Depth, 0, 0, 1);
	out_normal = float4(EncodeNormal(normal), specularSample.r, 1);
	out_diffuse = float4(diffuseSample.rgb, specularSample.a);
}

technique Default
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction(false);
    }
}

technique ClipAlpha
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction(true);
    }
}
