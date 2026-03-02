extends Control

@onready var progress_bar = $ProgressBar
var scene_path = "res://Main3DScene.tscn"
var progress = []

func _ready():
	ResourceLoader.load_threaded_request(scene_path)

func _process(delta):
	var status = ResourceLoader.load_threaded_get_status(scene_path, progress)
	if status == ResourceLoader.THREAD_LOAD_IN_PROGRESS:
		progress_bar.value = progress[0] * 100
	elif status == ResourceLoader.THREAD_LOAD_LOADED:
		var scene = ResourceLoader.load_threaded_get(scene_path)
		get_tree().change_scene_to_packed(scene)
