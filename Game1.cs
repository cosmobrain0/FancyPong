using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Render2D;
using Easing;
using System.Collections.Generic;

namespace FancyPong;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    const int ScreenWidth = 1200;
    const int ScreenHeight = 600;

    Effect playButtonShader;

    bool playing = false;
    DateTime menuStartTime = DateTime.MinValue;
    Ball ball;

    Paddle leftPaddle;
    Paddle rightPaddle;

    int leftScore;
    int rightScore;

    SpriteFont font;

    List<CollisionEffect> collisionEffects;

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
        ball = new Ball(new Vector2(ScreenWidth, ScreenHeight)/2, new Vector2(0, 0), 40);
        ball.TargetVelocity = new Vector2(0.3f, 0.15f);
        ball.TargetRadius = 23f;
        leftPaddle = new Paddle(new Vector2(30, ScreenHeight/2), Side.Left);
        rightPaddle = new Paddle(new Vector2(ScreenWidth-30-Paddle.width, ScreenHeight/2), Side.Right);
        leftScore = 0;
        rightScore = 0;
        collisionEffects = new List<CollisionEffect>();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        playButtonShader = Content.Load<Effect>("playButtonShader");
        Ball.ballShader = Content.Load<Effect>("ballShader");
        Paddle.paddleShader = Content.Load<Effect>("paddleShader");
        font = Content.Load<SpriteFont>("font");
        CollisionEffect.collisionEffectShader = Content.Load<Effect>("collisionEffectShader");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        MouseState mouseState = Mouse.GetState();
        KeyboardState keyboard = Keyboard.GetState();
        Vector2 mouse = new Vector2(mouseState.X, mouseState.Y);

        if (playing)
        {
            for (int i=collisionEffects.Count-1; i>=0; i--)
                if (collisionEffects[i].Complete)
                {
                    collisionEffects.RemoveAt(i);
                }

            Side? side = ball.Update(gameTime, new Vector2(ScreenWidth, ScreenHeight), new Paddle[] { leftPaddle, rightPaddle }, TriggerCollision);
            if (side == Side.Right)
            {
                leftScore++;
                playing = false;
            }
            else if (side == Side.Left)
            {
                rightScore++;
                playing = false;
            }
            
            if (keyboard.IsKeyDown(Keys.W)) leftPaddle.MoveUp(0, gameTime);
            if (keyboard.IsKeyDown(Keys.S)) leftPaddle.MoveDown(ScreenHeight, gameTime);
            if (keyboard.IsKeyDown(Keys.Up)) rightPaddle.MoveUp(0, gameTime);
            if (keyboard.IsKeyDown(Keys.Down)) rightPaddle.MoveDown(ScreenHeight, gameTime);
        }
        if (!playing)
        {
            if (menuStartTime == DateTime.MinValue) menuStartTime = DateTime.Now;
            if (mouseState.LeftButton == ButtonState.Pressed && (mouse-PlayButtonPosition()).LengthSquared() <= PlayButtonRadius()*PlayButtonRadius())
            {
                playing = true;
                Restart();
            }
        }

        base.Update(gameTime);
    }

    void Restart()
    {
        // TODO: remove repetition in Initialize
        menuStartTime = DateTime.MinValue;
        ball = new Ball(new Vector2(ScreenWidth, ScreenHeight)/2, new Vector2(0, 0), 40);
        ball.TargetVelocity = new Vector2(0.3f, 0.15f);
        ball.TargetRadius = 23f;
        leftPaddle = new Paddle(new Vector2(30, ScreenHeight/2), Side.Left);
        rightPaddle = new Paddle(new Vector2(ScreenWidth-30-Paddle.width, ScreenHeight/2), Side.Right);
        leftScore = 0;
        rightScore = 0;
        collisionEffects = new List<CollisionEffect>();
    }

    protected override void Draw(GameTime gameTime)
    {
        Render.graphicsDevice = GraphicsDevice;
        Render.spriteBatch = _spriteBatch;
        GraphicsDevice.Clear(Color.Black);

        MouseState mouseState = Mouse.GetState();

        if (playing)
        {
            foreach (CollisionEffect effect in collisionEffects) effect.Draw();
            ball.Draw();
            leftPaddle.Draw();
            rightPaddle.Draw();
        }
        else
        {
            DrawPlayButton(gameTime, mouseState);
        }

        _spriteBatch.Begin();
        _spriteBatch.DrawString(font, leftScore.ToString(), new Vector2(15, 15), new Color(1, 1, 1, 0.8f));
        Vector2 size = font.MeasureString(rightScore.ToString());
        _spriteBatch.DrawString(font, rightScore.ToString(), new Vector2(_graphics.PreferredBackBufferWidth-15-size.X, 15), new Color(1, 1, 1, 0.8f));
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawPlayButton(GameTime gameTime, MouseState mouseState)
    {
        double time = Ease.Clamp01(Ease.InverseLerp((DateTime.Now - menuStartTime).TotalMilliseconds, 250, 750));
        Render.SetValue(playButtonShader, "Time", (float)gameTime.TotalGameTime.TotalMilliseconds);
        Render.SetValue(playButtonShader, "Mouse", new Vector2(mouseState.Position.X, _graphics.PreferredBackBufferHeight - mouseState.Position.Y));
        Render.Circle(PlayButtonPosition(), PlayButtonRadius(), playButtonShader, new int[] { 0 });
    }

    private float PlayButtonRadius()
    {
        double time = Ease.Clamp01(Ease.InverseLerp((DateTime.Now - menuStartTime).TotalMilliseconds, 250, 750));
        return (float)Ease.Lerp(Ease.Smoothstep(time), 0, 25);
    }

    private Vector2 PlayButtonPosition()
    {
        return new Vector2(ScreenWidth, ScreenHeight) / 2;
    }

    public void TriggerCollision(Vector2 location, float maxRadius, TimeSpan duration, Vector2 direction)
    {
        collisionEffects.Add(new CollisionEffect(location, maxRadius, duration, direction));
    }
}
