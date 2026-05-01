extends StaticBody3D

var isOn: bool = false 
# IF FLASHLIGHT IS ON YOU FUCKERR
@onready var batteryPower = $batteryBar
@onready var playerFlashlight = GlobalVariables.playerFlashlight
@onready var outlineMesh = $flashlight/flashlight/MeshInstance3D
@onready var flashlight = $flashlight

var selected = false
var outlineWidth = 0.1

func _ready() -> void:
	batteryPower.value = GlobalVariables.FlashlightBattery 
	
	if playerFlashlight: 
		playerFlashlight.light_energy = 0
	
	isOn = false
	
	if batteryPower:
		batteryPower.visible = false 

	call_deferred("_connect_to_player")

func _connect_to_player():
	var player = get_tree().get_first_node_in_group("player")
	if player:
		var interact_node = player.find_child("interact") 
		if interact_node and interact_node.has_signal("focused_object_changed"):
			interact_node.focused_object_changed.connect(_set_selected)

func _process(_delta: float) -> void:
	if not outlineMesh:
		return 
	# KAYA PALA AYAW DAHIL C# CODE YUNG RAYCAST P[UTANGINAAAAA
	selected = (GlobalVariables.target == self)
	outlineMesh.visible = selected
	
	if selected: 
		flashlight.position.y = outlineWidth
	else: 
		flashlight.position.y = 0

func _set_selected(object):
	selected = (self == object)

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
		batteryPower.value -= 0.2 * delta
		GlobalVariables.FlashlightBattery = batteryPower.value
		# If battery capacity ran out, it'll shutopff the flashlight
		if batteryPower.value <= 0:
			shutOffFlashlight()
