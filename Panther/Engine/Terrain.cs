using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Panther
{
    class Terrain : DrawableGameComponent
    {
        #region Fields
        VertexBuffer TheVBuffer;
        IndexBuffer TheIBuffer;
        GraphicsDevice R_Graphics;
        Camera R_Camera;
        Effect R_Effect;
        Texture2D HeightMap;
        Texture2D TheTexture1;
        Texture2D TheTexture2;
        Texture2D TheTexture3;
        Vector3 LightDirection;
        Vector4 LightColor;
        Vector4 AmbiantLightColor;
        float AmbientLightLevel;
        float LightBrightness;
        float TextureScale;
        float MaxHeight;
        float HeightScale;
        float[,] Heights;
        int Width, Height;
        #endregion
        #region Constructor
        public Terrain(Game game, Camera camera, Effect effect,
            float textureScale, int width, int height, float heightScale) : base(game)
        {
            R_Graphics = Helper.Graphics;
            R_Camera = camera;
            R_Effect = effect;
            TextureScale = textureScale;
            Width = width;
            Height = height;
            HeightScale = heightScale;
            MaxHeight = heightScale;

            game.Components.Add(this);
        }
        #endregion
        public override void Initialize()
        {
            LightDirection = new Vector3(-1, 1, -1);
            LightDirection.Normalize();
            LightColor = new Vector4(1, 1, 1, 1);
            LightBrightness = 0.8f;
            AmbiantLightColor = new Vector4(1, 1, 1, 1);
            AmbientLightLevel = 0.15f;

            base.Initialize();
            LoadContent();
            BeginRun();
        }

        protected override void LoadContent()
        {
            TheTexture1 = Helper.LoadTexture("Grass");
            TheTexture2 = Helper.LoadTexture("Rocky");
            TheTexture3 = Helper.LoadTexture("Snowy");
            HeightMap = Helper.LoadTexture("heightmap_flat");
        }

        public void BeginRun()
        {
            ReadHeightMap(HeightMap, Width, Height, HeightScale);
            BuildVertexBuffer(Width, Height);
            BuildIndexBuffer(Width, Height);
            CalculateNormals();
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }
        #region Draw
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            R_Effect.CurrentTechnique = R_Effect.Techniques["Technique1"];
            R_Effect.Parameters["World"].SetValue(Matrix.Identity);
            R_Effect.Parameters["View"].SetValue(R_Camera.View);
            R_Effect.Parameters["Projection"].SetValue(R_Camera.Projection);
            R_Effect.Parameters["lightDirection"].SetValue(LightDirection);
            R_Effect.Parameters["lightColor"].SetValue(LightColor);
            R_Effect.Parameters["lightBrightness"].SetValue(LightBrightness);
            R_Effect.Parameters["ambientLightColor"].SetValue(AmbiantLightColor);
            R_Effect.Parameters["ambientLightLevel"].SetValue(AmbientLightLevel);
            R_Effect.Parameters["terrainTexture1"].SetValue(TheTexture1);
            R_Effect.Parameters["terrainTexture2"].SetValue(TheTexture2);
            R_Effect.Parameters["terrainTexture3"].SetValue(TheTexture3);
            R_Effect.Parameters["maxElevation"].SetValue(MaxHeight);

            foreach (EffectPass pass in R_Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                R_Graphics.SetVertexBuffer(TheVBuffer);
                R_Graphics.Indices = TheIBuffer;
                R_Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    TheIBuffer.IndexCount / 3);
            }

            base.Draw(gameTime);
        }
        #endregion
        #region HeightMap
        void ReadHeightMap(Texture2D heightMap, int width, int height, float heightScale)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            Heights = new float[width, height];
            Color[] heightMapData = new Color[heightMap.Width * heightMap.Height];
            heightMap.GetData(heightMapData);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    byte heightData = heightMapData[x + z * width].R;
                    Heights[x, z] = heightData / 255f;
                    max = MathHelper.Max(max, Heights[x, z]);
                    min = MathHelper.Min(min, Heights[x, z]);
                }
            }

            float range = (max - min);

            for(int x = 0; x < width; x++)
            {
                for(int z = 0; z < height; z++)
                {
                    Heights[x, z] = ((Heights[x, z] - min) / range) * heightScale;
                }
            }
        }
        #endregion
        #region Vertex Buffer
        void BuildVertexBuffer(int width, int height)
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    vertices[x + (z * width)].Position = new Vector3(x - (width / 2), Heights[x, z], z - (height / 2));
                    vertices[x + (z * width)].TextureCoordinate =
                        new Vector2(x / TextureScale, z / TextureScale);
                }
            }

            TheVBuffer = new VertexBuffer(R_Graphics, typeof(VertexPositionNormalTexture),
                vertices.Length, BufferUsage.None);
            TheVBuffer.SetData(vertices);
        }
        #endregion
        #region Index Buffer
        void BuildIndexBuffer(int width, int height)
        {
            int indexCount = (width - 1) * (height - 1) * 6;
            short[] indices = new short[indexCount];
            int counter = 0;

            for (short z = 0; z < height - 1; z++)
            {
                for (short x = 0; x < width - 1; x++)
                {
                    short upperLeft = (short)(x + (z * width));
                    short upperRight = (short)(upperLeft + 1);
                    short lowerLeft = (short)(upperLeft + width);
                    short lowerRight = (short)(upperLeft + width + 1);
                    indices[counter++] = upperLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;
                    indices[counter++] = upperLeft;
                    indices[counter++] = upperRight;
                    indices[counter++] = lowerRight;
                }
            }

            TheIBuffer = new IndexBuffer(R_Graphics, IndexElementSize.SixteenBits,
                indices.Length, BufferUsage.None);
            TheIBuffer.SetData(indices);
        }
        #endregion
        #region Helper Methods
        void CalculateNormals()
        {
            VertexPositionNormalTexture[] vertices =
                new VertexPositionNormalTexture[TheVBuffer.VertexCount];
            short[] indices = new short[TheIBuffer.IndexCount];
            TheVBuffer.GetData(vertices);
            TheIBuffer.GetData(indices);

            for (int i = 0; i < vertices.Length / 3; i++)
            {
                vertices[i].Normal = Vector3.Zero;
            }

            int triangleCount = indices.Length / 3;

            for (int i = 0; i < triangleCount; i++)
            {
                int v1 = indices[i * 3];
                int v2 = indices[(i * 3) + 1];
                int v3 = indices[(i * 3) + 2];
                Vector3 firstSide = vertices[v2].Position - vertices[v1].Position;
                Vector3 secondSide = vertices[v1].Position - vertices[v3].Position;
                Vector3 triangleNormal = Vector3.Cross(firstSide, secondSide);
                triangleNormal.Normalize();
                vertices[v1].Normal += triangleNormal;
                vertices[v2].Normal += triangleNormal;
                vertices[v3].Normal += triangleNormal;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal.Normalize();
            }

            TheVBuffer.SetData(vertices);
        }

        public float GetHeight(float x, float z)
        {
            int xmin = (int)Math.Floor(x);
            int xmax = xmin + 1;
            int zmin = (int)Math.Floor(z);
            int zmax = zmin + 1;

            if (xmin < 0 || zmin < 0 ||
                xmax > Heights.GetUpperBound(0) || zmax > Heights.GetUpperBound(1))
            {
                return 0;
            }

            Vector3 p1 = new Vector3(xmin, Heights[xmin, zmax], zmax);
            Vector3 p2 = new Vector3(xmax, Heights[xmax, zmin], zmin);
            Vector3 p3;

            if ((x - xmin) + (z - zmin) <= 1)
            {
                p3 = new Vector3(xmin, Heights[xmin, zmin], zmin);
            }
            else
            {
                p3 = new Vector3(xmax, Heights[xmax, zmax], zmax);
            }

            Plane plane = new Plane(p1, p2, p3);
            Ray ray = new Ray(new Vector3(x, 0, z), Vector3.Up);
            float? height = ray.Intersects(plane);

            return height.HasValue ? height.Value : 0f;
        }
        #endregion
    }
}
