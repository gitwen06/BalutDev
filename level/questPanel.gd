extends Panel

@onready var popup_list = $"."

func _input(event):
	if event.is_action_pressed("questPanel"):
		popup_list.visible = !popup_list.visible
		
		if popup_list.visible:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		else:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
