shader_type spatial;
render_mode unshaded;

#include "shading.gdshaderinc"

/*
	Uniforms;
	
	'Matcap' is the matcap texture.
	'Margin' reduces the size of the final UV, to avoid seams.
	'Is Spatial' allows horizontal wrapping.
*/
uniform sampler2D matcap: source_color, hint_default_white;
uniform float margin: hint_range(0.0, 0.314) = 0.314;
uniform bool is_spatial = false;
uniform vec3 tint: source_color = vec3(1,1,1);

/*
	Varyings;
	
	World-space normal, position, and eye position.
*/
varying vec3 v_world_normal;
varying vec3 v_world_pos;
varying vec3 v_eye_pos;

void vertex() {
	// Called for every vertex the material is visible on.
	v_world_normal = (MODEL_NORMAL_MATRIX * NORMAL);
	v_world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	v_eye_pos = (INV_VIEW_MATRIX * vec4(EYE_OFFSET, 1.0)).xyz;
}

void fragment() {
	vec4 color = MatcapColor(matcap, v_world_normal, v_eye_pos, v_world_pos, margin, is_spatial);
	
	vec3 worldNormal = (INV_VIEW_MATRIX * vec4(NORMAL.xyz, 0.0)).xyz;
	float lightDirDot = (dot(worldNormal, vec3(0, 1, 0)) + 1.0) / 2.0;
	ALBEDO = COLOR.rgb * color.rgb * tint * lightDirDot;
}

void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
	DIFFUSE_LIGHT = vec3(0,0,0);
}
