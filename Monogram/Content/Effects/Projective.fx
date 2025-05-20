float4x4 WVP;
float3x3 WorldIT;

// these matrices are used to render the texture from the projector's POV
float4x4 ProjectorViewProjection;

texture2D ProjectedTexture;
sampler2D ProjectorSampler = sampler_state { texture = <ProjectedTexture>; };

// Projector location
float3 ProjectorPosition : POSITION;

// Material properties
float3 DiffuseColor	: COLOR0;
float3 AmbientColor	: COLOR1;

float Time;
float AmbientIntensity;


struct VSIN {
	float4 Position : POSITION;
	float3 Normal : NORMAL;
};

struct VSOUT {
	float4 Position : POSITION;
	float3 Normal : TEXCOORD0;
	float4 PPos : TEXCOORD1;
};

VSOUT VS(VSIN input) {
	VSOUT output;

	// do the usual matrix multiplications for vertex position and normal
    output.Position = mul(input.Position, WVP);
	output.Normal = normalize(mul(input.Normal, WorldIT));
	
	// do a second multiplication for the position the way the projector sees it
	output.PPos	= mul(input.Position, ProjectorViewProjection);

	return output;
}

float3 hsv2rgb(float h, float s, float v)
{
    float3 rgb = float3(0.0, 0.0, 0.0);

    float c = v * s;
    float h_ = h * 6.0;
    float x = c * (1.0 - abs(fmod(h_, 2.0) - 1.0));
		 if (0.0 <= h_ && h_ < 1.0) rgb = float3(c, x, 0.0);
    else if (1.0 <= h_ && h_ < 2.0) rgb = float3(x, c, 0.0);
    else if (2.0 <= h_ && h_ < 3.0) rgb = float3(0.0, c, x);
    else if (3.0 <= h_ && h_ < 4.0) rgb = float3(0.0, x, c);
    else if (4.0 <= h_ && h_ < 5.0) rgb = float3(x, 0.0, c);
    else if (5.0 <= h_ && h_ < 6.0) rgb = float3(c, 0.0, x);

    float m = v - c;
    return rgb + m;
}

float4 PS(VSOUT input) : COLOR {
	float4 output;

	// the +1.0 will become +0.5 after the multiplication with 0.5
	float u = 0.5f * ((input.PPos.x / input.PPos.w) + 1.0);
	float v = 0.5f * ((-input.PPos.y / input.PPos.w) + 1.0);
	
	float diffuseIntensity = saturate(dot(input.Normal, normalize(ProjectorPosition)));
	float3 ambient = AmbientColor * AmbientIntensity;
    float3 diffuse = DiffuseColor * diffuseIntensity;
	
	// manually prevent texture from wrapping
	float3 texel = float3(0.0f, 0.0f, 0.0f);

	// only use texel if uv coordinates are in range of the projection
    if (u >= 0.0f && u <= 1.0f && v >= 0.0f && v <= 1.0f) {
        float3 sample = tex2D(ProjectorSampler, float2(u, v)).rgb;
		
        // Cycle hue from 0 to 1 over time for a full RGB spectrum
        float hue = fmod(Time * 0.15, 1.0); // 0.15 controls speed
        float3 rainbow = hsv2rgb(hue, 1.0, 1.0);
		
        texel = sample * rainbow * diffuseIntensity;
    }

	output.rgb = saturate(ambient + diffuse + texel);
	output.a = 1.0f;

	return output;
}

technique ProjectiveTextureMapping {
	pass Pass1 {
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
	}
}
