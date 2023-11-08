using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace Tile.Core.Hull
{
    public static class Convex_Hull
    {
        private static (List<Point3d>, List<int>) Andrew_ConvexHull(IEnumerable<Point3d> Pts)
        {
            var PtList = Pts.ToList();
            if (PtList.Count < 4) return (new List<Point3d>(), new List<int>());
            PtList.Sort(Compare);

            var lowerhull = new List<Point3d>();
            for (int i = 0; i < PtList.Count; i++)
            {
                while (lowerhull.Count >= 2 &&
                    Cross2D(lowerhull[lowerhull.Count - 2], lowerhull[lowerhull.Count - 1], PtList[i]) <= 0)
                    lowerhull.RemoveAt(lowerhull.Count - 1);
                lowerhull.Add(PtList[i]);
            }
            var upperhull = new List<Point3d>();
            for (int i = PtList.Count - 1; i >= 0; i--)
            {
                while (upperhull.Count >= 2 &&
                    Cross2D(upperhull[upperhull.Count - 2], upperhull[upperhull.Count - 1], PtList[i]) <= 0)
                    upperhull.RemoveAt(upperhull.Count - 1);
                upperhull.Add(PtList[i]);
            }

            lowerhull.RemoveAt(lowerhull.Count - 1);
            upperhull.RemoveAt(upperhull.Count - 1);
            lowerhull.AddRange(upperhull);

            var IndList = new List<int>();
            for (int i = 0; i < PtList.Count; i++)
                if (lowerhull.Contains(PtList[i]))
                    IndList.Add(i);

            return (lowerhull, IndList);
        }
        private static int Compare(Point3d P1, Point3d P2)
        {
            //if (P1.X != P2.X) return P1.X.CompareTo(P2.X);
            //if (P1.Y != P2.Y) return P2.Y.CompareTo(P2.Y);
            //return P1.Z.CompareTo(P2.Z);
            return P1.CompareTo(P2);
        }
        private static double Cross2D(Point3d O, Point3d A, Point3d B)
        {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }
        private static List<Point3d> ConvertCurvesToPts(IEnumerable<Curve> Crvs, double Tor = 3)
        {
            List<Curve> CurvedCrv = new List<Curve>();
            List<Curve> StraigntCrv = new List<Curve>();
            foreach (var Crv in Crvs)
            {
                if (Crv.Degree == 1)
                    StraigntCrv.Add(Crv);
                else
                    CurvedCrv.Add(Crv);
            }
            var PtsList = new List<Point3d>();
            foreach (var StrPt in StraigntCrv)
            {
                PtsList.AddRange(StrPt.DuplicateSegments().Select(x => x.PointAtEnd).ToList());
                PtsList.AddRange(StrPt.DuplicateSegments().Where(x => !PtsList.Contains(x.PointAtStart)).Select(x => x.PointAtStart));
            }
            foreach (var CrPt in CurvedCrv)
            {
                CrPt.DivideByCount((int)Math.Round(CrPt.GetLength() / Tor), true, out Point3d[] Pts);
                PtsList.AddRange(Pts);
            }
            return PtsList;
        }
        public static Convex_Hull_Object ConvexHull2D(IEnumerable<Point3d> Pts)
        {
            var _ConvextList = Andrew_ConvexHull(Pts);
            return new Convex_Hull_Object(_ConvextList);
        }
        public static Convex_Hull_Object ConvexHull2D(IEnumerable<Curve> Crvs, double Tor)
        {
            var _ConvextList = Andrew_ConvexHull(ConvertCurvesToPts(Crvs, Tor));
            return new Convex_Hull_Object(_ConvextList);
        }
    }
    public struct Convex_Hull_Object
    {
        private List<Point3d> Pts2d;
        private List<int> Indices;
        public Convex_Hull_Object((List<Point3d>, List<int>) Indices)
        {
            this.Pts2d = Indices.Item1;
            this.Indices = Indices.Item2;
        }
        public List<Point3d> GetPoints => this.Pts2d;
        public List<int> GetProfileIndex => this.Indices;
        public PolylineCurve GetPolyCurve
        {
            get
            {
                var Pts = this.GetPoints;
                List<Line> PolySegs = new List<Line>();
                for (int i = 0; i < Pts.Count; i++)
                {
                    if (Pts[i] == Pts.Last())
                    {
                        PolySegs.Add(new Line(Pts[i], Pts[0]));
                    }
                    else
                        PolySegs.Add(new Line(Pts[i], Pts[i + 1]));
                }
                return Curve.JoinCurves(PolySegs.Select(x => new LineCurve(x)))[0] as PolylineCurve;

            }
        }
        public static implicit operator PolylineCurve(Convex_Hull_Object Object) => Object.GetPolyCurve;
        public static implicit operator List<Point3d>(Convex_Hull_Object Object) => Object.GetPoints;
    }
}
