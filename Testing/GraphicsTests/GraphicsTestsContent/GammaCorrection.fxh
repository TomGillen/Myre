float3 LinearToGamma(float3 colour)
{
    return pow(colour, 1/2.2f);
}

float3 GammaToLinear(float3 colour)
{
    return pow(colour, 2.2f);
}