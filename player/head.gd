extends Node3D

# Camera sensitivity
var sensitivity = 0.5
var camera : Camera3D

# FOV settings
var normal_fov = 75.0
var run_fov = 100.0
var fov_lerp_speed = 6.0

func _ready():
	camera = $Camera3D
	camera.fov = normal_fov

func _process(delta):
	var target_fov = normal_fov
	if Global.isRunning:
		target_fov = run_fov

	# Smooth FOV transition
	camera.fov = lerp(camera.fov, target_fov, delta * fov_lerp_speed)

func _input(event: InputEvent) -> void:
	# Toggle mouse capture
	if event.is_action_pressed("untoggle mouse"):
		if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		else:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

	# Mouse look rotation
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		# Yaw rotation (parent rotates horizontally)
		get_parent().rotate_y(deg_to_rad(-event.relative.x * sensitivity))
		
		# Pitch rotation (rotate camera up/down)
		rotate_x(deg_to_rad(-event.relative.y * sensitivity))
		
		# Clamp camera pitch
		rotation.x = clamp(rotation.x, deg_to_rad(-90), deg_to_rad(90))
