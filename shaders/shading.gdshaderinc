const vec3 up = vec3(0.0, 1.0, 0.0);

vec4 MatcapColor(sampler2D matcap, vec3 v_world_normal, vec3 v_eye_pos, vec3 v_world_pos, float margin, bool is_spatial) {
	// Called for every pixel the material is visible on.

	/*
		Fix the normal, and calculate the direction from the eye position.
		Then calculate the reflection based on both of these.
		All of this in world space, for convenience.
	*/
	vec3 normal = normalize(v_world_normal);
	vec3 view_dir = normalize(v_eye_pos - v_world_pos);
	vec3 reflection = reflect(view_dir, normal);

	/*
		Find the horizontal and vertical target direction.
		Differs if we're going spatial.
	*/
	float vert = dot(reflection, up);
	float hor;
	if (is_spatial) {
		hor = cross(reflection, up).z;
	} else {
		hor = cross(reflection, normal).y;
	}

	/*
		Calculate the UV based on the horizontal and vertical direction.
		Account for the margin.
	*/
	vec2 uv = vec2(hor, vert) * (1.0 - margin);
	uv = 0.5 + 0.5 * uv;

	/*
		Sample the texture and finish up.
	*/
	vec4 color = texture(matcap, uv);

	return color;
}