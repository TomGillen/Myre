#include "EncodeNormals.fxh"
#include "GammaCorrection.fxh"
#include "FullScreenQuad.fxh"

float3 Direction : LIGHTDIRECTION;
float3 Colour : LIGHTCOLOUR;
float3 CameraPosition : CAMERAPOSITION;
bool EnableShadows;

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

void PixelShaderFunction(in float2 in_TexCoord : TEXCOORD0,
						 in float3 in_FrustumRay : TEXCOORD1,
						 out float4 out_Colour : COLOR0)
{
	float4 sampledDepth = tex2D(depthSampler, in_TexCoord);
	float3 viewPosition = in_FrustumRay * sampledDepth.x;

	float4 sampledNormals = tex2D(normalSampler, in_TexCoord);
	float3 normal = DecodeNormal(sampledNormals.xy);
	float specularIntensity = sampledNormals.z;

	float4 sampledDiffuse = tex2D(diffuseSampler, in_TexCoord);
	float3 diffuse = GammaToLinear(sampledDiffuse.xyz);
	float specularPower = sampledDiffuse.w * 255;

	float3 L = -normalize(Direction);
	float3 V = normalize(CameraPosition - viewPosition);
	float3 R = normalize(reflect(-L, normal));

	float NdL = max(dot(normal, L), 0);
	float RdV = max(dot(R, V), 0);

	out_Colour = float4((NdL * diffuse * Colour) + ((specularIntensity * pow(RdV, specularPower)) * Colour), 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 FullScreenQuadFrustumCornerVS();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
