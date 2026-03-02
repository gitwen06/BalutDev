extends Node2D

@onready var player = get_tree().get_first_node_in_group("player")
@onready var label = $Label

const baseText = "[E] to Touch"
var activeAreas = []
var canInteract = true

func registerArea(area: interactionArea):
	activeAreas.push_back(area)

func unregisterArea(area: interactionArea):
	var index = activeAreas.find(area)
	if index != -1:
		activeAreas.remove_at(index)

func _process(delta):
	if activeAreas.size() > 0 && canInteract:
		activeAreas.sort_custom(sort_by_distance_to_player)
		label.text = baseText + activeAreas[0].action_name
		label.global_position = activeAreas[0].global_positon
<<<<<<< Updated upstream
<<<<<<< Updated upstream
		label.global_position.y -= 36
		label.global_position.x -= label.size.x / 2
		label.show()
	else:
		label.hide()
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

func sort_by_distance_to_player(area1, area2):
	var area1_to_player = player.global_position.distamce_to(area1.global_position)
	var area2_to_player = player.global_position.distamce_to(area2.global_position)
	return area1_to_player > area2_to_player
<<<<<<< Updated upstream
<<<<<<< Updated upstream
	
func _input(event):
	if event.is_action_pressed("interact") && canInteract:
		if activeAreas.size() > 0:
			canInteract = false
			label.hide()
				
			await activeAreas[0].interact.call()
			canInteract = true
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
