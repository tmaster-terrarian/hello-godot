shader_type sky;

void sky() {
	float yVal = pow((EYEDIR.y + 1.0) * 0.5, 0.3);
	vec3 color1 = vec3(0, 0, 1);
	vec3 color2 = vec3(0, 1, 1);
	COLOR = mix(color1, color2, yVal);
}
