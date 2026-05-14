extends CanvasLayer

@export var sceneArgs: Array
@export var tipContents: Array[Dictionary] = [
	{"text": "Look around your surroundings, and try to save your breath."},
	{"text": "Press [R] to scream BALUT."},
	{"text": "Don't skip the dialogue, so you don't get lost..."},
	{"text": "This Game Has Loud Noises."},
	{"text": "Save Your Battery and Energy Drink."},
	# Joke Tips
	{"text": "Psst, Play Shin Megami Tensei V Vengeance."},
	{"text": "Ignore the last keybinds in the menu."},
	{"text": "Do you even read these??"},
	{"text": "Psst, Bring Aling Neneng a Flashlight"}
]
var parameters: Dictionary
var loaded := false

func  _ready() -> void:
	update_tip()
	
func update_tip() -> void:
	var tipSelected := tipContents[randi() % tipContents.size()]
	%TipContent.text = tipSelected.text
