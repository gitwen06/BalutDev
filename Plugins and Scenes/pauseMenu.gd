extends CanvasLayer

# --- Tab References ---
@onready var tabs = {
	"main": $paused,
	"options": $options, 
	"keybinds": $keybinds,
	"credits": $credits
}
## -------- Standard References --------
@onready var pauseOptions = [%continuePause, %keybindsPause, %optionsPause, %creditsPause, %leavePause]
@onready var cursor_box = %cursor
@onready var pausedAnim = $paused/menuOpenClose
@onready var optionsAnim: AnimationPlayer = $options/optionsOpenClose
@onready var keybindsAnim: AnimationPlayer = $keybinds/keybindsOpenClose
@onready var creditsAnim: AnimationPlayer = %creditsOpenClose
@onready var buttonMoveSFX: AudioStreamPlayer = $buttonMoveSFX
@onready var buttonSelectSFX: AudioStreamPlayer = $buttonSelectSFX

var isPaused = false
var isAnimated = false
var current_tab = "main"

func _ready() -> void:
	hide()
	for btn in pauseOptions:
		btn.mouse_entered.connect(func(): btn.grab_focus())
		btn.focus_entered.connect(_update_selection.bind(btn))
		btn.pressed.connect(_handle_selection.bind(btn))
	_close_pause_menu()

## -------- Updates the Text Highlight --------
func _update_selection(btn: Button) -> void:
	# safety return
	if isAnimated: return 
	
	buttonMoveSFX.play() 
	var tween = create_tween().set_parallel(true)
	
	for b in pauseOptions:
		if b == btn:
			# Highlighted COlor, Slide to the Right, and Scale for no reason
			b.modulate = Color("8b003b") 
			tween.tween_property(b, "position:x", 280.0, 0.1).set_trans(Tween.TRANS_SINE) 
			tween.tween_property(b, "scale", Vector2(1.1, 1.1), 0.1).set_trans(Tween.TRANS_SINE) 
			# Cursor following the button currently highlighted
			tween.tween_property(cursor_box, "global_position:y", btn.global_position.y + 15, 0.1).set_trans(Tween.TRANS_SINE) 
		else:
			# When unselected it resets the values and bring them back to normal.
			b.modulate = Color.WHITE 
			tween.tween_property(b, "position:x", 250.0, 0.1).set_trans(Tween.TRANS_SINE) 
			tween.tween_property(b, "scale", Vector2(1.0, 1.0), 0.1).set_trans(Tween.TRANS_SINE) 

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("paused"):
		if not visible: 
			_open_pause_menu()
		else:
			if current_tab != "main":
				switch_tab("main")
			else:
				if not isPaused:
					_open_pause_menu()
				else:
					_close_pause_menu()
				
		if visible and event.is_action_pressed("menuAccept"):
			var focused_node = get_viewport().gui_get_focus_owner()
		# Check main pause buttons
			if focused_node in pauseOptions:
				_handle_selection(focused_node)
		# Also checks buttons in other tabs
			elif focused_node is Button or focused_node is CheckBox:
				_handle_selection(focused_node)

## --------Tab Switching --------
func switch_tab(tab_name: String) -> void:
	# Play a click sound for the transition
	buttonSelectSFX.play()
	
	if tab_name == "main":
		if current_tab == "options":
			optionsAnim.play_backwards("optionsTab")
			await optionsAnim.animation_finished 
		elif current_tab == "keybinds":
			keybindsAnim.play_backwards("keybindsOpenClose")
			await keybindsAnim.animation_finished
		elif current_tab == "credits":
			creditsAnim.play_backwards("creditsOpenClose")
	
	for t in tabs.values():
		t.hide() # hides all containers
	tabs[tab_name].show()
	current_tab = tab_name
	
	if tab_name == "main":
		pausedAnim.play("cursorpauseOpen")
		%continuePause.grab_focus()
		_update_selection(%continuePause)
	elif tab_name == "options":
		$options/optionsOpenClose.play("optionsTab")
		%muteButton.grab_focus()
	elif tab_name == "keybinds":
		keybindsAnim.play("keybindsOpenClose")
	elif tab_name == "credits":
		creditsAnim.play("creditsOpenClose")

## -------- Tab Switching Functions --------
func _handle_selection(btn: Button) -> void:
	buttonSelectSFX.play() 
	if btn == %continuePause:
		_close_pause_menu()
	elif btn == %optionsPause:
		switch_tab("options")
	elif btn == %keybindsPause:
		switch_tab("keybinds")
	elif  btn == %creditsPause:
		switch_tab("credits")
	elif btn == %leavePause:
		get_tree().quit() 
	else:
		print("Selected: ", btn.text) 

func _open_pause_menu() -> void:
	isPaused = true
	buttonSelectSFX.play() 
	show() 
	_update_selection(%continuePause) 
	get_tree().paused = true 
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE 
	cursor_box.global_position.y = %continuePause.global_position.y + 15 
	get_viewport().warp_mouse(%continuePause.global_position + (%continuePause.size / 2)) 
	isAnimated = true 
	pausedAnim.play("cursorpauseOpen") #a
	await pausedAnim.animation_finished 
	isAnimated = false # when it finishes the animtaions it updates the selected button
	%continuePause.grab_focus() 

func _close_pause_menu() -> void:
	isPaused = false
	buttonSelectSFX.play() 
	isAnimated = true 
	pausedAnim.play_backwards("cursorpauseOpen") 
	await pausedAnim.animation_finished 
	hide() 
	get_tree().paused = false 
	isAnimated = false 
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

# --- Options Tab ---
##func _open_options_menu():
	#print("gay ass shellwin")

func _on_master_volume_value_changed(value: float) -> void:
	var linear_value = value / 100.0 # to prevent the crushed sound
	AudioServer.set_bus_volume_db(0, linear_to_db(linear_value))

func _on_mute_button_toggled(toggled_on: bool) -> void:
	if isAnimated: return
	buttonSelectSFX.play()
	AudioServer.set_bus_mute(0, toggled_on)
