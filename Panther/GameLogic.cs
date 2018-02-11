using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;
using System;

public enum GameState
{
    Over,
    InPlay,
    HighScore,
    MainMenu
};

namespace Panther
{
    class GameLogic : GameComponent
    {
        Camera CameraRef;
        List<Cube> TheBoxs;
        Terrain TheTerrain;
        Numbers ScoreDisplay;
        Letters WordDisplay;
        Effect TerrainEffect;
        float Rotation;

        GameState GameMode = GameState.InPlay;
        KeyboardState OldKeyState;

        public GameState CurrentMode { get => GameMode; }

        public GameLogic(Game game, Camera camera) : base(game)
        {
            CameraRef = camera;
            TheBoxs = new List<Cube>();
            ScoreDisplay = new Numbers(game);
            WordDisplay = new Letters(game);

            // Screen resolution is 1200 X 900.
            // Y positive is Up.
            // X positive is right of window when camera is at rotation zero.
            // Z positive is towards the camera when at rotation zero.
            // Positive rotation rotates CCW. Zero has front facing X positive. Pi/2 on Y faces Z negative.
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            base.Initialize();

        }

        public void LoadContent()
        {
            TerrainEffect = Game.Content.Load<Effect>("Effects/Terrain");

        }

        public void BeginRun()
        {
            TheTerrain = new Terrain(Game, CameraRef, TerrainEffect,
                "heightmap_01", "Grass", "Rocky", "Snowy", 32, 5, 25);
            Cube box = new Cube(Game, CameraRef);

            for (int i = 0; i < 3; i++)
            {
                TheBoxs.Add(new Cube(Game, CameraRef));
            }

            TheBoxs[0].ModelScale = new Vector3(20);
            TheBoxs[0].Position = new Vector3(0, 50, 0);
            TheBoxs[0].RotationVelocity = new Vector3(0, 0, 1);
            TheBoxs[0].DefuseColor = new Vector3(0.5f, 0.4f, 0.1f);
            TheBoxs[0].EmissiveColor = new Vector3(0.5f, 0.4f, 0.1f);
            TheBoxs[1].ModelScale = new Vector3(2);
            TheBoxs[1].Position = new Vector3(150, 0, 0);
            TheBoxs[1].RotationVelocity = new Vector3(0, 0, 2);
            TheBoxs[1].DefuseColor = new Vector3(0.2f, 0.2f, 0.6f);
            TheBoxs[1].AddAsChildOf(TheBoxs[0]);
            TheBoxs[2].ModelScale = new Vector3(1);
            TheBoxs[2].Position = new Vector3(30, 0, 0);
            TheBoxs[2].RotationVelocity = new Vector3(0, 0, 2);
            TheBoxs[2].AddAsChildOf(TheBoxs[1]);
            TheBoxs[2].DefuseColor = new Vector3(0.6f, 0.6f, 0.6f);

            ScoreDisplay.Setup(new Vector2(0, 200), 1);
            ScoreDisplay.SetNumber(100);
            WordDisplay.Setup(Vector2.Zero, 1);
            WordDisplay.SetWords("This is a TEST");
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState KBS = Keyboard.GetState();

            if (KBS != OldKeyState)
            {
                if (KBS.IsKeyDown(Keys.Space))
                {
                    TheBoxs[0].Enabled = !TheBoxs[0].Enabled;
                }
            }

            if (KBS.IsKeyDown(Keys.Left))
            {
                TheBoxs[0].PO.Velocity.X = -10;
            }
            else if (KBS.IsKeyDown(Keys.Right))
            {
                TheBoxs[0].PO.Velocity.X = 10;
            }
            else
            {
                TheBoxs[0].PO.Velocity.X = 0;
            }

            OldKeyState = Keyboard.GetState();

            Rotation += MathHelper.Pi * 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }
    }
}
