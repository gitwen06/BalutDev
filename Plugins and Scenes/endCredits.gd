extends Control

@onready var returnToMenu: Button = %returnToMenu

func _ready() -> void:
	returnToMenu.pressed.connect(backToMenu)

func backToMenu():
	get_tree().paused = false
	var returnMenu = "res://Plugins and Scenes/main_menu.scn"
	get_tree().change_scene_to_file(returnMenu)
