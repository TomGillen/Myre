float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

float Brightness;

texture EnvironmentMap;
sampler environmentSampler = sampler_state
{
	Texture = (EnvironmentMap);
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

void SkyboxVS(in float3 in_PositionOS : POSITION0,
				  out float4 out_PositionCS : POSITION0,
				  out float3 out_TexCoord : TEXCOORD0)
{
    // Rotate into view-space, centered on the camera
    float3 positionVS = mul(in_PositionOS.xyz * 2, (float3x3)View);    
    
    // Transform to clip-space
    out_PositionCS = mul(float4(positionVS, 1.0f), Projection);
	//out_PositionCS = float4(positionVS, 1.0f);

    // Make a texture coordinate
    out_TexCoord = in_PositionOS;
}

float4 SkyboxPS(in float3 in_TexCoord : TEXCOORD0) : COLOR0
{
    // Sample the skybox
    float3 colour = texCUBE(environmentSampler, normalize(in_TexCoord)).rgb;    
    return float4(colour * Brightness, 1.0f);
	//return float4(1, 1, 1, 1);
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 SkyboxVS();
		PixelShader = compile ps_3_0 SkyboxPS();
	}
}