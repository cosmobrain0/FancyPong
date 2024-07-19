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
float LineGap;
float LineHeight;
float Time;

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
    uv.y += Time/40000;
    float size = (LineGap + LineHeight)/Size.y;
    float x = uv.y % size;
	return float4(1, 1, 1, x <= LineGap/Size.y);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};



