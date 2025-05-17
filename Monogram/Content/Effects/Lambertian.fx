float4x4 WVP;
float3x3 WorldIT;
float3x3 World;

float3 LightPosition : POSITION0;
float3 AmbientColor : COLOR0;
float3 DiffuseColor : COLOR1;
float1 AmbientIntensity;

struct VSIN {
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VSOUT {
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 WorldPos : POSITION1;
};

VSOUT VS(VSIN input) {
	VSOUT output;
    output.Position = mul(input.Position, WVP);
	output.Normal = normalize(mul(input.Normal, WorldIT));
	output.WorldPos = mul(input.Position.xyz, World);
	return output;
}
 
float4 PS(VSOUT input) : COLOR {
	float3 lightDir = normalize(LightPosition - input.WorldPos);

	float3 ambient = saturate((AmbientColor.rgb * AmbientIntensity).rgb);
	float3 diffuse = saturate(dot(input.Normal, normalize(lightDir)) * DiffuseColor.rgb);

	return float4(saturate(ambient + diffuse), 1.0);
}

technique Lambertian {
	pass Pass1 {
		VertexShader = compile vs_4_0 VS();
		PixelShader  = compile ps_4_0 PS();
	}
}
