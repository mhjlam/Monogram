float4x4 WVP;
float3x3 WorldIT;

struct VSIN
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VSOUT
{
	float4 Position : POSITION0;
	float3 WorldPosition : TEXCOORD0;
};


VSOUT VS(VSIN input)
{
	VSOUT output;
    output.Position = mul(input.Position, WVP);
	output.WorldPosition = mul(input.Position.xyz, WorldIT);
	return output;
}

float4 PS(VSOUT input) : COLOR
{
	float3 p = saturate(sign(sin(3.141592 * input.WorldPosition.xyz * 50.0)));
	return (fmod(p.x + p.y + p.z, 2.0)) ? 0.9 : 0.1;
}

technique Checkers
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
	}
}
