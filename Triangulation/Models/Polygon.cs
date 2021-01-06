using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = System.Drawing.Rectangle;

namespace Triangulation.Models
{
    public class Polygon : IDrawable
    {
        private Dictionary<Dot,int> _pointsMap = new Dictionary<Dot, int>();
        private Dot[] _vertieces;
        private Dot[] _innerPoints = Array.Empty<Dot>();
        private Line[] _triangulations = Array.Empty<Line>();

        public Polygon(Dot[] vertices, Dot[] innerPoints, Line[] triangulations) : this(vertices,innerPoints)
        {
            _triangulations = triangulations;
        }

        public Polygon(Dot[] vertices, Dot[] innerPoints) : this(vertices)
        {
            _innerPoints = innerPoints;
        }
        
        public Polygon(Dot[] vertices)
        {
            if (vertices.Length == 0)
                throw new ArgumentException("Array length must be greater than 0");
            
            _vertieces = vertices.Distinct().ToArray();
            MinX = _vertieces[0];
            MinY = _vertieces[0];
            MaxX = _vertieces[0];
            MaxY = _vertieces[0];
            for (var i = 0; i < _vertieces.Length; i++)
            {
                if (_vertieces[i].X < MinX.X) MinX = _vertieces[i];
                if (_vertieces[i].Y < MinY.Y) MinY = _vertieces[i];
                if (_vertieces[i].X > MaxX.X) MaxX = _vertieces[i];
                if (_vertieces[i].Y > MaxY.Y) MaxY = _vertieces[i];
                _pointsMap[_vertieces[i]] = i;
            }
        }
                
        public ImmutableArray<Dot> Vertex => _vertieces.ToImmutableArray();
        public ImmutableArray<Dot> Inner => _innerPoints.ToImmutableArray();
        public ImmutableArray<Line> Triangulations => _triangulations.ToImmutableArray();

        public readonly Dot MinX;
        public readonly Dot MinY;
        public readonly Dot MaxX;
        public readonly Dot MaxY;
        
        public int VertexIndexOf(Dot dot) => _pointsMap[dot];
        public Dot NextVertex(Dot dot) => _vertieces[(VertexIndexOf(dot) + 1) % VertexLength];

        public Dot PreviousVertex(Dot dot)
            => VertexIndexOf(dot) switch
            {
                0 => _vertieces[VertexLength - 1],
                { } index => _vertieces[index - 1]
            };

        public int VertexLength => _vertieces.Length;
        
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var point in _vertieces)
                point.Draw(spriteBatch);
            foreach (var point in _innerPoints)
            {
                var p = new Dot(point, Color.Blue);
                p.Draw(spriteBatch);
            }
            foreach (var triangulation in _triangulations)
                triangulation.Draw(spriteBatch);

            for (var i = 0; i < _vertieces.Length; i++)
            {
                var line = new Line(_vertieces[i], _vertieces[(i + 1) % VertexLength]);
                line.Draw(spriteBatch);
            }
        }
    }
}