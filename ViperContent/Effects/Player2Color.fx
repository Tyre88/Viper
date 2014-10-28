//Effect that changes one color to another color.

//VertexShader
struct VertexToPixel
{
    float4 Position     : POSITION;
    float4 Color        : COLOR0;
};

float4x4 xViewProjection;

sampler TextureSampler : register(s0);
float3 key_color = float3(1,1,1);
float3 new_color;

float4 ChangeColor(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	  float4 newColor = tex2D(TextureSampler, texCoord);

	  if (distance(key_color, newColor.rgb)<1.0f)
	  {
		 newColor.rgb = new_color;
	  } 

	  return newColor;
}

float Percentage;

float4 PixelShaderFunction(float2 Tex: TEXCOORD) : COLOR0
{
	float4 Color = tex2D(TextureSampler, Tex);
	float r = Color.r;
	float g = Color.g;
	float b = Color.b;
	Color.rgb = dot(Color.rgb, float3(0.7 * Percentage, 0.59 * Percentage, 0.11 * Percentage));
	r = r - (r - Color.rgb) * Percentage;
	g = g - (g - Color.rgb) * Percentage;
	b = b - (b - Color.rgb) * Percentage;
	Color.r = r;
	Color.g = g;
	Color.b = b;

	return Color;
}

VertexToPixel SimplestVertexShader(float4 inPos : POSITION)
{
    VertexToPixel Output = (VertexToPixel)0;

    Output.Position = mul(inPos, xViewProjection);
    Output.Color = 1.0f;

	//Output.Color.r = 1.0f;
	//Output.Color.g = 0.0f;
	//Output.Color.b = 1.0f;
	//Output.Color.a = 1.0f;
    
    return Output;
}



technique Player
{
	pass Normal
	{
		PixelShader = compile ps_2_0 ChangeColor();
	}
}

technique PlayerChili
{
	pass Chili
	{
		//VertexShader = compile vs_2_0 SimplestVertexShader();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}