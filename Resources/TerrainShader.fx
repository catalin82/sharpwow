texture blendTexture0;
texture blendTexture1;
texture blendTexture2;
texture blendTexture3;


texture alphaTexture;

sampler BaseSampler = sampler_state
{
	texture = <blendTexture0>; 
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

sampler BlendSampler1 = sampler_state
{
	texture = <blendTexture1>; 
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

sampler BlendSampler2 = sampler_state
{
	texture = <blendTexture2>; 
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

sampler BlendSampler3 = sampler_state
{
	texture = <blendTexture3>; 
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

sampler AlphaSampler = sampler_state
{
	texture = <alphaTexture>; 
	magfilter = ANISOTROPIC;
	minfilter = ANISOTROPIC;
	mipfilter = ANISOTROPIC;
	AddressU = wrap;
	AddressV = wrap;
};

float4x4 matrixViewProj;
float3 MousePosition;
float4 WhiteColor = float4(0.8f, 0.8f, 0.8f, 0.3f);
float4 TileColor = float4(0.7f, 0.0f, 0.0f, 1.0f);
float4 ChunkColor = float4(0.0f, 0.7f, 0.0f, 1.0f);
bool DrawMouse = false;
float3 CameraPosition;
static const float PI = 3.14159265f;
static const float TileSize = 533.3333333333f;
static const float ChunkSize = TileSize / 16.0f;
float4 fogColor = float4(0.6f, 0.6f, 0.6f, 1.0f); 
float fogStart = 500.0f;
float fogEnd = 530.0f;
float3 SunDirection = float3(-1, -1, -1);
float CircleRadius = 6;
float InnerRadius = 6 * 0.66f;
int brushType = 0;
float gameTime;
int TextureFlags0, TextureFlags1, TextureFlags2, TextureFlags3;
float3 diffuseLight = float3(1, 1, 1);
float3 ambientLight = float3(0, 0, 0);

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

float4 ApplyFog(float4 colorIn, float depth, float angle)
{
	if(depth < fogStart)
		return colorIn;

	float4 skyCol = fogColor;
	if(depth > fogEnd)
		return skyCol;

	float ratio = (depth - fogStart) / (fogEnd - fogStart);
	float4 col = float4(lerp(colorIn.rgb, skyCol.rgb, ratio), colorIn.a);
	return col;
}

float4 ApplyCircle(float4 colorIn, float3 distVec, float mouseDist, float maxRadius) {
	if((maxRadius - mouseDist) > (maxRadius * 0.05f))
		return colorIn;

	distVec = normalize(distVec);
	float angle = atan2(distVec.x, distVec.y);
	if(angle < 0)
		angle += PI * 2;

	angle = (angle * 180.0f) / PI;
	float resid = angle % 20;
	if(resid < 10)
	{
		colorIn.rgb *= 0.2f;
	}

	return colorIn;
}

float CalcDepth(float3 vertexPos)
{
	float dx = CameraPosition.x - vertexPos.x;
	float dy = CameraPosition.y - vertexPos.y;
	float dz = CameraPosition.z - vertexPos.z;
	return sqrt(dx * dx + dy * dy + dz * dz);
}

float4 ApplyTileLines(float4 color, float3 position)
{
	float modX = abs(position.x) % TileSize;
	float modZ = abs(position.y) % TileSize;
	float tolMin = TileSize - 0.3f;
	if(modX < 0.3f || modX > tolMin)
		return TileColor;
	if(modZ < 0.3f || modZ > tolMin)
		return TileColor;

	return color;
}

float4 ApplyChunkLines(float4 color, float3 position)
{
	float modX = abs(position.x) % ChunkSize;
	float modZ = abs(position.y) % ChunkSize;
	float tolMin = ChunkSize - 0.15f;
	if(modX < 0.15f || modX > tolMin)
		return ChunkColor;
	if(modZ < 0.15f || modZ > tolMin)
		return ChunkColor;

	return color;
}

float4 ApplyHeightLines(float4 color, float3 position)
{
	float modY = abs(position.z % 11.0f);
	if(modY < 0.05f || modY > 10.95f)
		return float4(0.0f, 0.0f, 0.0f, 1.0f);

	return color;
}

struct PixelInput
{
	float4 Position : POSITION0;
	float2 TextureCoords : TEXCOORD0;
	float2 AlphaCoords : TEXCOORD1;
	float3 PositionSpace : TEXCOORD2;
	float Depth : TEXCOORD3;
	//float Angle : TEXCOORD4;
	float3 Normal : TEXCOORD5;
	//float4 VertexColor : TEXCOORD6;
};

struct VertexInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	//float4 VertexColor : COLOR0;
	float2 TextureCoords : TEXCOORD0;
	float2 AlphaCoords : TEXCOORD1;
};



///////////////////////////////////////////////////
// Vertex-Shader
///////////////////////////////////////////////////

PixelInput TerrainBlendShader(VertexInput input)
{
	PixelInput retVal = (PixelInput)0;

	retVal.Position = mul(float4(input.Position.xyz, 1.0f), matrixViewProj);
	retVal.TextureCoords = input.TextureCoords;
	retVal.AlphaCoords = input.AlphaCoords;
	retVal.PositionSpace = input.Position.xyz;
	retVal.Depth = CalcDepth(input.Position.xyz);
	retVal.Normal = normalize(input.Normal);
	/*retVal.VertexColor = input.VertexColor;

	float3 diff = input.Position - CameraPosition;
	float diff2D = sqrt(diff.x * diff.x + diff.z * diff.z);
	//retVal.Angle = atan(diff.y / diff2D);*/

	return retVal;
}



///////////////////////////////////////////////////
// Pixel-Shader
///////////////////////////////////////////////////

float4 PixelBlendShader1Layer(PixelInput input) : COLOR0
{
	float2 coord1 = input.TextureCoords;
	if(TextureFlags0 == 1)
		coord1 += gameTime * float2(0.2, 0.2);
	float4 BaseColor = tex2D(BaseSampler, coord1);
	float4 AlphaValue = tex2D(AlphaSampler, input.AlphaCoords);
	BaseColor *= AlphaValue.a;
	//BaseColor *= 2 * input.VertexColor;
	
	if(DrawMouse && brushType != 2)
	{
		float3 mouseDiff = input.PositionSpace - MousePosition;
		float mouseDist = dot(mouseDiff, mouseDiff);
		float mouseRadius = CircleRadius * CircleRadius;

		if(brushType == 0) {
			if(mouseDist < mouseRadius) {
				BaseColor = (WhiteColor.a * WhiteColor) + (1.0f - WhiteColor.a) * BaseColor;
			}
		}
		else if(brushType == 1) {
			if(mouseDist < mouseRadius && (mouseDist > 0.6f * mouseRadius))
			{
				float alpha = (mouseDist - 0.6f * mouseRadius) / (0.6f * mouseRadius);
				BaseColor = (alpha * WhiteColor) + (1.0f - alpha) * BaseColor;
			}
		}
		else if(brushType == 3) {
			if(mouseDist < mouseRadius) {
				BaseColor = ApplyCircle(BaseColor, mouseDiff, mouseDist, mouseRadius);
				if(mouseDist < (InnerRadius * InnerRadius))
					BaseColor = ApplyCircle(BaseColor, mouseDiff, mouseDist, InnerRadius * InnerRadius);
			}
		}
	}

	//BaseColor = ApplyDiffuse(BaseColor);
	BaseColor = ApplySunLight(BaseColor, input.Normal);
	BaseColor = ApplyHeightLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyChunkLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyTileLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyFog(BaseColor, input.Depth, 0);

	return BaseColor;
}

float4 PixelBlendShader2Layer(PixelInput input) : COLOR0
{
	float2 coord1 = input.TextureCoords;
	float2 coord2 = input.TextureCoords;
	if(TextureFlags0 == 1)
		coord1 += gameTime * float2(0.2, 0.2);
	if(TextureFlags1 == 2)
		coord2 += gameTime * float2(0.2, 0.2);

	float4 BaseColor = tex2D(BaseSampler, coord1);
	float4 BlendColor = tex2D(BlendSampler1, coord2);
	float4 AlphaValues = tex2D(AlphaSampler, input.AlphaCoords);
	BaseColor *= AlphaValues.a;
	BaseColor = (BlendColor * AlphaValues.b) + (1 - AlphaValues.b) * BaseColor;
	//BaseColor *= 2 * input.VertexColor;

	if(DrawMouse && brushType != 2)
	{
		float3 mouseDiff = input.PositionSpace - MousePosition;
		float mouseDist = dot(mouseDiff, mouseDiff);
		float mouseRadius = CircleRadius * CircleRadius;

		if(brushType == 0) {
			if(mouseDist < mouseRadius) {
				BaseColor = (WhiteColor.a * WhiteColor) + (1.0f - WhiteColor.a) * BaseColor;
			}
		}
		else if(brushType == 1) {
			if(mouseDist < mouseRadius && (mouseDist > 0.6f * mouseRadius))
			{
				float alpha = (mouseDist - 0.6f * mouseRadius) / (0.6f * mouseRadius);
				BaseColor = (alpha * WhiteColor) + (1.0f - alpha) * BaseColor;
			}
		}
		else if(brushType == 3) {
			if(mouseDist < mouseRadius) {
				BaseColor = ApplyCircle(BaseColor, mouseDiff, mouseDist, mouseRadius);
				if(mouseDist < (InnerRadius * InnerRadius))
					BaseColor = ApplyCircle(BaseColor, mouseDiff, mouseDist, InnerRadius * InnerRadius);
			}
		}
	}

	//BaseColor = ApplyDiffuse(BaseColor);
	BaseColor = ApplySunLight(BaseColor, input.Normal);
	BaseColor = ApplyHeightLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyChunkLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyTileLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyFog(BaseColor, input.Depth, 0);

	return BaseColor;
}

float4 PixelBlendShader3Layer(PixelInput input) : COLOR0
{
	float2 coord1 = input.TextureCoords;
	float2 coord2 = input.TextureCoords;
	float2 coord3 = input.TextureCoords;
	if(TextureFlags0 == 1)
		coord1 += gameTime * float2(0.2, 0.2);
	if(TextureFlags1 == 1)
		coord2 += gameTime * float2(0.2, 0.2);
	if(TextureFlags2 == 1)
		coord3 += gameTime * float2(0.2, 0.2);

	float4 BaseColor = tex2D(BaseSampler, coord1);
	float4 BlendColor1 = tex2D(BlendSampler1, coord2);
	float4 BlendColor2 = tex2D(BlendSampler2, coord3);

	float4 AlphaValues = tex2D(AlphaSampler, input.AlphaCoords);
	BaseColor *= AlphaValues.a;

	float4 blend1 = (AlphaValues.b * BlendColor1) + ((1 - AlphaValues.b) * BaseColor);
	BaseColor = (BlendColor2 * AlphaValues.g) + ((1 - AlphaValues.g) * blend1);
	//BaseColor *= 2 * input.VertexColor;

	if(DrawMouse && brushType != 2)
	{
		float3 mouseDiff = input.PositionSpace - MousePosition;
		float mouseDist = dot(mouseDiff, mouseDiff);
		float mouseRadius = CircleRadius * CircleRadius;

		if(brushType == 0) {
			if(mouseDist < mouseRadius) {
				BaseColor = (WhiteColor.a * WhiteColor) + (1.0f - WhiteColor.a) * BaseColor;
			}
		}
		else if(brushType == 1) {
			if(mouseDist < mouseRadius && (mouseDist > 0.6f * mouseRadius))
			{
				float alpha = (mouseDist - 0.6f * mouseRadius) / (0.6f * mouseRadius);
				BaseColor = (alpha * WhiteColor) + (1.0f - alpha) * BaseColor;
			}
		}
		else if(brushType == 3) {
			if(mouseDist < mouseRadius) {
				BaseColor = ApplyCircle(BaseColor, mouseDiff, mouseDist, mouseRadius);
				if(mouseDist < (InnerRadius * InnerRadius))
					BaseColor = ApplyCircle(BaseColor, mouseDiff, mouseDist, InnerRadius * InnerRadius);
			}
		}
	}

	//BaseColor = ApplyDiffuse(BaseColor);
	BaseColor = ApplySunLight(BaseColor, input.Normal);
	BaseColor = ApplyHeightLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyChunkLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyTileLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyFog(BaseColor, input.Depth, 0);

	return BaseColor;
}

float4 PixelBlendShader4Layer(PixelInput input) : COLOR0
{
	float2 coord1 = input.TextureCoords;
	float2 coord2 = input.TextureCoords;
	float2 coord3 = input.TextureCoords;
	float2 coord4 = input.TextureCoords;
	if(TextureFlags0 == 1)
		coord1 += gameTime * float2(0.2, 0.2);
	if(TextureFlags1 == 1)
		coord2 += gameTime * float2(0.2, 0.2);
	if(TextureFlags2 == 1)
		coord3 += gameTime * float2(0.2, 0.2);
	if(TextureFlags3 == 1)
		coord4 += gameTime * float2(0.2, 0.2);

	float4 BaseColor = tex2D(BaseSampler, coord1);
	float4 BlendColor1 = tex2D(BlendSampler1, coord2);
	float4 BlendColor2 = tex2D(BlendSampler2, coord3);
	float4 BlendColor3 = tex2D(BlendSampler3, coord4);

	float4 AlphaValues = tex2D(AlphaSampler, input.AlphaCoords);
	BaseColor *= AlphaValues.a;
	float4 blend1 = (AlphaValues.b * BlendColor1) + ((1 - AlphaValues.b) * BaseColor);
	float4 blend2 = (BlendColor2 * AlphaValues.g) + ((1 - AlphaValues.g) * blend1);
	BaseColor = (BlendColor3 * AlphaValues.r) + ((1 - AlphaValues.r) * blend2);
	//BaseColor *= 2 * input.VertexColor;

	if(DrawMouse && brushType != 2)
	{
		float3 mouseDiff = input.PositionSpace - MousePosition;
		float mouseDist = dot(mouseDiff, mouseDiff);
		float mouseRadius = CircleRadius * CircleRadius;

		if(brushType == 0) {
			if(mouseDist < mouseRadius) {
				BaseColor = (WhiteColor.a * WhiteColor) + (1.0f - WhiteColor.a) * BaseColor;
			}
		}
		else if(brushType == 1) {
			if(mouseDist < mouseRadius && (mouseDist > 0.6f * mouseRadius))
			{
				float alpha = (mouseDist - 0.6f * mouseRadius) / (0.6f * mouseRadius);
				BaseColor = (alpha * WhiteColor) + (1.0f - alpha) * BaseColor;
			}
		}
		else if(brushType == 3) {
			if(mouseDist < mouseRadius) {
				BaseColor = ApplyCircle(BaseColor, mouseDiff, mouseDist, mouseRadius);
				if(mouseDist < (InnerRadius * InnerRadius))
					BaseColor = ApplyCircle(BaseColor, mouseDiff, mouseDist, InnerRadius * InnerRadius);
			}
		}
	}

	//BaseColor = ApplyDiffuse(BaseColor);
	BaseColor = ApplySunLight(BaseColor, input.Normal);
	BaseColor = ApplyHeightLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyChunkLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyTileLines(BaseColor, input.PositionSpace);
	BaseColor = ApplyFog(BaseColor, input.Depth, 0);

	return BaseColor;
}



///////////////////////////////////////////////////
// Technique
///////////////////////////////////////////////////


technique Blend1Layer
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 TerrainBlendShader();
		PixelShader = compile ps_3_0 PixelBlendShader1Layer();
	}
}

technique Blend2Layer
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 TerrainBlendShader();
		PixelShader = compile ps_3_0 PixelBlendShader2Layer();
	}
}

technique Blend3Layer
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 TerrainBlendShader();
		PixelShader = compile ps_3_0 PixelBlendShader3Layer();
	}
}

technique Blend4Layer
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 TerrainBlendShader();
		PixelShader = compile ps_3_0 PixelBlendShader4Layer();
	}
}