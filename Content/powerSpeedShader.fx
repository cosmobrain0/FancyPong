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

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = (input.Position.xy-TopLeft)/Size + float2(0, 1);
    float draw = max(abs(uv-0.5).x, abs(uv-0.5).y) >= 0.4;
    float radius = 0.13;
    float2 radiusVector = float2(1, 1)/length(float2(1, 1)) * (radius - 0.02 - 0.01);
    float2 circleCentre = float2(0.75, 0.25);
    draw += circle(circleCentre, radius, uv);
    draw += vectorLine(circleCentre, float2(0.25, 0.75), 0.02, uv);
    draw += vectorLine(circleCentre+radiusVector, float2(0.25, 0.75)+radiusVector.x, 0.02, uv);
    draw += vectorLine(circleCentre-radiusVector, float2(0.25, 0.75)-radiusVector.y, 0.02, uv);
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




