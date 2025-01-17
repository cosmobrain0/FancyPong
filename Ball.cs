using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Render2D;
using Easing;

namespace FancyPong;

// FEATURE-REQUEST: ball has spin based on the movement/rotation of the paddle upon contact
public class Ball
{
	public static Effect ballShader;
	const float acceleration = 0.01f;
	const float angularAcceleration = 3f;
	Vector2 centre;
	Vector2 velocity;
	float targetRadius;
	public float TargetSpeed { get => TargetVelocity.Length(); set => TargetVelocity *= value / TargetVelocity.Length(); }
    public Vector2 TargetVelocity { get; set; }
	public float TargetRadius { get => targetRadius; set => targetRadius = Math.Max(0, value); }
    float radius;

	public Vector2 Centre { get => centre; }
	public float Radius { get => radius; }

	public Ball(Vector2 _centre, Vector2 _velocity, float _radius)
	{
		centre = _centre;
		velocity = _velocity;
		radius = _radius;
		targetRadius = radius;
		TargetVelocity = velocity;
	}

	public void Draw(TimeSpan speedBoostDuration, TimeSpan speedBoostTime, GameTime gameTime)
	{
		// TODO: finish this
		Render.SetValue(ballShader, "Velocity", new Vector2(velocity.X, velocity.Y));
		Render.SetValue(ballShader, "SpeedBoostDuration", (float)speedBoostDuration.TotalMilliseconds);
		Render.SetValue(ballShader, "SpeedBoostTime", (float)speedBoostTime.TotalMilliseconds);
		Render.SetValue(ballShader, "Time", (float)gameTime.TotalGameTime.TotalMilliseconds);
		Render.Circle(centre, radius, ballShader, new int[] { 0 });
	}

	public Side? Update(GameTime gameTime, Vector2 screenSize, Paddle[] paddles, bool speedBoost, Action<Vector2, float, TimeSpan, Vector2> triggerCollisionEffect, Action<Particle> spawnParticle)
	{
		float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

		Vector3 colour = speedBoost ? new Vector3(242, 235, 12)/255f : Vector3.One;
		
		float velocityAngle = (float)Math.Atan2(velocity.Y, velocity.X);
		float speed = velocity.Length();
		float targetAngle = (float)Math.Atan2(TargetVelocity.Y, TargetVelocity.X);

		speed = MoveTowards(speed, TargetSpeed, acceleration);
		velocityAngle = MoveTowards(velocityAngle, velocityAngle + (float)Ease.AngleDistance(velocityAngle, targetAngle), angularAcceleration);
		velocity = new Vector2((float)Math.Cos(velocityAngle), (float)Math.Sin(velocityAngle)) * speed;
		
		centre += velocity*dt;

		radius = MoveTowards(radius, targetRadius, 0.03f*dt);

		// TODO: make this move backwards by the right amount instead
		if (centre.Y + radius > screenSize.Y && velocity.Y > 0)
		{
			centre.Y = screenSize.Y-radius;
			velocity.Y *= -1;
			TargetVelocity = new Vector2(TargetVelocity.X, -TargetVelocity.Y);
		}
		else if (centre.Y-radius < 0 && velocity.Y < 0)
		{
			centre.Y = radius;
			velocity.Y *= -1;
			TargetVelocity = new Vector2(TargetVelocity.X, -TargetVelocity.Y);
		}
		if (centre.X + radius > screenSize.X && velocity.X > 0)
			return Side.Right;
		else if (centre.X-radius < 0 && velocity.X < 0)
			return Side.Left;

		for (int i=0; i<dt; i++)
		{
			float particleRadius = Particle.RandFloat*radius * 0.4f;
			float position = (Particle.RandFloat*2 - 1)*(radius - particleRadius);
			Vector2 offset = new Vector2(-velocity.Y, velocity.X);
			offset = offset/offset.Length() * position;
            Vector2 particlePosition = this.centre + offset;
            Vector2 target = this.centre - velocity * dt * 10;
			Vector2 particleVelocity = target-particlePosition;
			particleVelocity = particleVelocity/particleVelocity.Length() * Particle.RandFloat/3;
			spawnParticle(new Particle(particlePosition, particleVelocity, particleRadius, colour));
		}

		foreach (Paddle paddle in paddles)
		{
			(Vector2, Vector2)? dataMaybe = paddle.BallCollisionData(centre, radius, velocity);
			if (dataMaybe != null)
			{
				Vector2 normal;
				Vector2 collisionPoint;
				(normal, collisionPoint) = ((Vector2, Vector2))dataMaybe;
				velocity = normal / normal.Length() * velocity.Length();
				// TargetVelocity = velocity / velocity.Length() * TargetSpeed;
				TargetVelocity = velocity;
				centre = collisionPoint + normal/normal.Length() * radius;
				triggerCollisionEffect(collisionPoint, radius*2, TimeSpan.FromMilliseconds(350), normal);

				for (int i=0; i<25; i++)
				{
					float angle = (float)(Particle.RandFloat*Math.PI - Math.PI/2 + Math.Atan2(normal.Y, normal.X));
					float particleSpeed = Particle.RandFloat*0.25f;
					float particleRadius = Particle.RandFloat*radius/8 + radius/2;
					spawnParticle(new Particle(collisionPoint, new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle))*speed, particleRadius, colour));
				}
				break;
			}
		}
		return null;
	}

	private float MoveTowards(float start, float end, float maxStep)
	{
		if (end > start)
		{
			float result = start + maxStep;
			return result > end ? end : result;
		}
		else
		{
			float result = start - maxStep;
			return result < end ? end : result;
		}
	}
}

