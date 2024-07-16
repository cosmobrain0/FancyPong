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
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        playButtonShader = Content.Load<Effect>("shader");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (playing)
        {
            
        }
        else
        {
            if (menuStartTime == DateTime.MinValue) menuStartTime = DateTime.Now;
        }


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Render.graphicsDevice = GraphicsDevice;
        Render.spriteBatch = _spriteBatch;
        GraphicsDevice.Clear(Color.Black);


        double time = Ease.Clamp01(Ease.InverseLerp((DateTime.Now-menuStartTime).TotalMilliseconds, 1000, 2000));
        Render.Circle(new Vector2(ScreenWidth, ScreenHeight)/2, (float)Ease.Lerp(Ease.Smoothstep(time), 0, 25), playButtonShader, new int[] { 0 });

        base.Draw(gameTime);
    }
}
