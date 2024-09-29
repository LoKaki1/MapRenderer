
out vec4 gColor;

in vec2 vUV;
in vec2 vBary;
flat in float vBrightness;

uniform bool showWireframe;

float barycentric(vec2 vBC, float width)
{
    vec3 bary = vec3(vBC.x, vBC.y, 1.0 - vBC.x - vBC.y);
    vec3 d = fwidth(bary);
    vec3 a3 = smoothstep(d * (width - 0.5), d * (width + 0.5), bary);
    return min(min(a3.x, a3.y), a3.z);
}


void main()
{
    gColor = vec4(vBrightness);

    if (showWireframe)
    {
        gColor.rgb *= 0.25;
        gColor.rgb += vec3(1.0 - barycentric(vBary, 1.0));
    }
}