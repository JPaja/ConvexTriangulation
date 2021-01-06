using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Triangulation.Models;
using Triangulation.Utils;
using Xssp.MonoGame.Primitives2D;
using IDrawable = Triangulation.Models.IDrawable;

namespace Triangulation
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private MouseState _oldMouseState;
        private KeyboardState _oldKeyboardState;

        private List<Dot> _points = new();
        private List<Polygon> _polygons = new();
        private int solutionFrame = 0;
        private List<(IDrawable,bool Remove)> _solution = new ();
        private Random _random = new Random();
        
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        
        protected override void Initialize()
        {
            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            base.Initialize();
            Window.Title = "C - Clear | R - Random | T - Triangulate | S - Save | L - Load";
        }
        
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _oldMouseState = Mouse.GetState();

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (mouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
                AddPoint(mouseState);

            if (keyboardState.IsKeyDown(Keys.C) && _oldKeyboardState.IsKeyUp(Keys.C))
                ClearPoints();

            if (keyboardState.IsKeyDown(Keys.R) && _oldKeyboardState.IsKeyUp(Keys.R))
                RandomizePoints();
            
            if (keyboardState.IsKeyDown(Keys.T) && _oldKeyboardState.IsKeyUp(Keys.T))
            {
                if (_points.Count != 0)
                {
                    solutionFrame = 0;
                    _solution.Clear();
                    var hull = PolygonUtils.Hull(_points,_solution,false);
                    _polygons.Clear();
                    _polygons.Add(hull);
                    _points.Clear();
                }
                else
                {
                    solutionFrame = 0;
                }

            }
 
            if (keyboardState.IsKeyDown(Keys.L) && _oldKeyboardState.IsKeyUp(Keys.L))
            {
                //TODO: Load
            }
            if (keyboardState.IsKeyDown(Keys.S) && _oldKeyboardState.IsKeyUp(Keys.S))
            { 
                //TODO: Save
            }
            if (keyboardState.IsKeyDown(Keys.E) && _oldKeyboardState.IsKeyUp(Keys.E))
            {
               
                solutionFrame = _solution.Count;

            }
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                if(solutionFrame != 0)
                    solutionFrame--;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                if(_solution.Count > solutionFrame)
                    solutionFrame++;
            }
                
            _oldMouseState = mouseState;
            _oldKeyboardState = keyboardState;

            if (!keyboardState.IsKeyDown(Keys.Space))
            {
                if (_solution.Count > solutionFrame && gameTime.TotalGameTime.Ticks % 10 == 0)
                {
                    /*while (solutionFrame <_solution.Count &&_solution[solutionFrame++].Remove)
                    {
                    }*/
                    solutionFrame++;
                }
            }


            base.Update(gameTime);
        }

        private void ClearPoints()
        {
            _polygons.Clear();
            _solution.Clear();
            solutionFrame = 0;
            _points.Clear();
        }

        private void AddPoint(MouseState mouseState)
        {
            var point = new Dot(new Vector2(mouseState.X, mouseState.Y));
            _points.Add(point);
            _polygons.Clear();
            _solution.Clear();
            solutionFrame = 0;
        }

        private void RandomizePoints()
        {
            _polygons.Clear();
            _solution.Clear();
            _points.Clear();
            var length = _random.Next(20,40);
            //var length = 13;
            for (var i = 0; i < length; i++)
            {
                var x = _random.Next(GraphicsDevice.Viewport.Width) * 0.8f;
                var y = _random.Next(GraphicsDevice.Viewport.Height) * 0.8f;
                x += 0.1f * GraphicsDevice.Viewport.Width;
                y += 0.1f * GraphicsDevice.Viewport.Height;
                _points.Add(new Dot(x, y));
            }
        }
        
        
        List<IDrawable> FilterSolutions()
        {
            var result = new List<IDrawable>();
            var toRemove = new HashSet<IDrawable>();
            for (var i = Math.Min(solutionFrame, _solution.Count - 1); i >= 0; i--)
            {
                var (drawable, remove) = _solution[i];
                if(remove)
                    toRemove.Add(drawable);
                else if(!toRemove.Contains(drawable))
                    result.Insert(0,drawable);
            }

            return result;
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            foreach (var point in _points)
                point.Draw(_spriteBatch);
            var solutions = FilterSolutions();
            for (var i = 0; i < solutions.Count; i++)
                solutions[i].Draw(_spriteBatch);
            
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
