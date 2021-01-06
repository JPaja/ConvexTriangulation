using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Triangulation.Models;
using IDrawable = Triangulation.Models.IDrawable;

namespace Triangulation.Utils
{
    public class PolygonUtils
    {
        public static Polygon Hull(List<Dot> points, List<(IDrawable,bool Permanent)> solutions, bool triangulate = true)
        {
            points = points
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .ToList();
            return DivideAndConquerHull(points,solutions,triangulate);
        }

        public static Polygon DivideAndConquerHull(List<Dot> points, List<(IDrawable,bool Remove)> solutions, bool triangulate = true)
        {
            if (points.Count == 1)
            {
                var result = new Polygon(points.ToArray());
                AddPermanentSolution(result,solutions);
                return result;
            }

            var leftPoints = points
                .Take(points.Count / 2)
                .ToList();
            var rightPoints = points
                .Skip(points.Count / 2)
                .ToList();

            var left = DivideAndConquerHull(leftPoints,solutions,triangulate);
            var right = DivideAndConquerHull(rightPoints,solutions,triangulate);
            var solution = Merge(left, right,solutions,triangulate);
            RemoveSolution(left, solutions);
            RemoveSolution(right, solutions);
            AddPermanentSolution(solution,solutions);
            return solution;
        }

        private static Polygon Merge(Polygon left, Polygon right, List<(IDrawable,bool Permanent)> solutions, bool triangulate = true)
        {
            var topTangent = GetTopTangent(left, right,solutions);
            var botTangent = GetBotTangent(left, right,solutions);

            var result = new List<Dot>();
            for(var i = 0; i <= left.VertexIndexOf(topTangent.Dot1); i++)
                result.Add(left.Vertex[i]);
            for(var i = right.VertexIndexOf(topTangent.Dot2); (i % right.VertexLength) != right.VertexIndexOf(botTangent.Dot2); i++)
                result.Add(right.Vertex[i % right.VertexLength]);
            result.Add(botTangent.Dot2);
            for(var i = left.VertexIndexOf(botTangent.Dot1); (i % left.VertexLength) != 0; i++)
                result.Add(left.Vertex[i % left.VertexLength]);
            
            var innerLeft = new List<Dot>(); 
            for(var i = left.VertexIndexOf(left.NextVertex(topTangent.Dot1)); (i % left.VertexLength) != left.VertexIndexOf(botTangent.Dot1); i++)
                innerLeft.Add(left.Vertex[i % left.VertexLength]);
            
            var innerRight = new List<Dot>(); 
            
            for(var i = right.VertexIndexOf(right.PreviousVertex(topTangent.Dot2)); i != right.VertexIndexOf(botTangent.Dot2); i =(i == 0?right.VertexLength: i-1))
                innerRight.Add(right.Vertex[i % right.VertexLength]);
            /*for(var i = right.VertexIndexOf(topTangent.Dot2) - 1; i >= 0; i--)
                innerRight.Add(right.Vertex[i]);
            for(var i = right.VertexLength - 1; i > right.VertexIndexOf(botTangent.Dot2); i--)
                innerRight.Add(right.Vertex[i]);*/
            
            var innderDots = new List<Dot>();
            innderDots.AddRange(left.Inner);
            innderDots.AddRange(innerLeft);
            innderDots.AddRange(right.Inner);
            innderDots.AddRange(innerRight);

            var triangulation = new List<Line>();
            triangulation.AddRange(left.Triangulations);
            triangulation.AddRange(right.Triangulations);
            
            GenerateInnerBoundTriangulations(left,right,solutions, innerLeft, triangulation, topTangent, botTangent, innerRight);

            innerLeft.Insert(0,topTangent.Dot1);
            if (!topTangent.Dot1.Equals(botTangent.Dot1))
                innerLeft.Add(botTangent.Dot1);
            innerRight.Insert(0,topTangent.Dot2);
            if (!topTangent.Dot2.Equals(botTangent.Dot2))
                innerRight.Add(botTangent.Dot2);
            if (innerRight.Count != 1 || innerLeft.Count != 1)
            {
                var newTriangulation = Triangulate(innerLeft, innerRight, solutions);
                triangulation.AddRange(newTriangulation);
            }

            return new Polygon(result.ToArray(),innderDots.ToArray(),triangulation.ToArray());
        }

