using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Render2D;
using Easing;
using System.Collections.Generic;

namespace FancyPong;

public class Particle
{
	public static Effect particleShader;
    static Random generator = new Random((int) (DateTime.Now.Ticks & 0b11111111111111111111111111111111));
	public static float RandFloat { get => generator.NextSingle(); }
	Vector2 position;
	Vector2 velocity;
	float radius;
	Vector3 colour;
	public bool Dead { get => radius <= 0.01; }

    public Particle(Vector2 position, Vector2 velocity, float radius, Vector3 colour)
    {
        this.position = position;
        this.velocity = velocity;
        this.radius = radius;
		this.colour = colour;
    }

	public void Update(GameTime gameTime, Vector2 screenSize)
	{
		float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
		if (Dead) return;
		position += velocity * dt;
		radius -= dt/1000 * 20;

		if (position.Y + radius > screenSize.Y && velocity.Y > 0)
		{
			position.Y = screenSize.Y-radius;
			velocity.Y *= -1;
		}
		else if (position.Y-radius < 0 && velocity.Y < 0)
		{
			position.Y = radius;
			velocity.Y *= -1;
		}
		if (position.X + radius > screenSize.X && velocity.X > 0)
		{
			position.X = screenSize.X-radius;
			velocity.X *= -1;
		}
		else if (position.X-radius < 0 && velocity.X < 0)
		{
			position.X = radius;
			velocity.X *= -1;
		}
	}

	public void Draw()
	{
		Render.SetValue(particleShader, "BaseColour", colour);
		Render.Circle(position, radius, particleShader, new int[] { 0 });
	}
}

