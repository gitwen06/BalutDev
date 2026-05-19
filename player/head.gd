extends Node3D

var sensitivity: float = 0.2
var contSensitivity: float = 0.1
var contDeadzone: float = 0.15
var camera: Camera3D

var normal_fov: float = 75.0
var run_fov: float = 85.0
var fov_lerp_speed: float = 6.0

var g

@onready var raycast = $Camera3D/playerRay
@onready var hand = $Camera3D/Hand

func _ready():
	camera = $Camera3D
	camera.fov = normal_fov
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

	g = get_node("/root/GlobalVariables")


func _process(delta):
	var target_fov = normal_fov

	# ============= GLOBAL RUN STATE =============
	if g.isRunning:
		target_fov = run_fov

	camera.fov = lerp(camera.fov, target_fov, delta * fov_lerp_speed)

	# ============= CONTROLLER INPUT =============
	var joystickX = Input.get_joy_axis(0, JOY_AXIS_RIGHT_X)
	var joystickY = Input.get_joy_axis(0, JOY_AXIS_RIGHT_Y)

	# DEADZONE
	if abs(joystickX) < contDeadzone:
		joystickX = 0.0
	if abs(joystickY) < contDeadzone:
		joystickY = 0.0

	get_parent().rotate_y(-joystickX * contSensitivity * delta * 60)

	var pitch = rotation.x - (joystickY * contSensitivity * delta * 60)
	rotation.x = clamp(pitch, deg_to_rad(-90), deg_to_rad(90))
	
	
	# ============= Pickable Item Test =============
	#var item = raycast.get_collider()
	#if raycast.is_colliding():
		#if item.is_in_group("pickable"):
			#if Input.is_action_pressed("interact"):
				#item.global_position = hand.global_position
				#item.global_rotation = hand.global_rotation
				#item.collision_layer = 2


func _input(event: InputEvent):

	if event.is_action_pressed("untoggle mouse"):
		Input.mouse_mode = (
			Input.MOUSE_MODE_VISIBLE
			if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED
			else Input.MOUSE_MODE_CAPTURED
		)

	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		get_parent().rotate_y(deg_to_rad(-event.relative.x * sensitivity))

		var pitch = rotation.x - deg_to_rad(event.relative.y * sensitivity)
		pitch = clamp(pitch, deg_to_rad(-90), deg_to_rad(90))
		rotation.x = pitch
