#version 330 core
out vec4 FragColor;

uniform vec3 color;
uniform mat4 view;
uniform double time_line;

void main() { 
    FragColor = vec4(color,1.0); 
}