        private static void GenerateInnerBoundTriangulations(Polygon left, Polygon right,List<(IDrawable, bool Permanent)> solutions, List<Dot> innerLeft, List<Line> triangulation,
            Line topTangent, Line botTangent, List<Dot> innerRight)
        {
            GenerateLeftInnerBoundTriangulation(left,right,solutions, innerLeft, triangulation, topTangent, botTangent);
            GenerateRightInnerBoundTriangulation(left,right,solutions, triangulation, topTangent, botTangent, innerRight);
        }

        private static void GenerateRightInnerBoundTriangulation(Polygon left, Polygon right,List<(IDrawable, bool Permanent)> solutions, List<Line> triangulation, Line topTangent,
            Line botTangent, List<Dot> innerRight)
        {
            if (!innerRight.Any())
            {
                if (!topTangent.Dot2.Equals(botTangent.Dot2) && !right.NextVertex(topTangent.Dot2).Equals(botTangent.Dot2))
                {
                    triangulation.Add(new Line(topTangent.Dot2, botTangent.Dot2, Color.Purple));
                    AddTemporarySolution(new Line(topTangent.Dot2, botTangent.Dot2, Color.Purple), solutions);
                }
                return;
            }
            
            triangulation.Add(new Line(topTangent.Dot2, innerRight[0], Color.Purple));
            AddTemporarySolution(new Line(topTangent.Dot2, innerRight[0], Color.Purple), solutions);
            for (int i = 1; i < innerRight.Count; i++)
            {
                triangulation.Add(new Line(innerRight[i - 1], innerRight[i], Color.Purple));
                AddTemporarySolution(new Line(innerRight[i - 1], innerRight[i], Color.Purple), solutions);
            }
            triangulation.Add(new Line(innerRight.Last(), botTangent.Dot2, Color.Purple));
            AddTemporarySolution(new Line(innerRight.Last(), botTangent.Dot2, Color.Purple), solutions);
        }

        private static void GenerateLeftInnerBoundTriangulation(Polygon left, Polygon right,List<(IDrawable, bool Permanent)> solutions,
            List<Dot> innerLeft, List<Line> triangulation,
            Line topTangent, Line botTangent)
        {

            if (!innerLeft.Any())
            {
                if (!topTangent.Dot1.Equals(botTangent.Dot1) && !left.PreviousVertex(topTangent.Dot1).Equals(botTangent.Dot1))
                {
                    triangulation.Add(new Line(topTangent.Dot1, botTangent.Dot1, Color.Purple));
                    AddTemporarySolution(new Line(topTangent.Dot1, botTangent.Dot1, Color.Purple), solutions);
                }
                return;
            }

            triangulation.Add(new Line(topTangent.Dot1, innerLeft[0], Color.Purple));
            AddTemporarySolution(new Line(topTangent.Dot2, innerLeft[0], Color.Purple), solutions);
            for (int i = 1; i < innerLeft.Count; i++)
            {
                triangulation.Add(new Line(innerLeft[i - 1], innerLeft[i], Color.Purple));
                AddTemporarySolution(new Line(innerLeft[i - 1], innerLeft[i], Color.Purple), solutions);
            }
            triangulation.Add(new Line(innerLeft.Last(), botTangent.Dot2, Color.Purple));
            AddTemporarySolution(new Line(innerLeft.Last(), botTangent.Dot1, Color.Purple), solutions);
        }


