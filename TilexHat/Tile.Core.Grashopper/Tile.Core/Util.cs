using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;

namespace Tile.Core.Util
{
    public struct HatGroup<T>
    {
        public T Hat_H_ID { get; set; }
        public T Hat_H1_ID { get; set; }
        public T Hat_T_ID { get; set; }
        public T Hat_P_ID { get; set; }
        public T Hat_F_ID { get; set; }
        public T this[int ID]
        {
            get
            {
                switch (ID)
                {
                    case 0: return Hat_H_ID;
                    case 1: return Hat_H1_ID;
                    case 2: return Hat_T_ID;
                    case 3: return Hat_P_ID;
                    case 4: return Hat_F_ID;
                    default: throw new ArgumentOutOfRangeException(nameof(ID), "Index out of range");
                }
            }
            set
            {
                switch (ID)
                {
                    case 0:
                        Hat_H_ID = value;
                        break;
                    case 1:
                        Hat_H1_ID = value;
                        break;
                    case 2:
                        Hat_T_ID = value;
                        break;
                    case 3:
                        Hat_P_ID = value;
                        break;
                    case 4:
                        Hat_F_ID = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ID), "Index out of range");
                }
            }
        }
        public HatGroup((T, T, T, T, T) ID)
        {
            Hat_H_ID = ID.Item1;
            Hat_H1_ID = ID.Item2;
            Hat_T_ID = ID.Item3;
            Hat_P_ID = ID.Item4;
            Hat_F_ID = ID.Item5;
        }
        public HatGroup(T H_1, T H1_2, T T_2, T P_3, T F_4)
        {
            Hat_H_ID = H_1;
            Hat_H1_ID = H1_2;
            Hat_T_ID = T_2;
            Hat_P_ID = P_3;
            Hat_F_ID = F_4;
        }
        public int Length => 5;
        public delegate object SetAll(T Item, object sender = null);
        public bool SystemSetting(SetAll Set, out List<object> Result)
        {
            Result = new List<object>();
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Result.Add(Set(this[i], null));
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        public bool SystemSetting(SetAll Set, object Item, out List<object> Result)
        {
            Result = new List<object>();
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Result.Add(Set(this[i], Item));
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }
    public static partial class Util
    {
        public struct BlockIDandNameManager
        {
            public Label BlockLabel;
            public Dictionary<string, int> BlockIdAndName { get; private set; }
            public BlockIDandNameManager(Label label)
            {
                Index = 0;
                this.BlockLabel = label;
                this.BlockIdAndName = new Dictionary<string, int>();
            }
            public int Index { get; set; }
            public bool HasName(string Name, out int ID)
            {
                if (BlockIdAndName.Keys.ToList().Contains(Name))
                {
                    ID = BlockIdAndName[Name];
                    return true;
                }
                else
                {
                    ID = -1;
                    return false;
                }
            }
            public bool AddIDandName(string Name, TilePatterns A_PatternManager, out int ID)
            {
                var Doc = RhinoDoc.ActiveDoc;
                if (Doc.InstanceDefinitions.Find(Name) is null)
                {
                    ID = Doc.InstanceDefinitions.Add(Name,
                        this.BlockLabel.ToString() + "_Type",
                        Point3d.Origin,
                        A_PatternManager.Patterns,
                        A_PatternManager.PatternAtts);

                    this.BlockIdAndName.Add(Name, ID);
                    return true;
                }
                else
                {
                    ID = -1;
                    return false;
                }
            }
            public bool RemoveIDandName(string Name)
            {
                if (BlockIdAndName.ContainsKey(Name))
                {
                    var Doc = RhinoDoc.ActiveDoc;
                    Doc.InstanceDefinitions.Delete(BlockIdAndName[Name], true, true);
                    BlockIdAndName.Remove(Name);
                    return true;
                }
                else
                    return false;
            }
            public bool RemoveIDandName(int ID)
            {
                if (BlockIdAndName.ContainsValue(ID))
                {
                    var Doc = RhinoDoc.ActiveDoc;
                    Doc.InstanceDefinitions.Delete(ID, true, true);
                    string Name = BlockIdAndName.Where(x => x.Value == ID).Select(x => x.Key).ToList()[0];
                    this.BlockIdAndName.Remove(Name);
                    return true;
                }
                else
                    return false;
            }
            public void RemoveAll()
            {
                var NameList = this.BlockIdAndName.Keys.ToList();
                for (int i = 0; i < NameList.Count; i++)
                    this.RemoveIDandName(NameList[i]);
            }
        }
    }
    public static partial class Util
    {
        public static bool SaveOldHatBlock = false;
        public static HatGroup<BlockIDandNameManager> BlockManagers = new HatGroup<BlockIDandNameManager>(
            new BlockIDandNameManager(Label.H),
            new BlockIDandNameManager(Label.H1),
            new BlockIDandNameManager(Label.T),
            new BlockIDandNameManager(Label.P),
            new BlockIDandNameManager(Label.T)
            );
        public static void NewSetPatterns(List<AddPatternOption> Options, ref TilePatterns[] PatternsManager)
        {
            if (PatternsManager.Length != 5) return;
            for (int i = 0; i < Options.Count; i++)
            {
                TilePatterns TempPattern = new TilePatterns();
                Options[i].Get(out string Label, out _, out TempPattern.Frame, out _,out _, out _);
                switch (Label.ToLower())
                {
                    case "all":
                        PatternsManager[0] = SetPattern(Options[i], Core.Label.H);
                        PatternsManager[1] = SetPattern(Options[i], Core.Label.H1);
                        PatternsManager[2] = SetPattern(Options[i], Core.Label.T);
                        PatternsManager[3] = SetPattern(Options[i], Core.Label.P);
                        PatternsManager[4] = SetPattern(Options[i], Core.Label.F);
                        if (TempPattern.Frame)
                            NewSetFrame(ref PatternsManager);
                        break;
                    case "h":
                        PatternsManager[0] = SetPattern(Options[i], Core.Label.H);
                        if (TempPattern.Frame)
                            NewSetFrame(ref PatternsManager, Core.Label.H);
                        break;
                    case "h1":
                        PatternsManager[1] = SetPattern(Options[i], Core.Label.H1);
                        if (TempPattern.Frame)
                            NewSetFrame(ref PatternsManager, Core.Label.H1);
                        break;
                    case "t":
                        PatternsManager[2] = SetPattern(Options[i], Core.Label.T);
                        if (TempPattern.Frame)
                            NewSetFrame(ref PatternsManager, Core.Label.T);
                        break;
                    case "p":
                        PatternsManager[3] = SetPattern(Options[i], Core.Label.P);
                        if (TempPattern.Frame)
                            NewSetFrame(ref PatternsManager, Core.Label.P);
                        break;
                    case "f":
                        PatternsManager[4] = SetPattern(Options[i], Core.Label.F);
                        if (TempPattern.Frame)
                            NewSetFrame(ref PatternsManager, Core.Label.F);
                        break;
                }
            }
        }
        private static TilePatterns SetPattern(AddPatternOption Pattern, Core.Label label)
        {
            TilePatterns Tile = new TilePatterns();
            Pattern.Get(out _, out Tile.Patterns, out Tile.Frame, out Tile.PatternAtts,
                out Tile.ColourFromObject, out Tile.Guids);
            Tile.Patterns = Tile.Patterns.Select(x => x.Duplicate()).ToList();
            Tile.PatternAtts = Tile.PatternAtts.Select(x => x.Duplicate()).ToList();
            Tile.label = label;
            Tile.HasFrame = false;
            return Tile;
        }
        public static void NewSetFrame(ref TilePatterns[] PatternsManager)
        {
            for (int i = 0; i < PatternsManager.Length; i++)
            {
                if (PatternsManager[i].HasFrame) return;
                PatternsManager[i].Patterns.Add((new Einstein.HatTile("Outline")).PreviewShape);
                ObjectAttributes Att = new ObjectAttributes();
                Att.ColorSource = ObjectColorSource.ColorFromLayer;
                PatternsManager[i].PatternAtts.Add(Att);
                PatternsManager[i].Frame = true;
                PatternsManager[i].HasFrame = true;
            }
        }
        public static void NewSetFrame(ref TilePatterns[] PatternsManager, Core.Label label)
        {
            ObjectAttributes Att = new ObjectAttributes();
            int Index = (int)label;
            if (PatternsManager[Index].HasFrame) return;
            PatternsManager[Index].Patterns.Add((new Einstein.HatTile("Outline")).PreviewShape);
            Att = new ObjectAttributes();
            Att.ColorSource = ObjectColorSource.ColorFromLayer;
            PatternsManager[Index].PatternAtts.Add(Att);
            PatternsManager[Index].Frame = true;
            PatternsManager[Index].HasFrame = true;
        }
        public static void SetLayer()
        {
            Layer ParentLayer = null;
            Layer layer_H = null;
            Layer layer_H1 = null;
            Layer layer_T = null;
            Layer layer_P = null;
            Layer layer_F = null;
            var Doc = RhinoDoc.ActiveDoc;

            if (Doc.Layers.FindName("Hat") is null)
            {
                ParentLayer = new Layer();
                ParentLayer.Name = "Hat";
                Doc.Layers.Add(ParentLayer);
            }

            ParentLayer = Doc.Layers.FindName("Hat");

            if (Doc.Layers.FindName("Hat_H") is null)
            {
                layer_H = new Layer();
                layer_H.ParentLayerId = ParentLayer.Id;
                layer_H.Name = "Hat_H";
                layer_H.Color = Color.Blue;
                Doc.Layers.Add(layer_H);
            }
            if (Doc.Layers.FindName("Hat_H1") is null)
            {
                layer_H1 = new Layer();
                layer_H1.ParentLayerId = ParentLayer.Id;
                layer_H1.Name = "Hat_H1";
                layer_H1.Color = Color.LightBlue;
                Doc.Layers.Add(layer_H1);
            }
            if (Doc.Layers.FindName("Hat_T") is null)
            {
                layer_T = new Layer();
                layer_T.ParentLayerId = ParentLayer.Id;
                layer_T.Name = "Hat_T";
                layer_T.Color = Color.Gray;
                Doc.Layers.Add(layer_T);
            }
            if (Doc.Layers.FindName("Hat_P") is null)
            {
                layer_P = new Layer();
                layer_P.ParentLayerId = ParentLayer.Id;
                layer_P.Name = "Hat_P";
                layer_P.Color = Color.Green;
                Doc.Layers.Add(layer_P);
            }
            if (Doc.Layers.FindName("Hat_F") is null)
            {
                layer_F = new Layer();
                layer_F.ParentLayerId = ParentLayer.Id;
                layer_F.Name = "Hat_F";
                layer_F.Color = Color.Red;
                Doc.Layers.Add(layer_F);
            }
        }
        public static (int, int, int, int, int) FindBlock(List<string> Names)
        {
            int[] ReturnID = { -1, -1, -1, -1, -1 };
            if (Names.Count > 0)
            {
                for (int i = 0; i < BlockManagers.Length; i++)
                {
                    for (int j = 0; j < Names.Count; j++)
                    {
                        if (BlockManagers[i].HasName(Names[j], out int ID))
                        {
                            ReturnID[i] = ID;
                        }
                    }
                }
            }
            return (ReturnID[0], ReturnID[1], ReturnID[2], ReturnID[3], ReturnID[4]);
        }
        public static object _Remove(BlockIDandNameManager Manager, object Name)
        {
            if (Manager.HasName(Name.ToString(), out _))
            {
                return Manager.RemoveIDandName(Name.ToString());
            }
            else
                return false;
        }
        public static void Remove(List<string> Name)
        {

            for (int i = 0; i < Name.Count; i++)
            {
                BlockManagers.SystemSetting(_Remove, Name[i], out List<object> Return);
                bool Final = false;
                for (int j = 0; j < Return.Count; j++)
                {
                    Final |= (bool)Return[j];
                }
                //if(!Final)
                //    throw new Exception("Name isn't existed in the name list");
            }
        }
        public static (int, int, int, int, int) SetNewBlock(ref TilePatterns[]  PatternsManager, List<string> Names, bool Overwrite = false)
        {
            if (Names == null) Names = new List<string>();
            if (PatternsManager[0].Patterns.Count == 0)
                NewSetFrame(ref PatternsManager, Label.H);
            if (PatternsManager[1].Patterns.Count == 0)
                NewSetFrame(ref PatternsManager, Label.H1);
            if (PatternsManager[2].Patterns.Count == 0)
                NewSetFrame(ref PatternsManager, Label.T);
            if (PatternsManager[3].Patterns.Count == 0)
                NewSetFrame(ref PatternsManager, Label.P);
            if (PatternsManager[4].Patterns.Count == 0)
                NewSetFrame(ref PatternsManager, Label.F);
            var Doc = RhinoDoc.ActiveDoc;

            if (Overwrite)
            {
                string[] PrefixName = { "H_", "H1_", "T_", "P_", "F_" };
                for (int i = 0; i < BlockManagers.Length; i++)
                {
                    if (BlockManagers[i].Index != 0)
                    {
                        var Index = BlockManagers[i].Index - 1;
                        var BlockName = PrefixName[i] + Index.ToString();
                        BlockManagers[i].RemoveIDandName(BlockName);
                        var TempMB = BlockManagers[i];
                        TempMB.Index--;
                        BlockManagers[i] = TempMB;
                    }
                }
            }

            int[] ReturnID = { -1, -1, -1, -1, -1};

            if (Names.Count > 0)
            {
                for (int i = 0; i < BlockManagers.Length; i++)
                {
                    for (int j = 0; j < Names.Count; j++)
                    {
                        if (BlockManagers[i].HasName(Names[j], out int ID))
                        {
                            ReturnID[i] = ID;
                        }
                    }
                }
            }

            for (int i = 0; i < ReturnID.Length; i++)
            {
                if (ReturnID[i] == -1)
                    break;
                else if (i == 4)
                    return (ReturnID[0], ReturnID[1], ReturnID[2], ReturnID[3], ReturnID[4]);
            }


            //Set New Block
            
            SetLayer();
            
            string[] LayerName = { "Hat_H", "Hat_H1", "Hat_T", "Hat_P", "Hat_F" };
            for (int i = 0; i < PatternsManager.Length; i++)
            {
                for (int j = 0; j < PatternsManager[i].PatternAtts.Count; j++)
                {
                    int LayerIndex = Doc.Layers.FindName(LayerName[i]).Index;
                    PatternsManager[i].PatternAtts[j].LayerIndex = LayerIndex;
                    if (!PatternsManager[i].ColourFromObject)
                        PatternsManager[i].PatternAtts[j].ColorSource = ObjectColorSource.ColorFromLayer;
                    else
                        PatternsManager[i].PatternAtts[j].ColorSource = ObjectColorSource.ColorFromObject;
                }
            }

            var H_NewName = "H_" + BlockManagers[0].Index.ToString();
            var H1_NewName = "H1_" + BlockManagers[1].Index.ToString();
            var T_NewName = "T_" + BlockManagers[2].Index.ToString();
            var P_NewName = "P_" + BlockManagers[3].Index.ToString();
            var F_NewName = "F_" + BlockManagers[4].Index.ToString();

            string[] NewName = { H_NewName, H1_NewName, T_NewName, P_NewName, F_NewName };
            for (int i = 0; i < NewName.Length; i++)
            {
                if (ReturnID[i] == -1)
                {
                    if (BlockManagers[i].AddIDandName(NewName[i], PatternsManager[i], out int TempID))
                    {
                        ReturnID[i] = TempID;
                        var Temp = BlockManagers[i];
                        Temp.Index++;
                        BlockManagers[i] = Temp;
                    }
                    else
                        throw new Exception("The block is Existed.");
                }
            }

            return (ReturnID[0], ReturnID[1], ReturnID[2], ReturnID[3], ReturnID[4]);
        }
    }
}
