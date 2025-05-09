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
uniform float matcap_intensity: hint_range(0.0, 1.0) = 1.0;
uniform bool is_spatial = false;
uniform vec3 tint: source_color = vec3(1,1,1);
instance uniform float brightness: hint_range(0.0, 1.0) = 1.0;

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
	vec4 worldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0));
	v_world_pos = worldPos.xyz;

	//vec4 posOriginal = POSITION;
	//vec2 screenResolution = vec2(320, 240);
	//vec4 vert = vec4(VERTEX, 1.0);
	//vec4 wvp = PROJECTION_MATRIX * (MODELVIEW_MATRIX * vert);
	//vec2 snap = vec2(screenResolution.xy / wvp.w);
	//POSITION = vec4(trunc(wvp.xy * snap) / snap, wvp.z, wvp.w);
}

void fragment() {
	vec4 color = MatcapColor(matcap, v_world_normal, v_eye_pos, v_world_pos, margin, is_spatial);
	color.rgb = mix(vec3(1.0), color.rgb, matcap_intensity);

	vec3 worldNormal = (INV_VIEW_MATRIX * vec4(NORMAL.xyz, 0.0)).xyz;
	float lightDirDot = ((dot(worldNormal, vec3(0, 1, 0)) + 1.0) / 2.0) + 0.2;

	vec3 hsv = rgb2hsv(tint);
	hsv.y = mix(hsv.y, 1.0, 1.0 - brightness);
	hsv.z = mix(hsv.z, hsv.z * 0.8, 1.0 - brightness);
	vec3 tintAdjusted = hsv2rgb(hsv);
	ALBEDO = COLOR.rgb * color.rgb * tintAdjusted * lightDirDot * 1.5;
}

//void light() {
	//// Called for every pixel for every light affecting the material.
	//// Uncomment to replace the default light processing function with this one.
	//DIFFUSE_LIGHT = vec3(1,1,1) * 3.0;
//}
