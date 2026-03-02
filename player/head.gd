extends Node3D

var sensitivity = 0.5
var camera : Camera3D

var normal_fov = 75.0
var run_fov = 100.0
var fov_lerp_speed = 6.0

func _ready():
	camera = $Camera3D
	camera.fov = normal_fov

func _process(delta):
	# kunin ang target fov
	var target_fov = normal_fov
	if Input.is_action_pressed("run"):
		target_fov = run_fov

	# transition
	camera.fov = lerp(camera.fov, target_fov, delta * fov_lerp_speed)

func _input(event: InputEvent) -> void:
	# toggle mouse capture
	if event.is_action_pressed("untoggle mouse"):
		if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		else:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

	# head rotation by mouse look
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		get_parent().rotate_y(deg_to_rad(-event.relative.x * sensitivity))
		rotate_x(deg_to_rad(-event.relative.y * sensitivity))
		rotation.x = clamp(rotation.x, deg_to_rad(-90), deg_to_rad(90))
