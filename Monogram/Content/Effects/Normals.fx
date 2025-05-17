float4x4 WVP;
float3x3 WorldIT;

struct VSIN {
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VSOUT {
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

VSOUT VS(VSIN input) {
	VSOUT output;

	float3 normal = normalize(mul(input.Normal, WorldIT));
    output.Position = mul(input.Position, WVP);
	output.Color = float4(0.5 + (0.5 * normal), 1.0);

	return output;
}
 
float4 PS(VSOUT input) : COLOR {
	return float4(input.Color.rgb, 1.0);
}

technique Normals {
	pass Pass1 {
		VertexShader = compile vs_4_0 VS();
		PixelShader  = compile ps_4_0 PS();
	}
}
