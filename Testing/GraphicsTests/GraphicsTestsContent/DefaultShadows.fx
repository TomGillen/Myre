float4x4 WorldView : WORLDVIEW;
float4x4 Projection : PROJECTION;
float FarClip : FARCLIP;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 PositionCS : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float Depth : TEXCOORD1;
	float3x3 TangentToView : TEXCOORD2;
};

void VertexShaderFunction(in float4 in_Position : POSITION0,
						  out float4 out_PositionCS : POSITION0,
						  out float2 out_Depth : TEXCOORD0)
{
    float4 viewPosition = mul(in_Position, WorldView);
	float4 clipPosition = mul(viewPosition, Projection);
    
	out_PositionCS = clipPosition;

	//out_Depth = length(viewPosition);
	out_Depth = clipPosition.zw;
	//out_Depth = -viewPosition.z / FarClip;
	//out_Depth = float2(0.5, 1);
}

void PixelShaderFunction(in float2 in_Depth : TEXCOORD0,
						 out float4 out_Colour : COLOR0)
{
	out_Colour = float4(in_Depth.x / in_Depth.y, 0, 0, 1);
	//out_Colour = float4(in_Depth, 0, 0, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
