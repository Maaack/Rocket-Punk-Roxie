using Godot;
using System;

public partial class Asteroid2D : RigidBody2D
{
	public enum Sizes {
		Smallest,
		Small,
		Medium,
		Large,
		Largest,
	}

	[Export]
	public PackedScene PartScene;
	[Export]
	public Sizes Size = Sizes.Large;
	[Export]
	public int SplitParts = 2;
	[Export]
	public float SplitForce = 1.0f;
	private bool Destroyed = false;
	private bool PlayerDestroyed = false;

	public bool IsDestroyed()
	{
		return Destroyed;
	}

	public bool IsPlayerDestroyed()
	{
		return PlayerDestroyed;
	}

	private Godot.Collections.Dictionary SizeScaleMap = new Godot.Collections.Dictionary{
		{(int)Sizes.Smallest, 0.125f},
		{(int)Sizes.Small, 0.25f},
		{(int)Sizes.Medium, 0.5f},
		{(int)Sizes.Large, 1.0f},
		{(int)Sizes.Largest, 2.0f},
	};
	
	private RigidBody2D SpawnPart(Vector2 offsetPosition, Vector2 offsetVelocity)
	{
		var partInstance = PartScene.Instantiate<RigidBody2D>();
		partInstance.Transform = GetTransform();
		partInstance.Position += offsetPosition;
		partInstance.LinearVelocity = LinearVelocity + offsetVelocity;
		partInstance.AngularVelocity = AngularVelocity * 2;
		if (partInstance is Asteroid2D asteroidPart)
		{
			asteroidPart.Size = Size - 1;
		}
		GetParent().CallDeferred("add_child", partInstance);
		return partInstance;
	}
	
	private float GetScaleModifier()
	{
		return (float)SizeScaleMap[(int)Size];
	}
	
	private void Destroy(int damagingTeam)
	{
		if ( IsDestroyed() ) { return; }
		if ( damagingTeam == (int)Constants.Teams.Player ){
			PlayerDestroyed = true;
		}
		Destroyed = true;
		QueueFree();
	}

	private void OnHurtArea2DDamageReceived(double damageAmount, int damagingTeam, double damageAngle)
	{
		if ( IsDestroyed() ) { return; }
		if ( Size > 0 )
		{
			var partAngleDifference = (2.0f * Math.PI) / (float)SplitParts;
			for ( int i = 0 ; i < SplitParts ; i ++ )
			{
				var splitAngle = (float)((partAngleDifference * i) + (partAngleDifference/2) + damageAngle);
				var positionOffset = Vector2.FromAngle(splitAngle) * 10.0f * GetScaleModifier();
				var velocityOffset = Vector2.FromAngle(splitAngle) * SplitForce;
				var partInstance = SpawnPart(positionOffset, velocityOffset);
			}

		}
		Destroy(damagingTeam);
	}

	private void UpdateAsteroidScale()
	{
		var spriteNode = GetNode<Sprite2D>("Sprite2D");
		var collisionShapeNode1 = GetNode<CollisionShape2D>("CollisionShape2D");
		var collisionShapeNode2 = GetNode<CollisionShape2D>("HurtArea2D/CollisionShape2D");
		var collisionShapeNode3 = GetNode<CollisionShape2D>("HitArea2D/CollisionShape2D");
		var circleShape = collisionShapeNode1.Shape.Duplicate() as CircleShape2D;
		var scaleMod = GetScaleModifier();
		Mass *= scaleMod;
		spriteNode.Scale = Vector2.One * scaleMod;
		circleShape.Radius *= scaleMod;
		collisionShapeNode1.Shape = circleShape;
		collisionShapeNode2.Shape = circleShape;
		collisionShapeNode3.Shape = circleShape;
	}

	public override void _Ready()
	{
		base._Ready();
		UpdateAsteroidScale();
	}
}


