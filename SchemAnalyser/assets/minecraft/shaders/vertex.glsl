#version 330 core
#include "minecraft:core.glsl"

layout(location = 0) in vec3 aPos;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 world_position;
uniform vec3 cam;
vec3 center = vec3(0,0.5,0);


void main() {
    
    gl_Position = projection * view * model * vec4(aPos, 1.0) + projection * view * vec4(world_position, 0.0);
}