shader_type spatial;
render_mode unshaded;

global uniform vec3 editor_highlight_position;
varying vec3 v_world_pos;

void vertex() {
	v_world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	// Called for every pixel the material is visible on.
	ALBEDO = COLOR.rgb;
	
	float highlightDistance = pow(length(editor_highlight_position - v_world_pos) / 5.0, 2);
	ALPHA = max(mix(COLOR.a, 0.0, highlightDistance), 0.1);
}