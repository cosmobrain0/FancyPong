#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// TODO: figure out how to use the texture coordinate

matrix WorldViewProjection;
float2 CircleCentre;
float Time;
float Radius;
float2 Velocity;

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
	float2 uv = (input.Position.xy-CircleCentre)/Radius;
	float theta = acos(dot(uv, Velocity)/length(Velocity));
	float halfpi = 3.1415926/2;
	float radius = 1 + pow((max(theta, halfpi)-halfpi)/halfpi, 12)*length(Velocity)/0.3;
	input.Position.xy = uv/length(uv) * radius * Radius + CircleCentre;
	output.Position = mul(input.Position, WorldViewProjection);

	return output;
}

float smoothstep(float a, float b, float t)
{
	t = saturate((t - a) / (b - a));
	return t * t * (3.0 - 2.0 * t);
}

float gradientLine(float2 a, float2 b, float2 uv)
{
	float m = (b.y-a.y)/(b.x-a.x);
	float c = a.y - m*a.x;
	return int(uv.y > m*uv.x + c);
}


float verticalLine(float x, float2 uv)
{
	return int(uv.x>x);
}

float rotation(float time)
{
	float t = saturate((time-0.5)*1.3 + 0.5);
	return t*t*(3-2*t);
}

float2 rotate(float theta, float2 v)
{
	float c = cos(theta);
	float s = sin(theta);
	// TODO: figure out how to use matrices
	return float2(c*v.x - s*v.y, s*v.x + c*v.y);
}

float repeat(float t)
{
	return t%1;
}

float loop(float t)
{
	return t%1 > 0.5 ? 2 - 2*(t%1) : 2 * (t%1);
}

float lerp(float a, float b, float t)
{
	return saturate((t-a)/(b-a));
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return float4(1, 1, 1, 1);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

