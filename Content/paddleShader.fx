#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// TODO: figure out how to use the texture coordinate

matrix WorldViewProjection;
float2 TopLeft;
float2 Size;
bool RightSide;

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(input.Position, WorldViewProjection);

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = (input.Position.xy-TopLeft)/Size + float2(0, 1);
    if (RightSide) uv.x = 1-uv.x;
    float x = -4*uv.y*uv.y + 4*uv.y;
	return float4(1, 1, 1, int(uv.x <= x && uv.x >= x-0.2) + int(uv.x <= x)*0.5);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};


