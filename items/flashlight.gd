extends RigidBody3D
var isOn: bool = false #if flashlight is turned
@onready var lightSource = $SpotLight3D
@onready var batteryPower = $batteryBar

func _ready() -> void:
	#Added FlashlightBattery in the global variables
	batteryPower.value = GlobalVariables.FlashlightBattery #so battery doesnt reset
	if lightSource: 
		#so the fl's energy is 0
		lightSource.light_energy = 0
	isOn = false
	if batteryPower:
		batteryPower.visible = false 
		# so the battery indicator doesn't show when the flash light hans't been picked up

func _input(event: InputEvent) -> void:
	# Check if flashlight is in your hand
	if get_parent() != null and get_parent().name == "Hand":
		if event.is_action_pressed("toggleFlashlight") and not event.is_echo():
			# If the flashlight battery value is greater than 0
			if batteryPower.value > 0 or isOn:
				# the echoc hecks if its being beign held down instead of pressed once
				toggleFlashlight()
		if batteryPower:
			#if flashlight is held battery icon pops up
			batteryPower.visible = true
			
			#test code for toggleFlashlight() !!dont remove
			#if Input.is_action_pressed("toggleFlashlight"):
				#$SpotLight3D.light_energy = 16
			#else:
				#$SpotLight3D.light_energy = 0
	else:
		if batteryPower:
			batteryPower.visible = false

func toggleFlashlight() -> void:
	if not lightSource:
		return
	isOn = !isOn
	#turn on and turn off mecha
	if isOn: lightSource.light_energy = 16.0
	else: lightSource.light_energy = 0.0

func shutOffFlashlight() -> void:
	#WHen the battery runs out, this is the function for it turn off automaticaLLY
	isOn = false #so it turns off the fl
	lightSource.light_energy = 0

func _physics_process(delta: float) -> void:
	# IF the flsahlight is turned on, the battery's capacity will drain
	if isOn:
		# Indicator how fast the flashlight drains its battery
		batteryPower.value -= 1.0 * delta
		GlobalVariables.FlashlightBattery = batteryPower.value
		# print(batteryPower.value)
		# If battery capacity ran out, it'll shutopff the flashlight
		if batteryPower.value <= 0:
			shutOffFlashlight()
			
	#if $SpotLight3D.light_energy == 16:
		#$batteryBar.value -= 1
