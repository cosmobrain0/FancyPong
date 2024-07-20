#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float2 CircleCentre;
float Time;
float Radius;
float2 Velocity;
float SpeedBoostDuration;
float SpeedBoostTime;

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

float angleDistance(float start, float end)
{
    float diff = (end - start + 3.1415926) % (3.1415926*2) - 3.1415926;
    return diff < -3.1415926 ? diff + 3.1415926*2 : diff;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 uv = (input.Position.xy-CircleCentre)/Radius;
	float distance = length(uv);

	float3 white = float3(1, 1, 1);
	float3 gold = float3(242, 235, 12)/255;
	float3 black = float3(0, 0, 0);

	float speedBoostPercentage = SpeedBoostTime/SpeedBoostDuration;
	float k = 15;
	float t = clamp(k * (0.5 - abs(0.5-speedBoostPercentage)), 0, 1) / 2;
	float goldenRadius = 16 * t * t * (1-t) * (1-t);

	float normalisedTheta = atan2(uv.y, uv.x) / (2*3.1415926) + 0.5;
	normalisedTheta = (normalisedTheta+0.5) % 1;
	float progressPercentage = 1-speedBoostPercentage;
	float withinArcDistance = smoothstep(0.052, 0.05, abs(distance-0.8));
	float withinArcAngle = smoothstep(progressPercentage/2+0.01, progressPercentage/2, abs(progressPercentage/2 - normalisedTheta));
	float showWhiteProgressBar = withinArcDistance * withinArcAngle * smoothstep(0, 0.01, speedBoostPercentage);
	

	float colour = smoothstep(goldenRadius, goldenRadius+0.001, distance);
	
	return float4(lerp(lerp(gold, white, colour), black, showWhiteProgressBar), 1);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

