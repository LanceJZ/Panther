using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;

namespace Panther
{
    class PlaneEntity : DrawableGameComponent
    {
        PositionedObject PO;
        Camera TheCamera;
        VertexPositionTexture[] Verts = new VertexPositionTexture[6];
        VertexBuffer PlaneVertexBuffer;
        Texture2D XNATexture;
        Matrix BaseWorld;
        BasicEffect PlaneBasicEffect;

        public float Width;
        public float Height;

        public PlaneEntity(Game game, Camera camera) : base(game)
        {
            PO = new PositionedObject(game);
            TheCamera = camera;
            PlaneBasicEffect = new BasicEffect(game.GraphicsDevice);
            game.Components.Add(this);

        }

        public PlaneEntity(Game game, Camera camera, Texture2D texture) : base(game)
        {
            PO = new PositionedObject(game);
            TheCamera = camera;
            PlaneBasicEffect = new BasicEffect(game.GraphicsDevice);
            XNATexture = texture;
            game.Components.Add(this);
        }

        public PlaneEntity(Game game, Camera camera, Texture2D texture, Vector3 position) : base(game)
        {
            PO = new PositionedObject(game);
            TheCamera = camera;
            PlaneBasicEffect = new BasicEffect(game.GraphicsDevice);
            Create(texture);
            PO.Position = position;
            game.Components.Add(this);
        }

        public PlaneEntity(Game game, Camera camera, Texture2D texture,
            Vector3 position, Vector3 rotation) : base(game)
        {
            PO = new PositionedObject(game);
            TheCamera = camera;
            PlaneBasicEffect = new BasicEffect(game.GraphicsDevice);
            Create(texture);
            PO.Position = position;
            PO.Rotation = rotation;
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadContent();
            BegenRun();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public virtual void BegenRun()
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            BaseWorld = Matrix.CreateScale(PO.Scale) * RotateMatrix(PO.Rotation) *
                Matrix.CreateTranslation(PO.Position);

            if (PO.Child)
            {
                foreach (PositionedObject parentPO in PO.ParentPOs)
                {
                    BaseWorld *= Matrix.CreateTranslation(parentPO.Position)
                     * RotateMatrix(parentPO.Rotation)
                     * Matrix.CreateTranslation(parentPO.Position);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // Set object and camera info
            PlaneBasicEffect.World = BaseWorld;
            PlaneBasicEffect.View = TheCamera.View;
            PlaneBasicEffect.Projection = TheCamera.Projection;
            Game.GraphicsDevice.SetVertexBuffer(PlaneVertexBuffer);
            // Begin effect and draw for each frame
            foreach (EffectPass pass in PlaneBasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game.GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(
                    PrimitiveType.TriangleList, Verts, 0, 2);
            }
        }

        public Matrix RotateMatrix(Vector3 rotation)
        {
            return Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }

        public void Create(Texture2D texture)
        {
            PlaneVertexBuffer = new VertexBuffer(Game.GraphicsDevice,
                typeof(VertexPositionTexture),
                Verts.Length, BufferUsage.None);

            ChangePlaneTexture(texture);
        }

        public void ChangePlaneTexture(Texture2D texture)
        {
            XNATexture = texture;
            PlaneBasicEffect.Texture = texture;
            PlaneBasicEffect.TextureEnabled = true;

            if (texture != null)
                ChangePlaneSize(texture.Width, texture.Height);
        }

        public void ChangePlaneSize(float Width, float Height)
        {
            SetupPlaneVertexBuffer(Width, Height);
        }

        public Texture2D Load(string textureName)
        {
            return Helper.LoadTexture(textureName);
        }

        void SetupPlaneVertexBuffer(float width, float height)
        {
            // Setup plane
            Width = width;
            Height = height;

            Verts[0] = new VertexPositionTexture(new Vector3(-width / 2, -height / 2, 0), new Vector2(0, 0));
            Verts[1] = new VertexPositionTexture(new Vector3(-width / 2, height / 2, 0), new Vector2(0, 1));
            Verts[2] = new VertexPositionTexture(new Vector3(width / 2, -height / 2, 0), new Vector2(1, 0));
            Verts[3] = new VertexPositionTexture(new Vector3(-width / 2, height / 2, 0), new Vector2(0, 1));
            Verts[4] = new VertexPositionTexture(new Vector3(width / 2, height / 2, 0), new Vector2(1, 1));
            Verts[5] = new VertexPositionTexture(new Vector3(width / 2, -height / 2, 0), new Vector2(1, 0));

            PlaneVertexBuffer.SetData(Verts);
        }
    }
}
