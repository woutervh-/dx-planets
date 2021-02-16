cbuffer Constants : register(b0) {
	float4x4 worldViewProjectionMatrix;
};

struct PSInput {
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

PSInput VSMain(float4 position : POSITION, float4 color : COLOR) {
	PSInput result;

	result.position = mul(worldViewProjectionMatrix, position);
	result.color = color;

	return result;
}

float4 PSMain(PSInput input) : SV_TARGET {
	return input.color;
}
