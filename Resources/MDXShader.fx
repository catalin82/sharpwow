///////////////////////////////////////////////////
// Textures & Samplers
///////////////////////////////////////////////////

texture MeshTexture;

sampler MeshSampler = sampler_state
{
	texture = <MeshTexture>; 
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

///////////////////////////////////////////////////
// Globals
///////////////////////////////////////////////////

float4x4 matrixViewProj;
float3 CameraPosition;
static const float PI = 3.14159265f;
float4 fogColor = float4(0.6f, 0.6f, 0.6f, 1.0f); 
float fogStart = 500.0f;
float fogEnd = 530.0f;
float3 diffuseLight = float3(1, 1, 1);
float3 ambientLight = float3(0, 0, 0);
float3 SunDirection = float3(-1, -1, -1);
float4x4 BoneMatrices[61];
bool useAnimation = false;
bool isSelected = false;


///////////////////////////////////////////////////
// Helper functions
///////////////////////////////////////////////////

float4 ApplyFog(float4 colorIn, float depth, float angle)
{
	if(depth < fogStart)
		return colorIn;

	float4 skyCol = fogColor;
	if(depth > fogEnd)
		return float4(skyCol.rgb, colorIn.a);

	float ratio = (depth - fogStart) / (fogEnd - fogStart);
	float4 col = float4(lerp(colorIn.rgb, skyCol.rgb, ratio), colorIn.a);
	return col;
}

float CalcDepth(float3 vertexPos)
{
	float dx = CameraPosition.x - vertexPos.x;
	float dy = CameraPosition.y - vertexPos.y;
	float dz = CameraPosition.z - vertexPos.z;
	return sqrt(dx * dx + dy * dy + dz * dz);
}

float4 ApplySunLight(float4 base, float3 normal)
{
	float light = dot(normal, -normalize(SunDirection));
	if(light < 0)
		light = 0;
	if(light > 0.5f)
		light = 0.5f + (light - 0.5f) * 0.65f;

	light = saturate(light + 0.5f);
	float3 diffuse = diffuseLight * light;
	diffuse += ambientLight;
	base.rgb *= diffuse;
	return base;
}



///////////////////////////////////////////////////
// Shader structs (PixelInput and VertexInput)
///////////////////////////////////////////////////

struct PixelInput
{
	float4 Position : POSITION0;
	float2 TextureCoords : TEXCOORD0;
	float Depth : TEXCOORD1;
	float Angle : TEXCOORD2;
	float3 Normal : TEXCOORD3;
	float4 VariableInput : COLOR1;
};

struct VertexInput
{
	float4 Position : POSITION0;
	float4 Weights : BLENDWEIGHT;
	int4 Indices : BLENDINDICES;
	float3 Normal : NORMAL;
	float2 TextureCoords : TEXCOORD0;

	float4 VariableInput : COLOR1;
	float4 mat0 : TEXCOORD1;
	float4 mat1 : TEXCOORD2;
	float4 mat2 : TEXCOORD3;
	float4 mat3 : TEXCOORD4;
};



///////////////////////////////////////////////////
// Vertex-Shader
///////////////////////////////////////////////////

PixelInput MeshShader(VertexInput input)
{
	PixelInput retVal = (PixelInput)0;

	float4x4 modelMatrix = 
	{
		input.mat0,
		input.mat1,
		input.mat2,
		input.mat3
	};

	float4 posTmp = float4(input.Position.xyz, 1.0f);

	if(useAnimation)
	{
		float fW;
		float fBlendW[4] = (float[4])input.Weights;
		int iIndices[4] = (int[4])input.Indices;
		float4 posOrig = posTmp;
		float4 posTrans = posTmp;

		for(int i = 0; i < 4; ++i)
		{
			fW = fBlendW[i];
			if(fW == 0.0f)
				continue;

			posTrans.xyz += mul(posOrig, BoneMatrices[iIndices[i]]) * fW;
		}

		posTmp.xyz = posTrans.xyz - posTmp.xyz;
	}

	float4 instancePos = mul(posTmp, modelMatrix);
	float3 instanceNorm = mul(float4(input.Normal.xyz, 1), modelMatrix).xyz;

	retVal.Position = mul(instancePos, matrixViewProj);
	retVal.TextureCoords = input.TextureCoords;
	retVal.Depth = CalcDepth(instancePos.xyz);
	retVal.Normal = normalize(instanceNorm);
	retVal.VariableInput = input.VariableInput;

	//float3 diff = input.Position - CameraPosition;
	//float diff2D = sqrt(diff.x * diff.x + diff.z * diff.z);
	//retVal.Angle = atan(diff.y / diff2D);

	return retVal;
}



///////////////////////////////////////////////////
// Pixel-Shader
///////////////////////////////////////////////////

float4 PixelBlendShader(PixelInput input) : COLOR0
{
	float4 BaseColor = tex2D(MeshSampler, input.TextureCoords);
	BaseColor = ApplySunLight(BaseColor, input.Normal);

	BaseColor = ApplyFog(BaseColor, input.Depth, input.Angle);
	
	BaseColor.rgb *= input.VariableInput.rgb;

	return BaseColor;
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