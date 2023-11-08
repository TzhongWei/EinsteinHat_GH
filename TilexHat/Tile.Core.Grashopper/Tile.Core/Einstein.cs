using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Tile.Core
{
    public class Einstein:ICloneable
    {
        public enum Type
        {
            H = 0,
            T = 1,
            P = 2,
            F = 3
        }
        public Einstein() { }
        protected const double r3 = 1.7320508075688772;
        protected const double hr3 = 0.8660254037844386;
        public int Level { get; private set; } = 1;
        /// <summary>
        /// Match unit interval to line segment p -> q
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Transform MatchSeg(Point3d p, Point3d q)
        {
            Transform TS = Transform.Identity;
            TS[0, 0] = q.X - p.X;
            TS[0, 1] = p.Y - q.Y;
            TS[0, 3] = p.X;
            TS[1, 0] = q.Y - p.Y;
            TS[1, 1] = q.X - p.X;
            TS[1, 3] = p.Y;
            TS[2, 3] = p.Z;
            return TS;
        }
        /// <summary>
        /// MatchTwo Line And Generate a Transformation Matrix
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="q1"></param>
        /// <param name="p2"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Transform MatchTwo(Point3d p1, Point3d q1, Point3d p2, Point3d q2)
        {
            var TS1 = MatchSeg(p2, q2);
            var TS2 = MatchSeg(p1, q1);
            Transform TS3;
            TS2.TryGetInverse(out TS3);
            return TS1 * TS3;
        }
        /// <summary>
        /// construct a 2D transformation matrix in rhino with six values
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Transform TwoDMt(double a, double b, double c, double d, double e, double f)
        {
            Transform TS = Transform.Identity;
            TS[0, 0] = a;
            TS[0, 1] = b;
            TS[0, 3] = c;
            TS[1, 0] = d;
            TS[1, 1] = e;
            TS[1, 3] = f;
            return TS;
        }
        /// <summary>
        /// Intersect two lines defined by segments p1 -> q1 and p2 -> q2
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="q1"></param>
        /// <param name="p2"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static Point3d intersect(Point3d p1, Point3d q1, Point3d p2, Point3d q2)
        {
            var d = (q2.Y - p2.Y) * (q1.X - p1.X) - (q2.X - p2.X) * (q1.Y - p1.Y);
            var uA = ((q2.X - p2.X) * (p1.Y - p2.Y) - (q2.Y - p2.Y) * (p1.X - p2.X)) / d;
            var uB = ((q1.X - p1.X) * (p1.Y - p2.Y) - (q1.Y - p1.Y) * (p1.X - p2.X)) / d;

            return new Point3d(p1.X + uA * (q1.X - p1.X), p1.Y + uA * (q1.Y - p1.Y), 0);
        }
        public static Point3d Pt(double x, double y)
        {
            return new Point3d(x, y, 0);
        }
        public static Point3d HexPt(double x, double y)
        {
            return Pt(x + 0.5 * y, hr3 * y);
        }
        public MetaTile constructPatch(MetaTile[] Tiles)
        {
            List<string> rules = new List<string>(){
          "H",
          "0 0 P 2",
          "1 0 H 2",
          "2 0 P 2",
          "3 0 H 2",
          "4 4 P 2",
          "0 4 F 3",
          "2 4 F 3",
          "4 1 3 2 F 0",
          "8 3 H 0",
          "9 2 P 0",
          "10 2 H 0",
          "11 4 P 2",
          "12 0 H 2",
          "13 0 F 3",
          "14 2 F 1",
          "15 3 H 4",
          "8 2 F 1",
          "17 3 H 0",
          "18 2 P 0",
          "19 2 H 2",
          "20 4 F 3",
          "20 0 P 2",
          "22 0 H 2",
          "23 4 F 3",
          "23 0 F 3",
          "16 0 P 2",
          "9 4 0 2 T 2",
          "4 0 F 3"
          };
            var ret = new MetaTile(new List<Point3d>(), Tiles[0].Width);
            Dictionary<string, MetaTile> shapes = new Dictionary<string, MetaTile>();
            shapes.Add("H", Tiles[0]);
            shapes.Add("T", Tiles[1]);
            shapes.Add("P", Tiles[2]);
            shapes.Add("F", Tiles[3]);

            foreach (string r in rules)
            {
                List<string> Token = r.Split(' ').ToList();
                if (r.Length == 1)
                {
                    ret.AddChild(Transform.Identity, shapes[r]);
                }
                else if (Token.Count == 4)
                {
                    int I_0 = int.Parse(Token[0]);
                    int I_1 = int.Parse(Token[1]);
                    int I_3 = int.Parse(Token[3]);
                    var poly = ret.Children[I_0].Value.Shape;
                    var T = ret.Children[I_0].Key;
                    var P = poly[(I_1 + 1) % poly.Count];
                    P.Transform(T);
                    var Q = poly[I_1];
                    Q.Transform(T);
                    var nshp = shapes[Token[2]];
                    var npoly = nshp.Shape;

                    ret.AddChild(
                      MatchTwo(npoly[I_3], npoly[(I_3 + 1) % npoly.Count], P, Q),
                      nshp.Clone()
                      );
                }
                else
                {
                    int I_0 = int.Parse(Token[0]);
                    int I_1 = int.Parse(Token[1]);
                    int I_2 = int.Parse(Token[2]);
                    int I_3 = int.Parse(Token[3]);
                    int I_5 = int.Parse(Token[5]);

                    var chP = ret.Children[I_0];
                    var chQ = ret.Children[I_2];

                    var P = chQ.Value.Shape[I_3];
                    P.Transform(chQ.Key);
                    var Q = chP.Value.Shape[I_1];
                    Q.Transform(chP.Key);
                    var nshp = shapes[Token[4]];
                    var npoly = nshp.Shape;
                    ret.AddChild(
                      MatchTwo(npoly[I_5], npoly[(I_5 + 1) % npoly.Count], P, Q),
                      nshp.Clone()
                      );
                }
            }
            return ret;
        }
        public MetaTile[] ConstructMetatiles(MetaTile patch)
        {
            var bps1 = patch.EvalChild(8, 2);
            var bps2 = patch.EvalChild(21, 2);

            var rbps = new Point3d(bps2);
            rbps.Transform(Transform.Rotation(-2.0 * Math.PI / 3.0, bps1));

            var p72 = patch.EvalChild(7, 2);
            var p252 = patch.EvalChild(25, 2);

            var llc = intersect(bps1, rbps, patch.EvalChild(6, 2), p72);
            var w = patch.EvalChild(6, 2) - llc;

            // construct new H_outline
            var new_H_outline = new List<Point3d>() { llc, bps1 };

            w.Transform(Transform.Rotation(-Math.PI / 3.0, Point3d.Origin));
            new_H_outline.Add((new_H_outline[1] + w));
            new_H_outline.Add(patch.EvalChild(14, 2));
            w.Transform(Transform.Rotation(-Math.PI / 3.0, Point3d.Origin));
            new_H_outline.Add(new_H_outline[3] - w);
            new_H_outline.Add(patch.EvalChild(6, 2));

            new_H_outline.Add(new_H_outline[0]);

            var new_H = new MetaTile(new_H_outline, patch.Width * 2);

            foreach (int ch in new int[] { 0, 9, 16, 27, 26, 6, 1, 8, 10, 15 })
            {
                new_H.AddChild(patch.Children[ch].Key.Clone(), patch.Children[ch].Value.Clone());
            }

            //construct new P outline
            var new_P_outline = new List<Point3d>() { p72, (p72 + (bps1 - llc)), bps1, llc };

            new_P_outline.Add(new_P_outline[0]);

            var new_P = new MetaTile(new_P_outline, patch.Width * 2);
            foreach (int ch in new int[] { 7, 2, 3, 4, 28 })
            {
                new_P.AddChild(patch.Children[ch].Key.Clone(), patch.Children[ch].Value.Clone());
            }

            //construct new F outline
            var new_F_outline = new List<Point3d>(){bps2, patch.EvalChild(24, 2), patch.EvalChild(25, 0),
          p252, (p252 + (llc - bps1)) };

            new_F_outline.Add(new_F_outline[0]);

            var new_F = new MetaTile(new_F_outline, patch.Width * 2);
            foreach (int ch in new int[] { 21, 20, 22, 23, 24, 25 })
            {
                new_F.AddChild(patch.Children[ch].Key.Clone(), patch.Children[ch].Value.Clone());
            }

            //construct new T outline
            var AAA = new_H_outline[2];
            var BBB = new_H_outline[1] + (new_H_outline[4] - new_H_outline[5]);
            var CCC = new Point3d(AAA);
            CCC.Transform(Transform.Rotation(-Math.PI / 3.0, BBB));
            var new_T_outline = new List<Point3d>() { BBB, CCC, AAA };

            new_T_outline.Add(new_T_outline[0]);

            var new_T = new MetaTile(new_T_outline, patch.Width * 2);
            new_T.AddChild(patch.Children[11].Key.Clone(), patch.Children[11].Value.Clone());

            new_H.ReCentre();
            new_P.ReCentre();
            new_F.ReCentre();
            new_T.ReCentre();

            return new MetaTile[] { (MetaTile)new_H.Clone(), (MetaTile)new_T.Clone(), (MetaTile)new_P.Clone(), (MetaTile)new_F.Clone() };

        }
        /// <summary>
        /// This is the share data for HatTile and MetaTile
        /// </summary>
        public abstract class Geom
        {
            public readonly List<Point3d> Hat_Outline = new List<Point3d>()
        {
          HexPt(0, 0),
          HexPt(-1, -1),
          HexPt(0, -2),
          HexPt(2, -2),
          HexPt(2, -1),
          HexPt(4, -2),
          HexPt(5, -1),
          HexPt(4, 0),
          HexPt(3, 0),
          HexPt(2, 2),
          HexPt(0, 3),
          HexPt(0, 2),
          HexPt(-1, 2),
          HexPt(0, 0)
          };
            public List<Point3d> Shape = new List<Point3d>();
            public List<KeyValuePair<Transform, Geom>> Children = new List<KeyValuePair<Transform, Geom>>();
            public double Width = double.NaN;
            public virtual Curve PreviewShape
            {
                get
                {
                    return new PolylineCurve(Shape);
                }
            }
            public abstract KeyValuePair<string, Curve> draw(Transform Scale);
            public Geom()
            {
            }
            public abstract Geom Clone();
        }
        /// <summary>
        /// The Class of Hat Tile
        /// </summary>
        public class HatTile : Geom
        {
            public string Label = "";
            public Curve HatShape = null;

            public HatTile(string Label) : base()
            {
                this.Label = Label;
                this.Shape = new List<Point3d>(this.Hat_Outline);
            }
            public override KeyValuePair<string, Curve> draw(Transform Scale)
            {
                List<Point3d> nShp = new List<Point3d>(this.Shape);

                for (int i = 0; i < Shape.Count; i++)
                {
                    Point3d newPt = nShp[i];
                    newPt.Transform(Scale);
                    nShp[i] = newPt;
                }
                this.HatShape = new PolylineCurve(nShp);
                return new KeyValuePair<string, Curve>(this.Label, HatShape);
            }
            public override Geom Clone()
            {
                var Copy = new HatTile(this.Label);
                Copy.Width = this.Width;
                for (int i = 0; i < this.Children.Count; i++)
                {
                    Copy.Children.Add(new KeyValuePair<Transform, Geom>(this.Children[i].Key, this.Children[i].Value.Clone()));
                }
                return Copy;
            }
        }
        public class MetaTile : Geom
        {
            public MetaTile(List<Point3d> Shape, double Width) : base()
            {
                this.Shape = Shape;
                this.Width = Width;
                this.Children = new List<KeyValuePair<Transform, Geom>>();
            }
            public void AddChild(Transform TS, Geom Hat)
            {
                this.Children.Add(new KeyValuePair<Transform, Geom>(TS, Hat));
            }
            public Point3d EvalChild(int n, int i)
            {
                var Pt = this.Children[n].Value.Shape[i];
                Pt.Transform(Children[n].Key);
                return Pt;
            }
            public override KeyValuePair<string, Curve> draw(Transform Scale)
            {
                List<Point3d> nShp = new List<Point3d>(this.Shape);
                for (int i = 0; i < this.Shape.Count; i++)
                {
                    Point3d newPt = nShp[i];
                    newPt.Transform(Scale);
                    nShp[i] = newPt;
                }
                return new KeyValuePair<string, Curve>("Meta", new PolylineCurve(nShp));
            }
            public void ReCentre()
            {
                double Cx = 0, Cy = 0;
                foreach (Point3d Pt in this.Shape)
                {
                    Cx += Pt.X / this.Shape.Count;
                    Cy += Pt.Y / this.Shape.Count;
                }
                var Tr = new Point3d(-Cx, -Cy, 0);
                for (int i = 0; i < this.Shape.Count; i++)
                    this.Shape[i] += Tr;
                var M = Transform.Translation(new Vector3d(-Cx, -Cy, 0));
                for (int i = 0; i < this.Children.Count; i++)
                {
                    KeyValuePair<Transform, Geom> Ch = this.Children[i];
                    this.Children[i] = new KeyValuePair<Transform, Geom>(M * Ch.Key, Ch.Value);
                }
            }
            public override Geom Clone()
            {
                var Copy = new MetaTile(new List<Point3d>(this.Shape), this.Width);
                for (int i = 0; i < this.Children.Count; i++)
                {
                    Copy.Children.Add(new KeyValuePair<Transform, Geom>(this.Children[i].Key, this.Children[i].Value.Clone()));
                }
                return Copy;
            }
        }
        public abstract class InitTiles : MetaTile
        {
            public InitTiles(List<Point3d> Shape, double Width) : base(Shape, Width)
            {
            }
        }
        public class H_init : InitTiles
        {
            private static readonly List<Point3d> H_Outline = new List<Point3d>()
        {
          Pt(0, 0),
          Pt(4, 0),
          Pt(4.5, hr3),
          Pt(2.5, 5 * hr3),
          Pt(1.5, 5 * hr3),
          Pt(-0.5, hr3),
          Pt(0, 0)
          };
            public H_init() : base(H_Outline, 2)
            {
                this.AddChild(MatchTwo(Hat_Outline[5], Hat_Outline[7], H_Outline[5], H_Outline[0]), H_Hat.Clone());
                this.AddChild(MatchTwo(Hat_Outline[9], Hat_Outline[11], H_Outline[1], H_Outline[2]), H_Hat.Clone());
                this.AddChild(MatchTwo(Hat_Outline[5], Hat_Outline[7], H_Outline[3], H_Outline[4]), H_Hat.Clone());
                this.AddChild((Transform.Translation(new Vector3d(2.5, hr3, 0))) * (
                  TwoDMt(-0.5, -hr3, 0, hr3, -0.5, 0) * TwoDMt(0.5, 0, 0, 0, -0.5, 0)
                  ), H1_Hat.Clone());
            }
        }
        public class T_init : InitTiles
        {
            private static readonly List<Point3d> T_Outline = new List<Point3d>()
        {
          Pt(0, 0), Pt(3, 0), Pt(1.5, 3 * hr3),Pt(0, 0)
          };
            public T_init() : base(T_Outline, 2)
            {
                this.AddChild(TwoDMt(0.5, 0, 0.5, 0, 0.5, hr3), T_Hat.Clone());
            }
        }
        public class P_init : InitTiles
        {
            private static readonly List<Point3d> P_Outline = new List<Point3d>()
        {
          Pt(0, 0), Pt(4, 0),
          Pt(3, 2 * hr3), Pt(-1, 2 * hr3),Pt(0, 0)
          };
            public P_init() : base(P_Outline, 2)
            {
                this.AddChild(TwoDMt(0.5, 0, 1.5, 0, 0.5, hr3), P_Hat.Clone());
                this.AddChild((Transform.Translation(new Vector3d(0, 2 * hr3, 0))) * (
                  TwoDMt(0.5, hr3, 0, -hr3, 0.5, 0) * TwoDMt(0.5, 0.0, 0.0, 0.0, 0.5, 0.0)
                  ), P_Hat.Clone());
            }
        }
        public class F_init : InitTiles
        {
            private static readonly List<Point3d> F_Outline = new List<Point3d>()
        {
          Pt(0, 0), Pt(3, 0),
          Pt(3.5, hr3), Pt(3, 2 * hr3), Pt(-1, 2 * hr3),Pt(0, 0)
          };
            public F_init() : base(F_Outline, 2)
            {
                this.AddChild(TwoDMt(0.5, 0, 1.5, 0, 0.5, hr3), F_Hat.Clone());
                this.AddChild((Transform.Translation(new Vector3d(0, 2 * hr3, 0))) * (
                  TwoDMt(0.5, hr3, 0, -hr3, 0.5, 0) * TwoDMt(0.5, 0.0, 0.0, 0.0, 0.5, 0.0)), F_Hat.Clone());
            }
        }
        //=========================================================================================================
        // Static
        //=========================================================================================================
        public static HatTile H1_Hat = new HatTile("H1");
        public static HatTile H_Hat = new HatTile("H");
        public static HatTile T_Hat = new HatTile("T");
        public static HatTile P_Hat = new HatTile("P");
        public static HatTile F_Hat = new HatTile("F");
        public MetaTile[] Tiles = new MetaTile[] { new H_init(), new T_init(), new P_init(), new F_init() };
        public void reset()
        {
            Tiles = new MetaTile[] { new H_init(), new T_init(), new P_init(), new F_init() };
            this.Level = 1;
            Hat_Transform = new List<Transform>();
            Hat_Labels = new List<string>();
        }
        public void NextIteration()
        {
            var patch = constructPatch(this.Tiles);
            this.Tiles = ConstructMetatiles(patch);
            this.Level++;
        }
        public object Clone()
        {
            var CopyEin = new Einstein();
            CopyEin.Level = this.Level;
            CopyEin.Tiles = new MetaTile[] {
                (MetaTile)this.Tiles[0].Clone(), 
                (MetaTile)this.Tiles[1].Clone(), 
                (MetaTile)this.Tiles[2].Clone(), 
                (MetaTile)this.Tiles[3].Clone()};

            CopyEin.Hat_Transform = new List<Transform>(this.Hat_Transform);
            CopyEin.Hat_Labels = new List<string>(this.Hat_Labels);
            
            return CopyEin;
        }
        public List<KeyValuePair<string, Curve>> Draw(Type type = Type.H)
        {
            List<KeyValuePair<string, Curve>> output = new List<KeyValuePair<string, Curve>>();

            Geom Tile = this.Tiles[(int)type];

            this.Draw(Tile, Level, ref output);

            return output;
        }
        public List<Transform> Hat_Transform { get; private set; } = new List<Transform>();
        public List<string> Hat_Labels = new List<string>();
        public List<Transform> Draw(Geom Tile, int Level, ref List<KeyValuePair<string, Curve>> output)
        {
            List<Transform> TS = new List<Transform>();
            if (Level > 1)
            {
                List<KeyValuePair<Transform, Geom>> TileChildren = Tile.Children;
                for (int i = 0; i < TileChildren.Count; i++)
                {
                    Geom TileChild = TileChildren[i].Value.Clone();   //MetaTile
                    output.Add(TileChild.draw(TileChildren[i].Key));
                    Transform Adjust = TileChildren[i].Key;

                    //Additional Transform for children

                    //if (i != 0 && i != 2) continue;

                    var ChildrenOfTileChild = TileChild.Children;
                    for (int j = 0; j < ChildrenOfTileChild.Count; j++)
                    {
                        Geom ChildOfTileChild = ChildrenOfTileChild[j].Value;
                        var New_TS = new Transform(Adjust * ChildrenOfTileChild[j].Key);
                        ChildrenOfTileChild[j] = new KeyValuePair<Transform, Geom>(New_TS, ChildOfTileChild);
                    }

                    TileChildren[i].Value.Children = ChildrenOfTileChild;

                    Draw(TileChild, Level - 1, ref output);
                }
            }
            else if (Level == 1)
            {
                List<KeyValuePair<Transform, Geom>> TileChildren = Tile.Children;
                for (int i = 0; i < TileChildren.Count; i++)
                {
                    Geom TileChild = TileChildren[i].Value;
                    output.Add(TileChild.draw(TileChildren[i].Key));
                    TS.Add(TileChildren[i].Key);
                    this.Hat_Transform.Add(TileChildren[i].Key);
                    this.Hat_Labels.Add(((HatTile)TileChildren[i].Value).Label);
                }
            }
            return TS;
        }
    }
    public interface IPermutation
    {
        bool SetNewBlock(List<string> Names, bool overwrite);
        bool PlaceBlock(Einstein MonoTile);
        void NewSetPatterns(List<AddPatternOption> Options);
        List<Curve> PreviewShape();
    }
}
