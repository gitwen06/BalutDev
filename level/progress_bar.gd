extends ProgressBar

var player

func _ready() -> void:
	player = get_node("/root/playerGlobals")
	min_value = 1
	max_value = player.maxStamina
	value = player.stamina

func _process(_delta: float) -> void:
	if player:
		value = player.stamina
