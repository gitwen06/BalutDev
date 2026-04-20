extends ProgressBar

var g

func _ready() -> void:
	g = get_node("/root/GlobalVariables")

	min_value = 0
	max_value = g.maxStamina
	value = g.stamina

func _process(_delta: float) -> void:
	if g:
		value = g.stamina
