extends RigidBody3D
@export var mesh: MeshInstance2D
@export var outlineMaterial: Material


func _on_mouse_exited() -> void:
	mesh.material_overlay = null

func _on_static_body_3d_mouse_entered() -> void:
	mesh.material_overlay = outlineMaterial




func _on_static_body_3d_mouse_exited() -> void:
	pass # Replace with function body.
