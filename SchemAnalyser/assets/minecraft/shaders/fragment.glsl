#version 330 core
#include "minecraft:core.glsl"

out vec4 FragColor;

uniform vec3 color;
uniform mat4 view;
uniform float time_line;

void main() {
    FragColor = vec4(color,1.0); 
}