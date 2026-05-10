extends ScrollContainer

@onready var keybind_1: HBoxContainer = %keybind1
@onready var buttonMoveSFX: AudioStreamPlayer = $"../../../buttonMoveSFX"

func _ready() -> void:
	for item in $VBoxContainer.get_children():
		if item is Control:
			item.focus_entered.connect(_on_item_focus_entered.bind(item))

func _on_item_focus_entered(item: Control) -> void:
	ensure_control_visible(item)

# Connect this in your _ready or when generating keybinds
func _setup_keybind_focus():
	var keybind_list = $keybinds/ScrollContainer/VBoxContainer
	for hbox in keybind_list.get_children():
		if hbox is HBoxContainer:
			hbox.focus_mode = Control.FOCUS_ALL # Make the row focusable
			hbox.focus_entered.connect(_on_keybind_focused.bind(hbox))

func _on_keybind_focused(hbox: HBoxContainer):
	$keybinds/ScrollContainer.ensure_control_visible(hbox)
	buttonMoveSFX.play()
