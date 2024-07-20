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

float smoothstep(float t, float a, float b)
{
    t = saturate((t-a)/(b-a));
    return t*t*(3-2*t);
}

float circle(float2 centre, float radius, float2 uv)
{
    float lengthSquared = dot(centre-uv, centre-uv);
    return smoothstep(lengthSquared, radius*radius+0.0001, radius*radius);
}

float vectorLine(float2 a, float2 b, float thickness, float2 uv)
{
    float2 direction = b-a;
    float2 offset = uv-a;
    float lambda = dot(offset, direction)/dot(direction, direction);
    float2 closestPoint = a + clamp(lambda, 0, 1)*direction;
    float d = length(uv-closestPoint);
    return smoothstep(d, thickness+0.01, thickness);
}

float2 rotate(float theta, float2 v)
{
	float c = cos(theta);
	float s = sin(theta);
	matrix<float, 2, 2> rotationMatrix = matrix<float, 2, 2>(c, -s, s, c);
	// return float2(c*v.x - s*v.y, s*v.x + c*v.y);
	return mul(rotationMatrix, v);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = (input.Position.xy-TopLeft)/Size + float2(0, 1);
    float draw = max(abs(uv-0.5).x, abs(uv-0.5).y) >= 0.4;

	uv = 2 * (uv-0.5);
	uv = rotate(3.1415926/6, uv);
	for (int i=0; i<6; i++)
	{
		uv = rotate(3.1415926/3, uv);
		draw += vectorLine(float2(0, 0), float2(0.6, 0), 0.05, uv);
		draw += vectorLine(float2(0.4, 0), float2(0.5, 0.2), 0.05, uv);
		draw += vectorLine(float2(0.4, 0), float2(0.5, -0.2), 0.05, uv);
	}
	return float4(draw, draw, draw, 1);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};





