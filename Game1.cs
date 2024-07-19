using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Render2D;
using Easing;
using System.Collections.Generic;
using System.Linq;

namespace FancyPong;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // NOTE: the logical aspect ratio and the screen aspect ratio *must* be the same
    const float LogicalScreenWidth = 1200;
    const float LogicalScreenHeight = 600;
    const int ScreenWidth = 1600;
    const int ScreenHeight = 800;
    public static Matrix transformationMatrix = new Matrix(ScreenWidth/LogicalScreenWidth, 0, 0, 0, 0, ScreenHeight/LogicalScreenHeight, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

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
    List<Particle> particles;

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
        ball = new Ball(new Vector2(LogicalScreenWidth, LogicalScreenHeight)/2, new Vector2(0.03f, 0.015f), 40);
        ball.TargetVelocity = new Vector2(0.3f, 0.15f);
        ball.TargetRadius = 8f;
        leftPaddle = new Paddle(new Vector2(30, LogicalScreenHeight/2), Side.Left);
        rightPaddle = new Paddle(new Vector2(LogicalScreenWidth-30-Paddle.width, LogicalScreenHeight/2), Side.Right);
        leftScore = 0;
        rightScore = 0;
        collisionEffects = new List<CollisionEffect>();
        particles = new List<Particle>();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        playButtonShader = Content.Load<Effect>("playButtonShader");
        Ball.ballShader = Content.Load<Effect>("ballShader");
        Paddle.paddleShader = Content.Load<Effect>("paddleShader");
        font = Content.Load<SpriteFont>("font");
        CollisionEffect.collisionEffectShader = Content.Load<Effect>("collisionEffectShader");
        Particle.particleShader = Content.Load<Effect>("particleShader");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        MouseState mouseState = Mouse.GetState();
        KeyboardState keyboard = Keyboard.GetState();
        Vector2 mouse = new Vector2(mouseState.X * LogicalScreenWidth/ScreenWidth, mouseState.Y * LogicalScreenHeight/ScreenHeight);

        if (playing)
        {
            for (int i=collisionEffects.Count-1; i>=0; i--)
                if (collisionEffects[i].Complete)
                {
                    collisionEffects.RemoveAt(i);
                }

            particles.AsParallel().ForAll(x => x.Update(gameTime, new Vector2(LogicalScreenWidth, LogicalScreenHeight)));
            particles = particles.AsParallel().Where(x => !x.Dead).ToList();

            Side? side = ball.Update(gameTime, new Vector2(LogicalScreenWidth, LogicalScreenHeight), new Paddle[] { leftPaddle, rightPaddle }, TriggerCollision, SpawnParticle);
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
            if (keyboard.IsKeyDown(Keys.S)) leftPaddle.MoveDown(LogicalScreenHeight, gameTime);
            if (keyboard.IsKeyDown(Keys.Up)) rightPaddle.MoveUp(0, gameTime);
            if (keyboard.IsKeyDown(Keys.Down)) rightPaddle.MoveDown(LogicalScreenHeight, gameTime);
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
        ball = new Ball(new Vector2(LogicalScreenWidth, LogicalScreenHeight)/2, new Vector2(0.03f, 0.015f), 40);
        ball.TargetVelocity = new Vector2(0.3f, 0.15f);
        ball.TargetRadius = 8f;
        leftPaddle = new Paddle(new Vector2(30, LogicalScreenHeight/2), Side.Left);
        rightPaddle = new Paddle(new Vector2(LogicalScreenWidth-30-Paddle.width, LogicalScreenHeight/2), Side.Right);
        leftScore = 0;
        rightScore = 0;
        collisionEffects = new List<CollisionEffect>();
        particles = new List<Particle>();
    }

    protected override void Draw(GameTime gameTime)
    {
        Render.graphicsDevice = GraphicsDevice;
        Render.spriteBatch = _spriteBatch;
        Render.scale = ScreenWidth/LogicalScreenWidth;
        GraphicsDevice.Clear(Color.Black);

        MouseState mouseState = Mouse.GetState();

        if (playing)
        {
            foreach (CollisionEffect effect in collisionEffects) effect.Draw();
            foreach (Particle particle in particles) particle.Draw();
            ball.Draw();
            leftPaddle.Draw();
            rightPaddle.Draw();
        }
        else
        {
            DrawPlayButton(gameTime, mouseState);
        }

        _spriteBatch.Begin(transformMatrix: transformationMatrix);
        _spriteBatch.DrawString(font, leftScore.ToString(), new Vector2(15, 15), new Color(1, 1, 1, 0.8f));
        Vector2 size = font.MeasureString(rightScore.ToString());
        _spriteBatch.DrawString(font, rightScore.ToString(), new Vector2(LogicalScreenWidth-15-size.X, 15), new Color(1, 1, 1, 0.8f));
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
        return new Vector2(LogicalScreenWidth, LogicalScreenHeight) / 2;
    }

    public void TriggerCollision(Vector2 location, float maxRadius, TimeSpan duration, Vector2 direction)
    {
        collisionEffects.Add(new CollisionEffect(location, maxRadius, duration, direction));
    }

    public void SpawnParticle(Particle particle)
    {
        particles.Add(particle);
    }
}
