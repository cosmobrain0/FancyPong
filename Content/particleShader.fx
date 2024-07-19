#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float2 CircleCentre;
float Radius;
float3 BaseColour;

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

float smoothstep(float a, float b, float t)
{
	t = saturate((t - a) / (b - a));
	return t * t * (3.0 - 2.0 * t);
}

float rotation(float time)
{
	float t = saturate((time-0.5)*1.3 + 0.5);
	return t*t*(3-2*t);
}

float repeat(float t)
{
	return t%1;
}

float loop(float t)
{
	return t%1 > 0.5 ? 2 - 2*(t%1) : 2 * (t%1);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 uv = (input.Position.xy-CircleCentre)/Radius;
    float colour = 1-length(uv);
    colour *= colour*colour;
	return float4(BaseColour, colour);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};


