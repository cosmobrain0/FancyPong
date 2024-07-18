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
float Progress;
float2 Direction;

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

float smoothstep(float t, float a, float b)
{
    t = saturate((t-a)/(b-a));
    return t*t*(3-2*t);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = (input.Position.xy-TopLeft)/Size + float2(0, 1);
    float2 direction = float2(Direction.x, -Direction.y);
    uv = (uv-float2(0.5, 0.5)) * 2;
    float colour = 0;
    float t = 1 - (1-Progress)*(1-Progress)*(1-Progress);
    for (int i=0; i<=5; i++)
    {
        float p = (float)i/5. * 0.95;
        float radius = lerp(0, p, t);
        float thickness = lerp(0.05, 0, t);
        colour += smoothstep(abs(length(uv) - radius), thickness+0.03, thickness-0.03);
    }
    colour *= lerp(0.7, 1, dot(uv, direction)/length(uv)) * int(dot(uv, direction) > 0);
    return float4(colour, colour, colour, 0.5);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};



