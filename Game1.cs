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
    const int ScreenWidth = 1800;
    const int ScreenHeight = 900;
    public static Matrix transformationMatrix = new Matrix(ScreenWidth/LogicalScreenWidth, 0, 0, 0, 0, ScreenHeight/LogicalScreenHeight, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

    Effect playButtonShader;
    Effect blurIceShader;

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
    List<(Vector2, DateTime)> iceEffectSnowflakes;

    Effect dashedLineShader;
    const float dashedLineWidth = 5;
    const float dashedLineHeight = 15;
    const float dashedLineGap = 10;

    List<PowerBox> powerBoxes;

    DateTime lastPowerBoxSpawn = DateTime.MinValue;
    readonly TimeSpan timeBetweenSpawns = TimeSpan.FromMilliseconds(7000);
    Random generator;

    DateTime speedBoostStart = DateTime.MinValue;
    readonly TimeSpan speedBoostDuration = TimeSpan.FromMilliseconds(5000);
    const float ballNormalSpeed = 0.5f;
    const float ballSpeedBoostSpeed = 0.7f;

    DateTime paddleIceStart = DateTime.MinValue;
    readonly TimeSpan paddleIceDuration = TimeSpan.FromMilliseconds(1000);

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
        ball.TargetVelocity = new Vector2((float)Math.Cos(Math.PI/4f), (float)Math.Sin(Math.PI/4f)) * ballNormalSpeed;
        ball.TargetRadius = 16f;
        leftPaddle = new Paddle(new Vector2(30, LogicalScreenHeight/2), Side.Left);
        rightPaddle = new Paddle(new Vector2(LogicalScreenWidth-30-Paddle.width, LogicalScreenHeight/2), Side.Right);
        leftScore = 0;
        rightScore = 0;
        collisionEffects = new List<CollisionEffect>();
        particles = new List<Particle>();
        PowerBox.triggerSpeed = () => {
            if (speedBoostStart == DateTime.MinValue)
            {
                ball.TargetSpeed = ballSpeedBoostSpeed;
            }
            speedBoostStart = DateTime.Now;
        };
        PowerBox.triggerIce = () => {
            paddleIceStart = DateTime.Now;
        };
        powerBoxes = new List<PowerBox>();
        generator = new Random((int) (DateTime.Now.Ticks & 0b11111111111111111111111111111111));
        iceEffectSnowflakes = new List<(Vector2, DateTime)>();
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
        dashedLineShader = Content.Load<Effect>("dashedLineShader");
        PowerBox.speedShader = Content.Load<Effect>("powerSpeedShader");
        PowerBox.iceShader = Content.Load<Effect>("powerIceShader");
        blurIceShader = Content.Load<Effect>("blurIceShader");
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
            bool iced = paddleIceStart + paddleIceDuration > DateTime.Now;

            for (int i=collisionEffects.Count-1; i>=0; i--)
                if (collisionEffects[i].Complete)
                {
                    collisionEffects.RemoveAt(i);
                }

            for (int i=powerBoxes.Count-1; i>=0; i--)
            {
                if (powerBoxes[i].Intersects(ball.Centre, ball.Radius))
                {
                    powerBoxes[i].Trigger();
                    powerBoxes.RemoveAt(i);
                }
            }

            if (speedBoostStart + speedBoostDuration <= DateTime.Now && speedBoostStart != DateTime.MinValue)
            {
                speedBoostStart = DateTime.MinValue;
                ball.TargetSpeed = ballNormalSpeed;
            }

            if (lastPowerBoxSpawn + timeBetweenSpawns <= DateTime.Now)
            {
                lastPowerBoxSpawn += timeBetweenSpawns;

                PowerBoxType boxType = generator.NextInt64()%2 == 0 ? PowerBoxType.Speed : PowerBoxType.Ice;

                Vector2 location;
                float spaceAroundBall = ball.Radius + ball.TargetSpeed*100f;
                int iterations = 0;
                do {
                    location = new Vector2(
                        generator.NextSingle()*(LogicalScreenWidth-200-PowerBox.sideLength) + 100,
                        generator.NextSingle()*(LogicalScreenHeight - PowerBox.sideLength)
                    );
                    iterations++;
                } while (
                    new PowerBox(location, boxType).Intersects(ball.Centre, ball.Radius) ||
                    ((location-ball.Centre).LengthSquared() <= spaceAroundBall*spaceAroundBall &&
                        Vector2.Dot((location-ball.Centre), ball.TargetVelocity) >= 0)
                );
                if (iterations > 100) Console.WriteLine(iterations);
                powerBoxes.Add(new PowerBox(location, boxType));
            }

            particles.AsParallel().ForAll(x => x.Update(gameTime, new Vector2(LogicalScreenWidth, LogicalScreenHeight)));
            particles = particles.AsParallel().Where(x => !x.Dead).ToList();

            Side? side = ball.Update(gameTime, new Vector2(LogicalScreenWidth, LogicalScreenHeight), new Paddle[] { leftPaddle, rightPaddle }, speedBoostStart != DateTime.MinValue, TriggerCollision, SpawnParticle);
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
           
            if (!iced)
            {
                if (keyboard.IsKeyDown(Keys.W)) leftPaddle.MoveUp(0, gameTime);
                if (keyboard.IsKeyDown(Keys.S)) leftPaddle.MoveDown(LogicalScreenHeight, gameTime);
                if (keyboard.IsKeyDown(Keys.Up)) rightPaddle.MoveUp(0, gameTime);
                if (keyboard.IsKeyDown(Keys.Down)) rightPaddle.MoveDown(LogicalScreenHeight, gameTime);
            }
            else
            {
                if (generator.NextSingle() <= 0.4)
                {
                    Vector2 leftPosition = leftPaddle.Position + new Vector2(Paddle.width*(generator.NextSingle()-0.5f)*2, Paddle.height*(generator.NextSingle() - 0.5f));
                    Vector2 rightPosition = rightPaddle.Position + new Vector2(Paddle.width*(generator.NextSingle()-0.5f)*2, Paddle.height*(generator.NextSingle() - 0.5f));
                    iceEffectSnowflakes.Add((leftPosition, DateTime.Now));
                    iceEffectSnowflakes.Add((rightPosition, DateTime.Now));
                }
            }
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
        ball.TargetRadius = 16f;
        leftPaddle = new Paddle(new Vector2(30, LogicalScreenHeight/2), Side.Left);
        rightPaddle = new Paddle(new Vector2(LogicalScreenWidth-30-Paddle.width, LogicalScreenHeight/2), Side.Right);
        collisionEffects = new List<CollisionEffect>();
        particles = new List<Particle>();
        powerBoxes = new List<PowerBox>();
        lastPowerBoxSpawn = DateTime.Now;
        speedBoostStart = DateTime.MinValue;
    }

    protected override void Draw(GameTime gameTime)
    {
        Render.graphicsDevice = GraphicsDevice;
        Render.spriteBatch = _spriteBatch;
        Render.scale = ScreenWidth/LogicalScreenWidth;
        bool iced = playing && paddleIceStart + paddleIceDuration > DateTime.Now;
        // GraphicsDevice.Clear(iced ? new Color(2, 21, 74) : Color.Black);
        GraphicsDevice.Clear(Color.Black);

        MouseState mouseState = Mouse.GetState();

        if (playing)
        {
            DrawDashedLine(gameTime);
            foreach (CollisionEffect effect in collisionEffects) effect.Draw();
            foreach (Particle particle in particles) particle.Draw();
            foreach (PowerBox powerBox in powerBoxes) powerBox.Draw(gameTime);
            ball.Draw(speedBoostDuration, speedBoostStart == DateTime.MinValue ? TimeSpan.Zero : DateTime.Now-speedBoostStart, gameTime);
            leftPaddle.Draw();
            rightPaddle.Draw();
            for (int i=iceEffectSnowflakes.Count-1; i>=0; i--)
            {
                float t = (float)Ease.Clamp01(Ease.InverseLerp((float)(DateTime.Now - iceEffectSnowflakes[i].Item2).TotalMilliseconds, 0, 1000));
                if (t >= 1)
                    iceEffectSnowflakes.RemoveAt(i);
                else
                    DrawSnowflake(iceEffectSnowflakes[i].Item1, iceEffectSnowflakes[i].Item2, t);
            }
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

    private void DrawSnowflake(Vector2 position, DateTime startTime, float t)
    {
        Render.SetValue(blurIceShader, "Progress", t);
        Render.Rectangle(position-Vector2.One*40, new Vector2(80, 80), blurIceShader, new int[] { 0 });
    }

    private void DrawPlayButton(GameTime gameTime, MouseState mouseState)
    {
        double time = Ease.Clamp01(Ease.InverseLerp((DateTime.Now - menuStartTime).TotalMilliseconds, 250, 750));
        Render.SetValue(playButtonShader, "Time", (float)gameTime.TotalGameTime.TotalMilliseconds);
        Render.SetValue(playButtonShader, "Mouse", new Vector2(mouseState.Position.X, _graphics.PreferredBackBufferHeight - mouseState.Position.Y));
        Render.Circle(PlayButtonPosition(), PlayButtonRadius(), playButtonShader, new int[] { 0 });
    }

    private void DrawDashedLine(GameTime gameTime)
    {
        Render.SetValue(dashedLineShader, "LineGap", dashedLineGap);
        Render.SetValue(dashedLineShader, "LineHeight", dashedLineHeight);
        Render.SetValue(dashedLineShader, "Time", (float)gameTime.TotalGameTime.TotalMilliseconds);
        Render.Rectangle(new Vector2(LogicalScreenWidth/2f - dashedLineWidth/2f, 0), new Vector2(dashedLineWidth, LogicalScreenHeight), dashedLineShader, new int[] { 0 });
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
