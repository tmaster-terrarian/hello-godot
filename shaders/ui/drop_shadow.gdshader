shader_type canvas_item;

instance uniform vec4 shadow_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

void fragment() {
	vec4 col = texture(TEXTURE, UV);
	COLOR = mix(vec4(shadow_color.rgb, 0.0), shadow_color, col.a);
}