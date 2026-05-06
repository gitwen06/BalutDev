extends CanvasLayer

@onready var pauseOptions = [%continuePause, %questPause, %keybindsPause, %optionsPause, %leavePause]
@onready var cursor_box = %cursor

func _ready() -> void:
	for btn in pauseOptions:
		# fucus handler
		btn.mouse_entered.connect(func(): btn.grab_focus())
		# some visual updates to not fuck it up
		btn.focus_entered.connect(_update_selection.bind(btn))
		# handles the action
		btn.pressed.connect(_handle_selection.bind(btn))
	_close_pause_menu()

func _update_selection(btn: Button) -> void:
	# updates the highlighted button
	for b in pauseOptions:
		b.modulate = Color("8b003b") if b == btn else Color.WHITE #selected/unselected
	var tween = create_tween()
	tween.tween_property(cursor_box, "global_position:y", btn.global_position.y, 0.1).set_trans(Tween.TRANS_SINE)

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("paused"):
		if not visible: _open_pause_menu()
		else: _close_pause_menu()
		
	if visible and event.is_action_pressed("menuAccept"):
		var focused_node = get_viewport().gui_get_focus_owner()
		if focused_node in pauseOptions:
			_handle_selection(focused_node)

# ------ HANDLES THE ACTIONS ------
func _handle_selection(btn: Button) -> void:
	if btn == %continuePause:
		_close_pause_menu()
	elif btn == %leavePause:
		get_tree().quit()
	else:
		print("Selected: ", btn.text)

func _open_pause_menu() -> void:
	show()
	get_tree().paused = true
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	%continuePause.grab_focus()

func _close_pause_menu() -> void:
	hide()
	get_tree().paused = false
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
