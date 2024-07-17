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
	Vector2 position;
	public Side side;
	public const float height = 50;
	public const float width = 10;
	public const float speed = 0.1f;

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
		Render.Rectangle(new Vector2(position.X, position.Y-height/2), new Vector2(width, height), Color.White);
	}

	/// <summary>
	/// If the specified ball is touching this paddle,
	/// then this method returns the normal of the surface of the paddle
	/// at the point of collision. Otherwise,
	/// it returns null.
	/// <summary>
	/// <param name="centre">The centre of the ball</param>
	/// <param name="radius">The radius of the ball</param>
	public (Vector2 normal, Vector2 newPosition)? BallCollisionData(Vector2 centre, float radius, Vector2 velocity)
	{
		// TODO: use an quadratic collision surface
		if (centre.Y != Math.Clamp(centre.Y, position.Y-height/2 - radius, position.Y+height/2 + radius))
			return null;
		if (side == Side.Left && velocity.X < 0)
		{
			float x = position.X + width;
			if (centre.X > x && centre.X-radius <= x) return (Vector2.UnitX, new Vector2(position.X+width+radius, centre.Y));
			else return null;
		}
		else if (side == Side.Right && velocity.X > 0)
		{
			float x = position.X;
			if (centre.X < x && centre.X+radius >= x) return (-Vector2.UnitX, new Vector2(position.X-radius, centre.Y));
			else return null;
		}
		return null;
	}
}

