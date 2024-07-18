using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Render2D;
using Easing;

namespace FancyPong;

public class CollisionEffect
{
	public static Effect collisionEffectShader;
	Vector2 centre;
	float maxRadius;
	TimeSpan duration;
	DateTime startTime;
	Vector2 direction;
	public bool Complete { get => startTime + duration <= DateTime.Now; }

    public CollisionEffect(Vector2 centre, float maxRadius, DateTime startTime, TimeSpan duration, Vector2 direction)
    {
        this.centre = centre;
        this.maxRadius = maxRadius;
        this.startTime = startTime;
        this.duration = duration;
        this.direction = direction/direction.Length();
    }

    public CollisionEffect(Vector2 centre, float maxRadius, TimeSpan duration, Vector2 direction)
    {
        this.centre = centre;
        this.maxRadius = maxRadius;
        startTime = DateTime.Now;
        this.duration = duration;
        this.direction = direction/direction.Length();
    }

    public void Draw()
	{
		if (Complete) return;
		Render.SetValue(collisionEffectShader, "Progress", (float)((DateTime.Now-startTime).TotalMilliseconds/duration.TotalMilliseconds));
		Render.SetValue(collisionEffectShader, "Direction", direction);
		Render.Rectangle(centre - Vector2.One*maxRadius, Vector2.One*maxRadius*2, collisionEffectShader, new int[] { 0 });
	}
}

