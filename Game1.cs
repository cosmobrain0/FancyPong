using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Render2D;
using Easing;

namespace FancyPong;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    const int ScreenWidth = 800;
    const int ScreenHeight = 400;

    Effect playButtonShader;

    bool playing = false;
    DateTime menuStartTime = DateTime.MinValue;
    Ball ball;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        ball = new Ball(new Vector2(ScreenWidth, ScreenHeight)/2, new Vector2(0, 0), 25);
        ball.TargetVelocity = new Vector2(0.2f, 0.1f);
        ball.TargetRadius = 15f;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        playButtonShader = Content.Load<Effect>("playButtonShader");
        Ball.ballShader = Content.Load<Effect>("ballShader");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        MouseState mouseState = Mouse.GetState();
        Vector2 mouse = new Vector2(mouseState.X, mouseState.Y);

        if (playing)
        {
            ball.Update(gameTime, new Vector2(ScreenWidth, ScreenHeight));
        }
        else
        {
            if (menuStartTime == DateTime.MinValue) menuStartTime = DateTime.Now;
            if (mouseState.LeftButton == ButtonState.Pressed && (mouse-PlayButtonPosition()).LengthSquared() <= PlayButtonRadius()*PlayButtonRadius())
            {
                menuStartTime = DateTime.MinValue;
                playing = true;
            }
        }


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Render.graphicsDevice = GraphicsDevice;
        Render.spriteBatch = _spriteBatch;
        GraphicsDevice.Clear(Color.Black);

        MouseState mouseState = Mouse.GetState();

        if (playing)
        {
            ball.Draw();
        }
        else
        {
            DrawPlayButton(gameTime, mouseState);
        }

        base.Draw(gameTime);
    }

    private void DrawPlayButton(GameTime gameTime, MouseState mouseState)
    {
        double time = Ease.Clamp01(Ease.InverseLerp((DateTime.Now - menuStartTime).TotalMilliseconds, 1000, 2000));
        Render.SetValue(playButtonShader, "Time", (float)gameTime.TotalGameTime.TotalMilliseconds);
        Render.SetValue(playButtonShader, "Mouse", new Vector2(mouseState.Position.X, _graphics.PreferredBackBufferHeight - mouseState.Position.Y));
        Render.Circle(PlayButtonPosition(), PlayButtonRadius(), playButtonShader, new int[] { 0 });
    }

    private float PlayButtonRadius()
    {
        double time = Ease.Clamp01(Ease.InverseLerp((DateTime.Now - menuStartTime).TotalMilliseconds, 1000, 2000));
        return (float)Ease.Lerp(Ease.Smoothstep(time), 0, 25);
    }

    private Vector2 PlayButtonPosition()
    {
        return new Vector2(ScreenWidth, ScreenHeight) / 2;
    }
}
