using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Render2D;
using Easing;

namespace FancyPong;

public enum Side
{
	Left, Right
}

public class Paddle
{
	public static Effect paddleShader;
	Vector2 position;
	public Side side;
	public const float height = 75;
	public const float width = 15;
	public const float speed = 0.3f;

    public Paddle(Vector2 position, Side side)
    {
        this.position = position;
		this.side = side;
    }

	public void MoveUp(float minY, GameTime gameTime)
	{
		float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
		position.Y = Math.Max(minY+height/2, position.Y-speed*dt);
	}
	
	public void MoveDown(float maxY, GameTime gameTime)
	{
		float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
		position.Y = Math.Min(maxY-height/2, position.Y+speed*dt);
	}

	public void Draw()
	{
		Render.SetValue(paddleShader, "RightSide", side == Side.Right);
		Render.Rectangle(new Vector2(position.X, position.Y-height/2), new Vector2(width, height), paddleShader, new int[] { 0 });
		Vector2 p1 = new Vector2(position.X + (side == Side.Right ? width + 10 : -10), position.Y);
		Vector2 p2 = new Vector2(position.X + (side == Side.Right ? width : 0), position.Y+height/2);
		Vector2 p3 = new Vector2(position.X + (side == Side.Right ? width : 0), position.Y-height/2);
		Render.Triangle(p1, p2, p3, new Color(0.5f, 0.5f, 0.5f));
		// Render.ThinLine(p1, p2, Color.White);
		// Render.ThinLine(p1, p3, Color.White);
	}

	/// <summary>
	/// If the specified ball is touching this paddle,
	/// then this method returns the normal of the surface of the paddle
	/// at the point of collision. Otherwise,
	/// it returns null.
	/// <summary>
	/// <param name="centre">The centre of the ball</param>
	/// <param name="radius">The radius of the ball</param>
	public (Vector2 normal, Vector2 collisionPoint)? BallCollisionData(Vector2 centre, float radius, Vector2 velocity)
	{
		// TODO: bounding box check optimisation
		const int samples = 100;
		float bestSqrDistance = float.PositiveInfinity;
		Vector2 bestPoint = Vector2.Zero;
		for (int i=0; i<samples; i++)
		{
			float percentage = (float)i/(float)samples;
			Vector2 point = new Vector2(
				side == Side.Right ? position.X + width - (4*percentage - 4*percentage*percentage)*width : position.X + (4*percentage - 4*percentage*percentage)*width,
				position.Y - height/2 + percentage*height
			);
			float sqrDistance = (point-centre).LengthSquared();
			if (sqrDistance <= bestSqrDistance)
			{
				bestSqrDistance = sqrDistance;
				bestPoint = point;
			}
		}

		if (bestSqrDistance <= radius*radius)
		{
			Vector2 collisionPoint = bestPoint;
			float normalGradient = width/height * (8*((bestPoint.Y - (position.Y-height/2))/height) - 4);
			Vector2 normal = side == Side.Right ? new Vector2(-1, normalGradient) : new Vector2(1, normalGradient);
			return (normal, collisionPoint);
		}
		else return null;
	}
}

