extends RigidBody3D
var isOn: bool = false #if flashlight is turned
@onready var batteryPower = $batteryBar
@onready var playerFlashlight = GlobalVariables.playerFlashlight
func _ready() -> void:
	#Added FlashlightBattery in the global variables
	batteryPower.value = GlobalVariables.FlashlightBattery #so battery doesnt reset
	if playerFlashlight: 
		#so the fl's energy is 0
		playerFlashlight.light_energy = 0
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
				toggleFlashlight()
func toggleFlashlight() -> void:
	if not playerFlashlight:
		return
	isOn = !isOn
	#turn on and turn off mecha
	if isOn: playerFlashlight.light_energy = 1.0
	else: playerFlashlight.light_energy = 0.0
func shutOffFlashlight() -> void:
	#WHen the battery runs out, this is the function for it turn off automaticaLLY
	isOn = false #so it turns off the fl
	playerFlashlight.light_energy = 0
func _physics_process(delta: float) -> void:
	# Sync battery from GlobalVariables
	batteryPower.value = GlobalVariables.FlashlightBattery
	
	# Check if flashlight is in your hand
	if get_parent() != null and get_parent().name == "Hand":
		if batteryPower:
			batteryPower.visible = true
	else:
		if batteryPower:
			batteryPower.visible = false
	
	# IF the flsahlight is turned on, the battery's capacity will drain
	if isOn:
		# Indicator how fast the flashlight drains its battery
		batteryPower.value -= 1 * delta
		GlobalVariables.FlashlightBattery = batteryPower.value
		# If battery capacity ran out, it'll shutopff the flashlight
		if batteryPower.value <= 0:
			shutOffFlashlight()
