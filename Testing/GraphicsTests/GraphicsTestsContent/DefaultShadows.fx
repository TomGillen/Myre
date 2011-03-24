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

void ViewLengthVS(in float4 in_Position : POSITION0,
						  out float4 out_PositionCS : POSITION0,
						  out float out_Depth : TEXCOORD0)
{
    float4 viewPosition = mul(in_Position, WorldView);
	float4 clipPosition = mul(viewPosition, Projection);
    
	out_PositionCS = clipPosition;
	out_Depth = length(viewPosition) / FarClip;
}

void ViewZVS(in float4 in_Position : POSITION0,
						  out float4 out_PositionCS : POSITION0,
						  out float out_Depth : TEXCOORD0)
{
    float4 viewPosition = mul(in_Position, WorldView);
	float4 clipPosition = mul(viewPosition, Projection);
    
	out_PositionCS = clipPosition;
	out_Depth = -viewPosition.z / FarClip;
}

void PS(in float in_Depth : TEXCOORD0,
		out float4 out_Colour : COLOR0)
{
	out_Colour = float4(in_Depth, 0, 0, 1);
}

technique ViewLength
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 ViewLengthVS();
        PixelShader = compile ps_3_0 PS();
    }
}

technique ViewZ
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 ViewZVS();
        PixelShader = compile ps_3_0 PS();
    }
}