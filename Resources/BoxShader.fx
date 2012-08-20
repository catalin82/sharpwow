///////////////////////////////////////////////////
// Globals
///////////////////////////////////////////////////

float4x4 matrixWorld;
float4x4 matrixViewProj;



///////////////////////////////////////////////////
// Shader structs (PixelInput and VertexInput)
///////////////////////////////////////////////////

struct PixelInput
{
	float4 Position : POSITION0;
	float2 TextureCoords : TEXCOORD0;
	float2 TextureCoords2 : TEXCOORD1;
};

struct VertexInput
{
	float4 Position : POSITION0;
	float2 TextureCoords : TEXCOORD0;
	float2 TextureCoords2 : TEXCOORD1;
};



///////////////////////////////////////////////////
// Vertex-Shader
///////////////////////////////////////////////////

PixelInput MeshShader(VertexInput input)
{
	PixelInput retVal = (PixelInput)0;

	float4 tmpPos = mul(input.Position, matrixWorld);
	retVal.Position = mul(tmpPos, matrixViewProj);
	retVal.TextureCoords = input.TextureCoords;
	retVal.TextureCoords2 = input.TextureCoords2;

	return retVal;
}



///////////////////////////////////////////////////
// Pixel-Shader
///////////////////////////////////////////////////

float4 PixelBlendShader(PixelInput input) : COLOR0
{
	float4 color = float4(0, 0, 0, 0);

	if(	(input.TextureCoords.x < 0.01 && input.TextureCoords.y < 0.01)  ||
		(input.TextureCoords.x > 0.99 && input.TextureCoords.y > 0.99)  ||
		(input.TextureCoords.x < 0.01 && input.TextureCoords.y > 0.99)  ||
		(input.TextureCoords.x > 0.99 && input.TextureCoords.y < 0.01)  ||
		(input.TextureCoords.x < 0.01 && input.TextureCoords.y < 0.01)  ||
		(input.TextureCoords.x > 0.99 && input.TextureCoords.y > 0.99)  ||
		(input.TextureCoords.x < 0.01 && input.TextureCoords.y > 0.99)  ||
		(input.TextureCoords.x > 0.99 && input.TextureCoords.y < 0.01)  ||
		(input.TextureCoords.x < 0.01 && input.TextureCoords2.x < 0.01) ||
		(input.TextureCoords.x > 0.99 && input.TextureCoords2.x < 0.01) ||
		(input.TextureCoords.x < 0.01 && input.TextureCoords2.x > 0.99) ||
		(input.TextureCoords.x > 0.99 && input.TextureCoords2.x > 0.99) ||
		(input.TextureCoords.y < 0.01 && input.TextureCoords2.x < 0.01) ||
		(input.TextureCoords.y > 0.99 && input.TextureCoords2.x < 0.01) ||
		(input.TextureCoords.y < 0.01 && input.TextureCoords2.x > 0.99) ||
		(input.TextureCoords.y > 0.99 && input.TextureCoords2.x > 0.99)   )
	{
		color.argb = 1;
	}

	return color;
}

///////////////////////////////////////////////////
// Technique
///////////////////////////////////////////////////


technique Blend1Layer
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 MeshShader();
		PixelShader = compile ps_3_0 PixelBlendShader();
	}
}