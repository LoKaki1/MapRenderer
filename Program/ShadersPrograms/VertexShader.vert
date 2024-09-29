
layout (location = 0) in float aAltitude;

out vec2 vUV;
out vec2 vBary;
flat out float vBrightness;

uniform mat4 mvp;

const float VERTICES_PER_RUN = " + Constants.VERTICES_PER_RUN + @".0;
const float VERTICES_PER_RUN_NOT_DEGENERATE = " + Constants.VERTICES_PER_RUN_NOT_DEGENERATE + @".0;

float rand3(vec3 c) { return fract(sin(dot(c.xyz, vec3(12.9898, 78.233, 133.719))) * 43758.5453);  }

void main()
{{    
    float runIndex = mod(gl_VertexID, VERTICES_PER_RUN);
    float clampedIndex = clamp(runIndex - 1.0, 0.0, VERTICES_PER_RUN_NOT_DEGENERATE); // First and last are degenerate


    // X increments every 2 vertices
    float xPos = floor(clampedIndex / 2.0);


    // Z increments every N vertices
    float zPos = floor(gl_VertexID / VERTICES_PER_RUN);


    // Move every 2nd vertex 1 unit on the z axis, to create a triangle
    zPos += mod(clampedIndex, 2.0);


    // Render to the screen
    vec3 pos = vec3(xPos, aAltitude, zPos);
    gl_Position = mvp * vec4(pos, 1.0);


    // Random triangle brightness
    vBrightness = mix(0.5, 1.0, fract(rand3(pos) * gl_VertexID));


    // Calculate barycentric stuff for the wireframe
    int baryIndex = int(mod(clampedIndex, 3));

    if (baryIndex == 0)
        vBary = vec2(0.0, 0.0);
    else if (baryIndex == 1)
        vBary = vec2(0.0, 1.0);
    else
        vBary = vec2(1.0, 0.0);
}}