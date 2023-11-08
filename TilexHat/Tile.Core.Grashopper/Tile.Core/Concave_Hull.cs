using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Grasshopper.Kernel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tile.Core.Hull
{
    public static class Concave_Hull
    {
        public static Concave_Hull_Object ConcaveHull2D(IEnumerable<Point3d> Pts, double Alpha = 1.1)
        {
            Mesh delaunayMesh = GetDelaunayMesh(Pts);
            double Thre = AverageLength(delaunayMesh, out _) * Alpha;
            Mesh HullMesh = GenerateConcaveHull(delaunayMesh, Thre);
            return new Concave_Hull_Object(HullMesh);
        }
        public static Concave_Hull_Object ConcaveHull2D(IEnumerable<Curve> Crvs, double Tor = 3, double Alpha = 1.1)
        {
            var Pts = ConvertCurvesToPts(Crvs, Tor);
            return ConcaveHull2D(Pts, Alpha);
        }
        private static Mesh GenerateConcaveHull(Mesh delaunayMesh, double Thre)
        {
            Mesh Copy_Mesh = delaunayMesh.DuplicateMesh();

            var EdgeIntList = GetNakedEdges(Copy_Mesh);

            List<int> RemoveInd = EdgeIntList.Where(x => Copy_Mesh.TopologyEdges.EdgeLine(x).Length > Thre).ToList();

            if (RemoveInd.Count == 0 || EdgeIntList.Count <= 3)
                return Copy_Mesh;
            else if (RemoveInd.Count > 1)
            {
                var faceInd = Copy_Mesh.TopologyEdges.GetConnectedFaces(RemoveInd[0])[0];
                Copy_Mesh.Faces.RemoveAt(faceInd);
                return GenerateConcaveHull(Copy_Mesh, Thre);
            }
            else
            {
                var faceInd = Copy_Mesh.TopologyEdges.GetConnectedFaces(RemoveInd[0])[0];
                Copy_Mesh.Faces.RemoveAt(faceInd);
                return Copy_Mesh;
            }
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
        private static Mesh GetDelaunayMesh(List<Point3d> pts)
        {
            //convert point3d to node2
            //grasshopper requres that nodes are saved within a Node2List for Delaunay
            var nodes = new Grasshopper.Kernel.Geometry.Node2List();
            for (int i = 0; i < pts.Count; i++)
            {
                //notice how we only read in the X and Y coordinates
                //  this is why points should be mapped onto the XY plane
                nodes.Append(new Grasshopper.Kernel.Geometry.Node2(pts[i].X, pts[i].Y));
            }

            //solve Delaunay
            var delMesh = new Mesh();
            var faces = new List<Grasshopper.Kernel.Geometry.Delaunay.Face>();

            faces = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Faces(nodes, 1);

            //output
            delMesh = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Mesh(nodes, 1, ref faces);
            return delMesh;
        }
        public static Mesh GetDelaunayMesh(IEnumerable<Point3d> pts)
        {
            return GetDelaunayMesh(pts.ToList());
        }
        public static double AverageLength(Mesh mesh, out List<Line> AllEdges)
        {
            var CopyMesh = mesh.DuplicateMesh();
            AllEdges = new List<Line>();
            double Length = 0;
            for (int i = 0; i < CopyMesh.TopologyEdges.Count; i++)
            {
                Length += CopyMesh.TopologyEdges.EdgeLine(i).Length / CopyMesh.TopologyEdges.Count;
                AllEdges.Add(CopyMesh.TopologyEdges.EdgeLine(i));
            }
            return Length;
        }
        public static List<int> GetNakedEdges(Mesh mesh)
        {
            // Find the naked edges of the mesh
            var nakedEdges = new List<int>();
            for (int edgeIndex = 0; edgeIndex < mesh.TopologyEdges.Count; edgeIndex++)
            {
                if (mesh.TopologyEdges.GetConnectedFaces(edgeIndex).Length == 1)
                {
                    nakedEdges.Add(edgeIndex);
                }
            }
            return nakedEdges;
        }
    }
    public struct Concave_Hull_Object
    {
        private Mesh _ConcaveMesh;
        public Concave_Hull_Object(Mesh ConcaveMesh)
        {
            this._ConcaveMesh = ConcaveMesh;
        }
        public Mesh GetMesh { get { return _ConcaveMesh; } }
        public List<int> GetProfileIndex
        {
            get
            {
                return Concave_Hull.GetNakedEdges(this._ConcaveMesh);
            }
        }
        public Curve GetPolyCurve
        {
            get
            {
                var Index = this.GetProfileIndex;
                List<Line> AllEdges;
                Concave_Hull.AverageLength(this._ConcaveMesh, out AllEdges);
                var NakeEdges = Index.Select(x => new LineCurve(AllEdges[x])).ToList();
                var JoinCurve = Curve.JoinCurves(NakeEdges)[0];
                return JoinCurve;
            }
        }
        public List<Point3d> GetPoints
        {
            get
            {
                PolylineCurve PLN = this.GetPolyCurve as PolylineCurve;
                return PLN.ToPolyline().ToList();
            }
        }
        public Concave_Hull_Object Unset => new Concave_Hull_Object(new Mesh());
    }
}
