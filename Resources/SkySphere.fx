float4x4 matrixViewProj;
float4x4 matWorld;
float fogEnd = 600.0f;

texture skyTexture;
sampler SkySampler = sampler_state
{
	texture = <skyTexture>; 
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

struct VertexInput
{
	float4 Position : POSITION0;
};

struct PixelInput
{
	float4 Position : POSITION0;
	float3 PosOld : TEXCOORD0;
};

PixelInput VertexShaderFunction(VertexInput input)
{
	PixelInput ret = (PixelInput)0;
	ret.Position = mul(input.Position, mul(matWorld, matrixViewProj));
	ret.PosOld = float3(input.Position.xyz);

	return ret;
}

float4 PixelShaderFunction(PixelInput input) : COLOR0
{
    float3 diff = input.PosOld;
    
    float coordTmp = diff.z / fogEnd;
    float coord = ((coordTmp + 1) / 2.0f);
    
    return tex1D(SkySampler, coord);
}

technique SkyTechnique
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}