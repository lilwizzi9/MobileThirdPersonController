using Godot;
using System;

public partial class Movement : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	[Export]
	public bool DetachCam = false;
	[Export]
	public bool CanFaceCam   = false;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	private SpringArm3D SpringArm;
	private Node3D CharacterHandler;
	private bool IsCrouched;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		SpringArm = GetNode<SpringArm3D>("./SpringArm3D");
		CharacterHandler = GetNode<Node3D>("./Collision/CharacterViewModelHandler");
		SpringArm.AddExcludedObject(this.GetRid());
		// AnimPlayer = GetNode<AnimationPlayer>("./AnimationPlayer");
	}
	// Called when input is Given
	public Vector2 fingerPosition = Vector2.Zero;
	public int TouchIndex;
	public float TouchSensitivity = .01f;
	private Vector3 camRotTmp = Vector3.Zero;  
	public override void _UnhandledInput(InputEvent @event)
	{

		if(@event is InputEventScreenTouch input){
			TouchIndex = input.Index;
			fingerPosition = input.Position;
		}
		if(@event is InputEventScreenDrag Input){
			if(TouchIndex == Input.Index ){
				var inputValue = (Input.Position - fingerPosition)*TouchSensitivity;
				var tmpYrot = !DetachCam?   this.RotationDegrees.Y+inputValue.X*-30:SpringArm.RotationDegrees.Y+inputValue.X*-30;
				// camera.RotateX(inputValue.Y*-.5f);
				var tmpXrot = SpringArm.RotationDegrees.X+inputValue.Y*-30;
				camRotTmp = new Vector3(Math.Clamp(tmpXrot,-80,80),tmpYrot,0);
				fingerPosition = Input.Position;
			}
			
		}
	}

	
	private void UpdateViewMeshRot(){
		double delta = this.GetProcessDeltaTime();
		
		if(DetachCam){	
			if(!IsOnFloor()){return;}		
			// CharacterHandler.GlobalRotation = new Vector3(0,
			// (float)Mathf.MoveToward(CharacterHandler.GlobalRotation.Y,SpringArm.GlobalRotation.Y,delta*30) 
			// ,0);
			//In Motion
			if(Velocity!=Vector3.Zero){
				CharacterHandler.GlobalRotation = new Vector3(0,
				(float)Mathf.LerpAngle(CharacterHandler.GlobalRotation.Y,SpringArm.GlobalRotation.Y,delta*25) 
				,0);
			}
			
		}
	}
	private Vector3 lastDirection =Vector3.Zero;
	private bool inMotion= false;
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		// SpringArm.RotationDegrees = new Vector3(
		// 	Mathf.MoveToward(camRotTmp.X,SpringArm.RotationDegrees.X,(float)delta*2),
		// 	DetachCam? SpringArm.RotationDegrees.Y:Mathf.MoveToward(camRotTmp.Y,SpringArm.RotationDegrees.Y,(float)delta*2),
		// 	SpringArm.RotationDegrees.Z
		// );
		UpdateViewMeshRot();
		if(!DetachCam){
			SpringArm.RotationDegrees = new Vector3(
				Mathf.MoveToward(camRotTmp.X,SpringArm.RotationDegrees.X,(float)delta*2),
				SpringArm.RotationDegrees.Y,
				SpringArm.RotationDegrees.Z
			);
			this.RotationDegrees = new Vector3(
				this.RotationDegrees.X,
				Mathf.MoveToward(camRotTmp.Y,SpringArm.RotationDegrees.Y,(float)delta*2),
				this.RotationDegrees.Z
			);
		}else{
			SpringArm.RotationDegrees = new Vector3(
				Mathf.MoveToward(camRotTmp.X,SpringArm.RotationDegrees.X,(float)delta*2),
				Mathf.MoveToward(camRotTmp.Y,SpringArm.RotationDegrees.Y,(float)delta*2),
				SpringArm.RotationDegrees.Z
			);
		}
		
		

		// Add the gravity.
		
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;


		if (Input.IsActionJustPressed("ui_accept")&& IsOnFloor()){
			if(IsCrouched){
				// if(ToggleCrouch()){
				// 	velocity.Y = JumpVelocity;
				// }
				velocity.Y = JumpVelocity;
			}else{
				velocity.Y = JumpVelocity;
			}
			
		}
		// if (Input.IsActionJustPressed("crouch")){
			//ToggleCrouch();
		// }
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if(IsOnFloor()){
			
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			// direction= direction.Rotated(Vector3.Up,(float)(SpringArm.RotationDegrees.Y*Math.PI)/1800);
			if(DetachCam){
				direction= direction.Rotated(Vector3.Up,SpringArm.GlobalRotation.Y);
			}
			
			if (direction != Vector3.Zero)
			{
				
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			}
			lastDirection = direction;
		}else{
			if (lastDirection != Vector3.Zero)
			{
				velocity.X = lastDirection.X * Speed;
				velocity.Z = lastDirection.Z * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed/4);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed/4);
			}
		}
		
		Velocity = velocity;
		MoveAndSlide();
	}
}