        public static Line GetTopTangent(Polygon left, Polygon right, List<(IDrawable,bool Permanent)> solutions)
        {
            bool modified;
            var leftPosition = left.MaxX;
            var rightPosition = right.MinX;
            
            AddTemporarySolution(new Line(leftPosition,rightPosition,Color.Orange),solutions);
           
            do
            {
                modified = false;
                var newLeftPosition = GetPreviousDown(left, leftPosition, rightPosition,true);
                if (!newLeftPosition.Equals(leftPosition))
                {
                    leftPosition = newLeftPosition;
                    AddTemporarySolution(new Line(leftPosition,rightPosition,Color.Orange),solutions);
                    modified = true;
                }
                var newRightPosition = GetNextUp(right, rightPosition,leftPosition,false);
                if (!newRightPosition.Equals(rightPosition))
                {
                    rightPosition = newRightPosition;
                    AddTemporarySolution(new Line(leftPosition,rightPosition,Color.Orange),solutions);
                    modified = true;
                }
            } while (modified);
            return new Line(leftPosition,rightPosition);
        }
        public static Line GetBotTangent(Polygon left, Polygon right, List<(IDrawable,bool Permanent)> solutions)
        {
            bool modified;
            var leftPosition = left.MaxX;
            var rightPosition = right.MinX;
            AddTemporarySolution(new Line(leftPosition,rightPosition,Color.Green),solutions);
            do
            {
                modified = false;
                var newLeftPosition = GetNextUp(left, leftPosition, rightPosition,true);
                if (!newLeftPosition.Equals(leftPosition))
                {
                    leftPosition = newLeftPosition;
                    modified = true;
                    AddTemporarySolution(new Line(leftPosition,rightPosition,Color.Green),solutions);
                }
                var newRightPosition = GetPreviousDown(right, rightPosition,leftPosition,false);
                if (!newRightPosition.Equals(rightPosition))
                {
                    rightPosition = newRightPosition;
                    modified = true;
                    AddTemporarySolution(new Line(leftPosition,rightPosition,Color.Green),solutions);
                }
            } while (modified);
            return new Line(leftPosition,rightPosition);
        }

        private static void AddPermanentSolution(IDrawable drawable, List<(IDrawable,bool Remove)> solutions)
        {
            solutions.Add((drawable,false));
        }
        private static void AddTemporarySolution(IDrawable drawable, List<(IDrawable,bool Remove)> solutions)
        {
            solutions.Add((drawable,false));
            solutions.Add((drawable,true));
        }
        private static void RemoveSolution(IDrawable drawable, List<(IDrawable,bool Remove)> solutions)
        {
            solutions.Add((drawable,true));
        }
        private static Dot GetNextUp(Polygon polygon, Dot polygonPoint, Dot point, bool leftToRight)
        {
            while (Slope(polygon.NextVertex(polygonPoint), point,leftToRight) > Slope(polygonPoint, point,leftToRight))
                polygonPoint = polygon.NextVertex(polygonPoint);
            return polygonPoint;
        }

        private static Dot GetNextDown(Polygon polygon, Dot polygonPoint, Dot point, bool leftToRight)
        {
            while (Slope(polygon.NextVertex(polygonPoint), point,leftToRight) < Slope(polygonPoint, point,leftToRight))
                polygonPoint = polygon.NextVertex(polygonPoint);
            return polygonPoint;
        }

        private static Dot GetPreviousUp(Polygon polygon, Dot polygonPoint, Dot point, bool leftToRight)
        {
            while (Slope(polygon.PreviousVertex(polygonPoint), point,leftToRight) > Slope(polygonPoint, point,leftToRight))
                polygonPoint = polygon.PreviousVertex(polygonPoint);
            return polygonPoint;
        }

        private static Dot GetPreviousDown(Polygon polygon, Dot polygonPoint, Dot point, bool leftToRight)
        {
            while (Slope(polygon.PreviousVertex(polygonPoint), point,leftToRight) < Slope(polygonPoint, point,leftToRight))
                polygonPoint = polygon.PreviousVertex(polygonPoint);
            return polygonPoint;
        }


        /*
        https://en.wikipedia.org/wiki/Slope
        slope == tg(alpha)
        A line is increasing if it goes up from left to right. The slope is positive, i.e. m > 0 {\displaystyle m>0} m>0.
        A line is decreasing if it goes down from left to right. The slope is negative, i.e. m < 0 {\displaystyle m<0} m<0.
        If a line is horizontal the slope is zero. This is a constant function.
        If a line is vertical the slope is undefined (see below).
        */
        private static float Slope(Dot left, Dot right, bool leftToRight)
        {
            var (x, y) = Vector2.Subtract(right.Position, left.Position);
            if (x == 0)
            {
                if(leftToRight)
                    x = 0.0000000001f;
                else 
                    x = -0.0000000001f;
            }
            var ret = -y / x;
            if (float.IsInfinity(ret) || float.IsNaN(ret))
            {
            }
            return ret;
        }
        

