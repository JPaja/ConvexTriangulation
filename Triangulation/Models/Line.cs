using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xssp.MonoGame.Primitives2D;

namespace Triangulation.Models
{
    public readonly struct Line : IDrawable
    {
        private readonly Color _color;
        public Line(Dot dot1, Dot dot2, Color color)
        {
            Dot1 = dot1;
            Dot2 = dot2;
            _color = color;
        }
        
        public Line(Dot dot1, Dot dot2)
        {
            Dot1 = dot1;
            Dot2 = dot2;
            _color = Color.Yellow;
        }
        
        public Dot Dot1 { get; }
        public Dot Dot2 { get; }

        public void Draw(SpriteBatch spriteBatch)
        {
            var position1 = Dot1.Position;
            var position2 = Dot2.Position;
            spriteBatch.DrawLine(position1,position2,_color,3f,3);
        }
        
    }
}