extends CanvasLayer

@export var sceneArgs: Array
@export var tipContents: Array[Dictionary] = [
	{"text": "Look around your surroundings, and try to save your breath."},
	{"text": "Press [R] to scream BALUT."},
	{"text": "Don't skip the dialogue, so you don't get lost..."},
	{"text": "This Game Has Loud Noises."},
	{"text": "Save Your Battery and Energy Drink."},
	{"text": "Be always on lookout for the monster.. You never know where it truly is."},
	# Joke Tips
	{"text": "Ignore the last keybinds in the menu."},
	{"text": "I lost so much sleep.."},
	{"text": "This game was originally way uglier than expected even if it is quite ugly though - lead dev"},
	{"text": "Hello po enjoy po hehe - lead dev"},
	{"text": "Have fun!! i guess its pretty scary i think. - lead dev"}
]
var parameters: Dictionary
var loaded := false

func  _ready() -> void:
	update_tip()
	
func update_tip() -> void:
	var tipSelected := tipContents[randi() % tipContents.size()]
	%TipContent.text = tipSelected.text