        //Monotone polygon triangulation
        private static List<Line> Triangulate(List<Dot> innerLeft, List<Dot> innerRight, List<(IDrawable,bool Remove)> solutions)
        {
            var triangulation = new List<Line>();
            if (!innerLeft.Any() || !innerRight.Any())
                return triangulation;
            int right = 0;
            for (var left = 0; left < innerLeft.Count; left++)
            {
                if(right == innerRight.Count)
                    break;
                var canLeft = left + 1 < innerLeft.Count; /*&& right != innerRight.Count -1*/;
                var canRight = right + -1 >= 0; /*&& left != innerLeft.Count -1*/;
                while (right < innerRight.Count)
                {
                    if (!canRight && !canLeft) 
                        break;
                    triangulation.Add(new Line(innerLeft[left], innerRight[right], Color.Purple));
                    AddTemporarySolution(new Line(innerLeft[left], innerRight[right], Color.Purple), solutions);
                    right++;
                }
                if (left + 1 < innerLeft.Count)
                {
                    triangulation.Add(new Line(innerLeft[left + 1], innerRight[right -1], Color.Purple));
                    AddTemporarySolution(new Line(innerLeft[left + 1], innerRight[right -1], Color.Purple), solutions);
                }
                /*if(right == innerRight.Count)
                    break;
                var canLeft = left + 1 < innerLeft.Count /*&& right != innerRight.Count -1#1#;
                var canRight = right + 1 < innerRight.Count /*&& left != innerLeft.Count -1#1#;*/
                /*if (canLeft && canRight)
                {
                    var distLeft = Vector2.Distance(innerLeft[left + 1].Position, innerRight[right].Position);
                    var distRight = Vector2.Distance(innerLeft[left].Position, innerRight[right + 1].Position);
                    if (distLeft < distRight)
                    {
                        triangulation.Add(new Line(innerLeft[left + 1], innerRight[right], Color.Purple));
                        AddTemporarySolution(new Line(innerLeft[left + 1],innerRight[right], Color.Purple),solutions);
                    }
                    else
                    {
                        triangulation.Add(new Line(innerLeft[left], innerRight[right + 1], Color.Purple));
                        AddTemporarySolution(new Line(innerLeft[left],innerRight[right + 1], Color.Purple),solutions);
                        right++;
                    }
                }
                else if (canLeft)
                {
                    triangulation.Add(new Line(innerLeft[left + 1], innerRight[right], Color.Purple));
                    AddTemporarySolution(new Line(innerLeft[left + 1],innerRight[right], Color.Purple),solutions);
                }
                else if (canRight)
                {
                    triangulation.Add(new Line(innerLeft[left], innerRight[right + 1], Color.Purple));
                    AddTemporarySolution(new Line(innerLeft[left],innerRight[right + 1], Color.Purple),solutions);
                    right++;
                }*/
            }
            
            /*while (left < innerLeft.Count && right < innerRight.Count)
            {
                while (left < innerRight.Count && right < innerRight.Count && CanTriangulate(innerLeft, innerRight,innerLeft[left], innerRight[right]))
                {
                    triangulation.Add(new Line(innerLeft[left], innerRight[right], Color.RosyBrown));
                    AddTemporarySolution(new Line(innerLeft[left],innerRight[right], Color.RosyBrown),solutions);
                    right++;
                }
                left++;
                if (left < innerLeft.Count && right != 0)
                {
                    triangulation.Add(new Line(innerLeft[left], innerRight[right - 1], Color.RosyBrown));
                    AddTemporarySolution(new Line(innerLeft[left], innerRight[right - 1], Color.RosyBrown), solutions);
                }
            }*/
            return triangulation;
        }
        
        private static bool CanTriangulate(List<Dot> innerLeft, List<Dot> innerRight,Dot leftPoint, Dot rightPoint)
        {
            var leftIndex = innerLeft.IndexOf(leftPoint);
            if (leftIndex + 1 < innerLeft.Count && TriangleOrientation(leftPoint, rightPoint, innerLeft[leftIndex + 1]) > 0)
                return false;
            var rightIndex = innerRight.IndexOf(rightPoint);
            if (rightIndex - 1 >= 0 && TriangleOrientation(leftPoint, rightPoint, innerRight[rightIndex - 1]) > 0)
                return false;
            return true;
        }
        
        private static int TriangleOrientation(Dot dot1, Dot dot2, Dot dot3)
        { 
            var val = (dot2.Y - dot1.Y) * (dot3.X - dot2.X) -
                        (dot2.X - dot1.X) * (dot3.Y - dot2.Y);
            return val switch
            {
                > 0 => 1,  //clockwise
                < 0 => -1, //counter clockwise
                { } => 0   //colinear
            };
        }
    }
}