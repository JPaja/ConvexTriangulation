using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xssp.MonoGame.Primitives2D;

namespace Triangulation.Models
{
    public readonly struct Dot : IEquatable<Dot>, IDrawable
    {
        private const float HalfWidth = 5f;

        private readonly Color _color;
        public Dot(Dot dot, Color color)
        {
            Position = dot.Position;
            _color = color;
        }
        public Dot(Vector2 position)
        {
            Position = position;
            _color = Color.Red;
        }
        public Dot(float x,float y)
        {
            Position = new Vector2(x,y);
            _color = Color.Red;
        }
        
        public Vector2 Position { get; }

        public float X => Position.X;
        public float Y => Position.Y;
        
        public void Draw(SpriteBatch spriteBatch)
        {
            var position = Position - new Vector2(HalfWidth, HalfWidth);
            var size = new Vector2(HalfWidth*2, HalfWidth*2);
            spriteBatch.FillRectangle(position,size,_color, 5);
        }

        public bool Equals(Dot other) 
            => Position.Equals(other.Position);

        public override bool Equals(object? obj)
            => obj is Dot other && Equals(other);

        public override int GetHashCode() 
            => Position.GetHashCode();

        public override string ToString() 
            => Position.ToString();
    }
}