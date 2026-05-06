extends CanvasLayer

@onready var pauseOptions = [%continuePause, %questPause, %keybindsPause, %optionsPause, %leavePause]
@onready var cursor_box = %cursor
@onready var pausedAnim = $Control/menuOpenClose
@onready var buttonMoveSFX: AudioStreamPlayer = $buttonMoveSFX
@onready var buttonSelectSFX: AudioStreamPlayer = $buttonSelectSFX

var isAnimated = false

func _ready() -> void:
	for btn in pauseOptions:
		btn.mouse_entered.connect(func(): btn.grab_focus())
		btn.focus_entered.connect(_update_selection.bind(btn))
		btn.pressed.connect(_handle_selection.bind(btn))
	_close_pause_menu()
	
func _update_selection(btn: Button) -> void:
	if isAnimated: return 
	buttonMoveSFX.play() #sfx player
	var tween = create_tween().set_parallel(true)
	
	for b in pauseOptions:
		if b == btn:
			b.modulate = Color("8b003b")
			tween.tween_property(b, "position:x", 300.0, 0.1).set_trans(Tween.TRANS_SINE)
			tween.tween_property(b, "scale", Vector2(1.1, 1.1), 0.1).set_trans(Tween.TRANS_SINE)
			# moves the cursor to the currently focused button vv
			tween.tween_property(cursor_box, "global_position:y", btn.global_position.y, 0.1).set_trans(Tween.TRANS_SINE)
		else:
			b.modulate = Color.WHITE
			tween.tween_property(b, "position:x", 250.0, 0.1).set_trans(Tween.TRANS_SINE)
			tween.tween_property(b, "scale", Vector2(1.0, 1.0), 0.1).set_trans(Tween.TRANS_SINE)

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("paused"):
		if not visible: _open_pause_menu()
		else: _close_pause_menu()
		
	if visible and event.is_action_pressed("menuAccept"):
		var focused_node = get_viewport().gui_get_focus_owner()
		if focused_node in pauseOptions:
			_handle_selection(focused_node)

func _handle_selection(btn: Button) -> void:
	buttonSelectSFX.play() # Play here for all buttons
	if btn == %continuePause:
		_close_pause_menu()
	elif btn == %leavePause:
		get_tree().quit()
	else:
		print("Selected: ", btn.text)

func _open_pause_menu() -> void:
	buttonSelectSFX.play()
	show()
	get_tree().paused = true
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	#--- FORCES TO RESET BACK TO THE FIRST OPTION ---
	cursor_box.global_position.y = %continuePause.global_position.y
	_update_selection(%continuePause)
	
	get_viewport().warp_mouse(%continuePause.global_position + (%continuePause.size / 2))
	isAnimated = true
	pausedAnim.play("cursorpauseOpen")
	await pausedAnim.animation_finished
	isAnimated = false 
	%continuePause.grab_focus()

func _close_pause_menu() -> void:
	buttonSelectSFX.play()
	isAnimated = true
	pausedAnim.play_backwards("cursorpauseOpen")
	await pausedAnim.animation_finished 
	# before the canvas dissapheres, it plays the animation backwards
	hide()
	get_tree().paused = false
	isAnimated = false
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
