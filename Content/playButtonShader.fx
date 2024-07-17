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
float2 Mouse;

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

float lerp(float a, float b, float t)
{
	return saturate((t-a)/(b-a));
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 mouseUV = (Mouse-CircleCentre)/Radius;
	float2 uv = (input.Position.xy-CircleCentre)/Radius;
	float2 playSignUV = rotate(smoothstep(0, 1, lerp(0.4, 0.6, repeat(Time/3000)))*3.1415926*2/3, uv);

	float backgroundColour = 0;
	bool colourInverted = false;
	if (length(mouseUV) <= 1)
	{
		colourInverted = true;
		backgroundColour = (1-length(mouseUV-uv))*0.08 + 0.92;
	}

	float normalisedTheta = repeat((atan2(uv.y, uv.x) + 3.1415926) / (2*3.1415926) + Time/12000);
	float distance = length(uv);
	float blackenThing = int(distance > 0.8) * int(distance < 0.9);
	float validTheta = int(floor(normalisedTheta*24)%2 == 0);

	// with radius 1...
	float horizontalOffset = smoothstep(0, 1, loop(Time/2000 - 0.25))*0.25 + 0.55;
	float thirdOffset = horizontalOffset/3;
	
	float sideLength = horizontalOffset/sqrt(3) * 2;
	float leftLine = verticalLine(-thirdOffset, playSignUV);
	float topLine = gradientLine(float2(-thirdOffset, sideLength/2), float2(thirdOffset*2, 0), playSignUV);
	float bottomLine = gradientLine(float2(-thirdOffset, -sideLength/2), float2(thirdOffset*2, 0), playSignUV);
	float playSign = leftLine * (1-topLine) * bottomLine;

	// TODO: IMMEDIATELY: fix the angle line thingy
	float colour = (1-playSign) * (1-blackenThing*validTheta);
	colour = 1-colour;
	colour = colour + backgroundColour;
	if (colour >= 1 && colourInverted) colour = 0;
 	return float4(colour, colour, colour, 1);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
