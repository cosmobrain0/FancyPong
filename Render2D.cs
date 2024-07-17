using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Render2D;

public static class Render {
	public static GraphicsDevice graphicsDevice;
	public static SpriteBatch spriteBatch;
	
	/// <summary>
	/// Draws an axis-aligned rectangle
	/// </summary>
	/// <param name="topLeft">The coordinates of the top-left corner of the rectangle</param>
	/// <param name="size">The width and height of the rectangle</param>
	/// <param name="colour">The fill colour of this rectangle</param>
	public static void Rectangle(Vector2 topLeft, Vector2 size, Color colour)
	{
		VertexPositionColor[] vertices = new VertexPositionColor[4]
		{
			new VertexPositionColor(new Vector3(topLeft.X, topLeft.Y , 0), colour),
			new VertexPositionColor(new Vector3(topLeft.X+size.X, topLeft.Y, 0), colour),
			new VertexPositionColor(new Vector3(topLeft.X, topLeft.Y+size.Y, 0), colour),
			new VertexPositionColor(new Vector3(topLeft.X+size.X, topLeft.Y+size.Y, 0), colour)
		};
		short[] indices = new short[] { 0, 1, 2, 2, 1, 3 };
		BasicEffect effect = new BasicEffect(graphicsDevice)
		{
			VertexColorEnabled = true,
			Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0f, 0f, 1f)
		};

		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			graphicsDevice.DrawUserIndexedPrimitives(
				PrimitiveType.TriangleList,
				vertices,
				0,
				4,
				indices,
				0,
				indices.Length/3
			);
		}
	}

	/// <summary>
	/// Draws a regular shape to approximate a circle (with 100 corners by default)
	/// <br />
	/// Can use anywhere between 3 and 255 corners
	/// </summary>
	/// <param name="centre">The coordinates of the centre of the circle</param>
	/// <param name="radius">The radius of the circle</param>
	/// <param name="colour">The fill colour of the circle</param>
	/// <param name="pointCount">The number of points of the shape used to approximate the circle. This will be automatically clamped between 3 and 255</param>
	public static void Circle(Vector2 centre, float radius, Color colour, int pointCount=100)
	{
		pointCount = Math.Clamp(pointCount, 3, 255);
		VertexPositionColor[] vertices = new VertexPositionColor[pointCount];
		for (int i=0; i<pointCount; i++)
		{
			float theta = (i * 2f * (float)Math.PI) / pointCount;
			Vector2 offset = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * radius;
			Vector2 position = centre + offset;
			vertices[i] = new VertexPositionColor(new Vector3(position.X, position.Y, 0f), colour);
		}
		short[] indices = new short[3 * (pointCount-2)];
		for (short triangleIndex=0; triangleIndex < pointCount-2; triangleIndex++)
		{
			indices[triangleIndex*3 + 0] = 0;
			indices[triangleIndex*3 + 1] = (short)(triangleIndex+1);
			indices[triangleIndex*3 + 2] = (short)(triangleIndex+2);
		}

		BasicEffect effect = new BasicEffect(graphicsDevice)
		{
			VertexColorEnabled = true,
			Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0f, 0f, 1f)
		};

		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			graphicsDevice.DrawUserIndexedPrimitives(
				PrimitiveType.TriangleList,
				vertices,
				0,
				pointCount,
				indices,
				0,
				pointCount-2
			);
		}
	}

	/// <summary>
	/// Draws a regular shape to approximate a circle (with 100 corners by default)
	/// </summary>
	/// <param name="centre">The coordinates of the centre of the circle</param>
	/// <param name="radius">The radius of the circle</param>
	/// <param name="effect">The shader/effect to draw with</param>
	/// <param name="passIndices">The indices of the passes of the effect to draw with, in order</param>
	/// <param name="pointCount">The number of points of the shape used to approximate the circle. This will be automatically clamped between 3 and 255</param>
	public static void Circle(Vector2 centre, float radius, Effect effect, int[] passIndices, int pointCount=100)
	{
		pointCount = Math.Clamp(pointCount, 3, 255);
		VertexPosition[] vertices = new VertexPosition[pointCount];
		for (int i=0; i<pointCount; i++)
		{
			float theta = (i * 2f * (float)Math.PI) / pointCount;
			Vector2 offset = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * radius;
			Vector2 position = centre + offset;
			// FIXME: normalise position [-1, 1]
			vertices[i] = new VertexPosition(new Vector3(position.X, position.Y, 0f));
		}
		short[] indices = new short[3 * (pointCount-2)];
		for (short triangleIndex=0; triangleIndex < pointCount-2; triangleIndex++)
		{
			indices[triangleIndex*3 + 0] = 0;
			indices[triangleIndex*3 + 1] = (short)(triangleIndex+1);
			indices[triangleIndex*3 + 2] = (short)(triangleIndex+2);
		}

		graphicsDevice.BlendState = BlendState.Additive;
        graphicsDevice.DepthStencilState = DepthStencilState.None;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
		SetValue(effect, "WorldViewProjection", Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0f, 0f, 1f));
		SetValue(effect, "Radius", radius);
		SetValue(effect, "CircleCentre", centre);

		foreach (int index in passIndices)
		{
	        effect.CurrentTechnique.Passes[index].Apply();
			graphicsDevice.DrawUserIndexedPrimitives(
				PrimitiveType.TriangleList,
				vertices,
				0,
				pointCount,
				indices,
				0,
				pointCount-2
			);
		}
	}

	public static bool SetValue(Effect effect, string name, object value)
	{
		if (effect.Parameters[name] == null) return false;
		if (value is Vector2) effect.Parameters[name].SetValue((Vector2)value);
		else if (value is Matrix) effect.Parameters[name].SetValue((Matrix)value);
		else if (value is float) effect.Parameters[name].SetValue((float)value);
		else if (value is int) effect.Parameters[name].SetValue((int)value);
		else if (value is bool) effect.Parameters[name].SetValue((bool)value);
		else return false;
		return true;
	}

	/// <summary>
	/// Draws a triangle. Most rendering libraries require that the vertices are specified in a clockwise order,
	/// but this method doesn't care about the order of the vertices.
	/// </summary> 
	/// <param name="a">A corner of the triangle</param>
	/// <param name="b">A corner of the triangle</param>
	/// <param name="c">A corner of the triangle</param>
	/// <param name="colour">The fill colour of this triangle</param>
	public static void Triangle(Vector2 a, Vector2 b, Vector2 c, Color colour)
	{
		VertexPositionColor[] vertices = new VertexPositionColor[3]
		{
			new VertexPositionColor(new Vector3(a.X, a.Y, 0), colour),
			new VertexPositionColor(new Vector3(b.X, b.Y, 0), colour),
			new VertexPositionColor(new Vector3(c.X, c.Y, 0), colour)
		};
		short[] indices = new short[6] { 0, 1, 2, 2, 1, 0 };
		BasicEffect effect = new BasicEffect(graphicsDevice)
		{
			VertexColorEnabled = true,
			Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0f, 0f, 1f)
		};

		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			graphicsDevice.DrawUserIndexedPrimitives(
				PrimitiveType.TriangleList,
				vertices,
				0,
				3,
				indices,
				0,
				2
			);
		}
	}

	/// <summary>
	/// Draws a line segment that is one pixel thick between two points
	/// </summary>
	/// <param name="start">One of the endpoints of the line</param>
	/// <param name="end">One of the endpoints of the line</param>
	/// <param name="colour">The colour of the line</param>
	public static void ThinLine(Vector2 start, Vector2 end, Color colour)
	{
		VertexPositionColor[] vertices = new VertexPositionColor[2]
		{
			new VertexPositionColor(new Vector3(start.X, start.Y, 0), colour),
			new VertexPositionColor(new Vector3(end.X, end.Y, 0), colour)
		};
		short[] indices = new short[2] { 0, 1 };
		BasicEffect effect = new BasicEffect(graphicsDevice)
		{
			VertexColorEnabled = true,
			Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0f, 0f, 1f)
		};

		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			graphicsDevice.DrawUserIndexedPrimitives(
				PrimitiveType.LineList,
				vertices,
				0,
				2,
				indices,
				0,
				1
			);
		}
	}

	/// <summary>
	/// Draw a straight, filled, thick line segments between two endpoints,
	/// <br />
	/// with straight edges at the endpoints. If you want rounded endpoints,
	/// <br />
	/// have a look at `Render.RoundedLine`
	/// </summary>
	/// <param name="start">One of the endpoints of the line</param>
	/// <param name="end">One of the endpoints of the line</param>
	/// <param name="thickness">The thickness of the line</param>
	/// <param name="colour">The colour of the line</param>
	public static void Line(Vector2 start, Vector2 end, float thickness, Color colour)
	{
		Vector2 offset = end-start;
		Vector2 normalOffset = new Vector2(offset.Y, -offset.X);
		normalOffset.Normalize();
		normalOffset *= thickness/2f;
		Vector2[] points = new Vector2[4]
		{
			start+normalOffset,
			end+normalOffset,
			start-normalOffset,
			end-normalOffset
		};
		Triangle(points[0], points[1], points[2], colour);
		Triangle(points[1], points[2], points[3], colour);
	}

	/// <summary>
	/// Draw a straight, filled, thick line segment between two endpoints,
	/// <br />
	/// with semicircles at either end, instead of a straight edge.
	/// <br />
	/// if you would like a straight edge, then use `Render.Line`
	/// </summary>
	/// <param name="start">One of the endpoints of the line</param>
	/// <param name="end">One of the endpoints of the line</param>
	/// <param name="thickness">The thickness of the line</param>
	/// <param name="colour">The colour of the line</param>
	public static void RoundedLine(Vector2 start, Vector2 end, float thickness, Color colour, int edgePointCount=50)
	{
		Line(start, end, thickness, colour);
		Circle(start, thickness/2f, colour, edgePointCount*2);
		Circle(end, thickness/2f, colour, edgePointCount*2);
	}

	/// <summary>
	/// Draw a concave polygon out of straight edges, between points which
	/// are specified either consistently in a clockwise order,
	/// or consistently in a counter-clockwise order.
	/// </summary>
	/// <param name="points">The points of the polygon, either in a clockwise order or a counter-clockwise order</param>
	/// <param name="colour">The colour of the shape</param>
	public static void ConvexPolygon(Vector2[] points, Color colour)
	{
		for (int i=1; i<points.Length-1; i++)
			Triangle(points[0], points[i], points[i+1], colour);
	}
}
