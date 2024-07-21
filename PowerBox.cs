using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Render2D;
using Easing;
using System.Collections.Generic;
using System.Linq;

namespace FancyPong;

public enum PowerBoxType
{
	Speed,
	Ice,
}

public class PowerBox
{
	public static Effect speedShader;
	public static Effect iceShader;
	public static Action triggerSpeed;
	public static Action triggerIce;

	// powerboxes will be square
	public static readonly float sideLength = 55;
	Vector2 position;
	PowerBoxType boxType;

    public PowerBox(Vector2 position, PowerBoxType boxType)
    {
        this.position = position;
        this.boxType = boxType;
    }

    public void Draw(GameTime gameTime)
	{
		Effect shader = GetData(boxType).effect;
		Render.SetValue(shader, "Time", (float)gameTime.TotalGameTime.TotalMilliseconds);
		Render.Rectangle(position, Vector2.One*sideLength, shader, new int[] { 0 });
	}

	/// <summary>
	/// Returns true if this box intersects with the specified circle
	/// </summary>
	public bool Intersects(Vector2 centre, float radius)
	{
		Vector2 closest = Vector2.Clamp(centre, position, position + Vector2.One*sideLength);
		return (closest-centre).LengthSquared() <= radius*radius;
	}

	public void Trigger()
	{
		GetData(boxType).trigger();
	}

	public static (Effect effect, Action trigger) GetData(PowerBoxType powerBoxType)
	{
		return powerBoxType switch {
			PowerBoxType.Speed => (speedShader, triggerSpeed),
			PowerBoxType.Ice => (iceShader, triggerIce),
		};
	}
}

