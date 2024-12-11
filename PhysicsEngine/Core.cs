using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Physics;
using Physics.Particles;
using Types;

namespace Core
{
    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Engine _engine = new Engine(); // The physics Engine

        Texture2D whiteTexture;
        private BasicEffect basicEffect; // BasicEffect for drawing polygons

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private int maxVertices = 1000; // Adjust based on expected maximum vertices
        private int maxIndices = 3000;  // Adjust based on expected maximum indices

        private SpriteFont font; // Font for displaying the framerate
        private double frameRate;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set the target framerate to 144 FPS
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 144.0);
        }

        protected override void Initialize()
        {
            // Initialize the engine
            _engine.Initialize();

            // Set screen width & height
            _graphics.PreferredBackBufferWidth = 1000;
            _graphics.PreferredBackBufferHeight = 800;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _engine.LoadContent();

            whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            whiteTexture.SetData([Color.White]);

            // Initialize BasicEffect
            basicEffect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1)
            };

            // Initialize vertex and index buffers
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), maxVertices, BufferUsage.WriteOnly);
            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, maxIndices, BufferUsage.WriteOnly);

            // Load the font
            font = Content.Load<SpriteFont>("Arial"); // Ensure you have a SpriteFont named "Arial" in your Content
        }

        protected override void Update(GameTime gameTime)
        {
            // Exit the game if the back button or escape key is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Update the physics engine
            _engine.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            // Calculate the framerate
            frameRate = 1 / gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        private void ResizeBuffers(int requiredVertices, int requiredIndices)
        {
            if (requiredVertices > maxVertices)
            {
                maxVertices = Math.Max(requiredVertices, maxVertices * 2);
                if (vertexBuffer != null) vertexBuffer.Dispose();
                vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), maxVertices, BufferUsage.WriteOnly);
            }

            if (requiredIndices > maxIndices)
            {
                maxIndices = Math.Max(requiredIndices, maxIndices * 2);
                if (indexBuffer != null) indexBuffer.Dispose();
                indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, maxIndices, BufferUsage.WriteOnly);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            List<VertexPositionColor> vertexList = new List<VertexPositionColor>();
            List<int> indexList = new List<int>();

            // Collect vertices and indices for all particles
            foreach (Particle particle in _engine.Particles)
            {
                try
                {
                    AddPolygonVertices(particle, vertexList, indexList, Color.White);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while drawing the polygon: {ex.Message}");
                }
            }

            // Update buffers and draw polygons if there are any vertices and indices
            if (vertexList.Count > 0 && indexList.Count > 0)
            {
                // Ensure buffers are large enough
                ResizeBuffers(vertexList.Count, indexList.Count);

                try
                {
                    vertexBuffer.SetData(vertexList.ToArray());
                    indexBuffer.SetData(indexList.ToArray());

                    GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    GraphicsDevice.Indices = indexBuffer;

                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexList.Count / 3);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Buffer error: {ex.Message}");
                }
            }

            // Draw the outlines of the polygons
            _spriteBatch.Begin();
            foreach (Particle particle in _engine.Particles)
            {
                Vect[] transformedVerts = particle.polygon.TransformedVertices();
                for (int i = 0; i < transformedVerts.Length; i++)
                {
                    Vector2 start = transformedVerts[i];
                    Vector2 end = transformedVerts[(i + 1) % transformedVerts.Length];
                    DrawLine(_spriteBatch, whiteTexture, start, end, Color.Red);
                }
            }

            // Draw the framerate
            _spriteBatch.DrawString(font, $"FPS: {frameRate:F2}", new Vector2(10, 10), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        // Add vertices and indices for a polygon to the lists
        private void AddPolygonVertices(Particle particle, List<VertexPositionColor> vertexList, List<int> indexList, Color fillColor)
        {
            Vect[] vertices = particle.polygon.TransformedVertices();
            int baseIndex = vertexList.Count;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertexList.Add(new VertexPositionColor(new Vector3(vertices[i], 0), fillColor));
            }

            for (int i = 0; i < vertices.Length - 2; i++)
            {
                indexList.Add(baseIndex);
                indexList.Add(baseIndex + i + 1);
                indexList.Add(baseIndex + i + 2);
            }
        }

        // Draw a line between two points
        private static void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color)
        {
            if (spriteBatch == null) throw new ArgumentNullException(nameof(spriteBatch));
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            spriteBatch.Draw(texture, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 1), null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
