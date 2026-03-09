extends Node3D

var sensitivity = 0.5
var contSensitivity = 0.1
var contDeadzone = 0.15
var camera : Camera3D

var normal_fov = 75.0
var run_fov = 100.0
var fov_lerp_speed = 6.0

func _ready():
	camera = $Camera3D
	camera.fov = normal_fov
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func _process(delta):
	var target_fov = normal_fov
	
	if playerGlobals.isRunning:
		target_fov = run_fov
	
	camera.fov = lerp(camera.fov, target_fov, delta * fov_lerp_speed)
	
	# Controller Capture
	var joystickX = Input.get_joy_axis(0, JOY_AXIS_RIGHT_X)
	var joystickY = Input.get_joy_axis(0, JOY_AXIS_RIGHT_Y)
	# PREVENT STICK DRIFT
	if abs(joystickX) < .15: 
		joystickX = 0.0
	if abs(joystickY) < .15:
		joystickY = 0.0
	# Right Controller Movement
	get_parent().rotate_y(-joystickX * contSensitivity * delta * 60)
	var pitch = rotation.x - (joystickY * contSensitivity * delta * 60)
	rotation.x = clamp(pitch, deg_to_rad(-90), deg_to_rad(90))

func _input(event: InputEvent):

	if event.is_action_pressed("untoggle mouse"):
		if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		else:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		get_parent().rotate_y(deg_to_rad(-event.relative.x * sensitivity))
		var pitch = rotation.x - deg_to_rad(event.relative.y * sensitivity)
		pitch = clamp(pitch, deg_to_rad(-90), deg_to_rad(90))
		rotation.x = pitch
