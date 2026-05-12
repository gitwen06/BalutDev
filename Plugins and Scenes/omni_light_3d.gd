extends OmniLight3D

@export var min_energy := 0.8
@export var max_energy := 1.2
@export var flicker_speed := 0.05

var timer := 0.0

func _process(delta):
	timer -= delta

	if timer <= 0:
		timer = flicker_speed
		light_energy = randf_range(min_energy, max_energy)
