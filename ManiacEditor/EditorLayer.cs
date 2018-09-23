﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using RSDKv5;
using SharpDX.Direct3D9;
using ManiacEditor.Actions;
using ManiacEditor.Enums;

namespace ManiacEditor
{
    public class EditorLayer : IDrawable
    {
        private SceneLayer _layer;
        internal SceneLayer Layer { get => _layer; }

        const int TILES_CHUNK_SIZE = 16;

        public const int TILE_SIZE = 16;

        public DevicePanel GraphicsPanel;


        Texture[][] TileChunksTextures;

        public PointsMap SelectedTiles;
        

        public Dictionary<Point, ushort> SelectedTilesValue = new Dictionary<Point, ushort>();

        public PointsMap TempSelectionTiles;
        public PointsMap TempSelectionDeselectTiles;
        bool TempSelectionDeselect;
        public int GlobalSelectedTiles;

        bool FirstDrag;
        bool isDragOver;

        public List<IAction> Actions = new List<IAction>();

        public string Name
        {
            get
            {
                string internalName = _layer.Name;
                return internalName?.TrimEnd('\0');
            }
            set
            {
                string name = value;
                if (name == null) name = "\0";
                if (!name.EndsWith("\0")) name += "\0";
                _layer.Name = name;
            }
        }

        public byte ScrollingVertical
        {
            get => _layer.IsScrollingVertical;
            set => _layer.IsScrollingVertical = value;
        }

        public byte UnknownByte2
        {
            get => _layer.UnknownByte2;
            set => _layer.UnknownByte2 = value;
        }

        public short UnknownWord1
        {
            get => _layer.UnknownWord1;
            set => _layer.UnknownWord1 = value;
        }

        public short UnknownWord2
        {
            get => _layer.UnknownWord2;
            set => _layer.UnknownWord2 = value;
        }

        public ushort Height { get => _layer.Height; }
        public ushort Width { get => _layer.Width; }
        public int HeightPixels { get => _layer.Height * TILE_SIZE; }
        public int WidthPixels { get => _layer.Height * TILE_SIZE; }

        /// <summary>
        /// Collection of rules and mappings representing the horizontal scrolling info
        /// and other rules affecting lines of pixels in this layer
        /// </summary>
        public IList<HorizontalLayerScroll> HorizontalLayerScroll { get => _horizontalLayerRules; }
        private IList<HorizontalLayerScroll> _horizontalLayerRules;

        static int DivideRoundUp(int number, int by)
        {
            return (number + by - 1) / by;
        }

        public class PointsMap
        {
            HashSet<Point>[][] PointsChunks;
            HashSet<Point> OutOfBoundsPoints = new HashSet<Point>();
            public int Count = 0;

            public PointsMap(int width, int height)
            {
                PointsChunks = new HashSet<Point>[DivideRoundUp(height, TILES_CHUNK_SIZE)][];
                for (int i = 0; i < PointsChunks.Length; ++i)
                {
                    PointsChunks[i] = new HashSet<Point>[DivideRoundUp(width, TILES_CHUNK_SIZE)];
                    for (int j = 0; j < PointsChunks[i].Length; ++j)
                        PointsChunks[i][j] = new HashSet<Point>();
                }
            }

            public void Add(Point point)
            {

                HashSet<Point> h;
                if (point.Y < 0 || point.X < 0 || point.Y / TILES_CHUNK_SIZE >= PointsChunks.Length || point.X / TILES_CHUNK_SIZE >= PointsChunks[0].Length)
                    h = OutOfBoundsPoints;
                else
                    h = PointsChunks[point.Y / TILES_CHUNK_SIZE][point.X / TILES_CHUNK_SIZE];
                Count -= h.Count;
                h.Add(point);
                Count += h.Count;
            }
            
            public void Remove(Point point)
            {
                HashSet<Point> h;
                if (point.Y < 0 || point.X < 0 || point.Y / TILES_CHUNK_SIZE >= PointsChunks.Length || point.X / TILES_CHUNK_SIZE >= PointsChunks[0].Length)
                    h = OutOfBoundsPoints;
                else
                    h = PointsChunks[point.Y / TILES_CHUNK_SIZE][point.X / TILES_CHUNK_SIZE];
                Count -= h.Count;
                h.Remove(point);
                Count += h.Count;
            }

            public bool Contains(Point point)
            {
                if (point.Y < 0 || point.X < 0 || point.Y / TILES_CHUNK_SIZE >= PointsChunks.Length || point.X / TILES_CHUNK_SIZE >= PointsChunks[0].Length)
                    return OutOfBoundsPoints.Contains(point);
                else
                    return PointsChunks[point.Y / TILES_CHUNK_SIZE][point.X / TILES_CHUNK_SIZE].Contains(point);
            }

            public bool IsChunkUsed(int x, int y)
            {
                return PointsChunks[y][x].Count > 0;
            }

            public void Clear()
            {
                for (int i = 0; i < PointsChunks.Length; ++i)
                    for (int j = 0; j < PointsChunks[i].Length; ++j)
                        PointsChunks[i][j].Clear();
                OutOfBoundsPoints.Clear();
                Count = 0;
            }

            public HashSet<Point> GetChunkPoint(int x, int y)
            {
                return PointsChunks[y][x];
            }

            public List<Point> PopAll()
            {
                List<Point> points = GetAll();
                Clear();
                return points;
            }

            public List<Point> GetAll()
            {
                List<Point> points = new List<Point>(Count);
                for (int i = 0; i < PointsChunks.Length; ++i)
                    for (int j = 0; j < PointsChunks[i].Length; ++j)
                        points.AddRange(PointsChunks[i][j]);
                points.AddRange(OutOfBoundsPoints);
                return points;
            }

            public void AddPoints(List<Point> points)
            {
                points.ForEach(point => Add(point));
            }


        }

        public EditorLayer(SceneLayer layer)
        {
            _layer = layer;

            TileChunksTextures = new Texture[DivideRoundUp(Height, TILES_CHUNK_SIZE)][];
            for (int i = 0; i < TileChunksTextures.Length; ++i)
                TileChunksTextures[i] = new Texture[DivideRoundUp(Width, TILES_CHUNK_SIZE)];

            SelectedTiles = new PointsMap(Width, Height);
            TempSelectionTiles = new PointsMap(Width, Height);
            TempSelectionDeselectTiles = new PointsMap(Width, Height);

            _horizontalLayerRules = ReadHorizontalLineRules();
        }

        /// <summary>
        /// Interpret the layer's set of horizontal scroll rules (ScrollInfo),
        /// and the line level map (ScrollIndexes) into that set of rules.
        /// </summary>
        /// <returns>List of HorizontalLayerScroll objects containing the scrolling rules</returns>
        private IList<HorizontalLayerScroll> ReadHorizontalLineRules()
        {
            var tempList = new List<HorizontalLayerScroll>();
            byte generatedId = 0;
            foreach (var scrollInfo in _layer.ScrollingInfo)
            {
                tempList.Add(new HorizontalLayerScroll(generatedId, scrollInfo));
                ++generatedId;
            }

            var ruleMapCount = _layer.ScrollIndexes.Count();
            int i = 0;
            while (i < ruleMapCount)
            {
                var currentValue = _layer.ScrollIndexes[i];
                var currentRule = _layer.ScrollingInfo[currentValue];
                var currentCount = 0;
                var start = i;
                while (   i < ruleMapCount 
                       && currentValue == _layer.ScrollIndexes[i])
                {
                    ++currentCount;
                    ++i;
                }

                tempList.First(hlr => hlr.Id == currentValue).AddMapping(start, currentCount);
            }

            return tempList;
        }

        /// <summary>
        /// Persist the contents of the HorizontalLayerRules collection,
        /// to the Layer's RSDKv5 backing objects and arrays.
        /// </summary>
        public void WriteHorizontalLineRules()
        {
            var newIndexes = new byte[_layer.ScrollIndexes.Length];
            _layer.ScrollingInfo = _horizontalLayerRules.Select(hlr => hlr.ScrollInfo).ToList();

            // the internal ID may now be inaccurate
            // we were only using it for display purposes anyway
            // generate some correct ones, and use those!
            byte newIndex = 0;
            foreach (var hlr in _horizontalLayerRules)
            {
                foreach (var lml in hlr.LinesMapList)
                {
                    var count = lml.LineCount;
                    int index = lml.StartIndex;
                    for (int i = 0; i < count; i++)
                    {
                        newIndexes[index + i] = newIndex;
                    }
                }
                ++newIndex;
            }

            _layer.ScrollIndexes = newIndexes;
        }

        /// <summary>
        /// Creates a new HorizontalLayerScroll object with backing ScrollInfo object.
        /// Adding it to the HorizontalLayerRules collection.
        /// </summary>
        public void ProduceHorizontalLayerScroll()
        {
            var id = (byte)(_horizontalLayerRules.Select(hlr => hlr.Id).Max() + 1);
            var info = new ScrollInfo();

            _layer.ScrollingInfo.Add(info);
            var hls = new HorizontalLayerScroll(id, info);
            _horizontalLayerRules.Add(hls);
        }

        public void StartDrag()
        {
            FirstDrag = true;
            RefreshTileCount();
        }

        public void StartDragOver(Point point, ushort value)
        {
            Deselect();
            isDragOver = true;
            DragOver(point, value);
            RefreshTileCount();
        }

        public void DragOver(Point point, ushort value)
        {
            SelectedTiles.Clear();
            SelectedTilesValue.Clear();
            point = new Point(point.X / TILE_SIZE, point.Y / TILE_SIZE);
            SelectedTiles.Add(point);
            SelectedTilesValue[point] = value;
            RefreshTileCount();
        }

        public void EndDragOver(bool remove)
        {
            if (isDragOver)
            {
                if (remove)
                {
                    SelectedTiles.Clear();
                    SelectedTilesValue.Clear();
                    RefreshTileCount();
                }
                isDragOver = false;
                RefreshTileCount();
            }
        }

        private void DetachSelected()
        {
            foreach (Point point in SelectedTiles.GetAll())
            {
                if (!SelectedTilesValue.ContainsKey(point))
                {
                    // Not moved yet
                    SelectedTilesValue[point] = _layer.Tiles[point.Y][point.X];
                    RemoveTile(point);
                    RefreshTileCount();
                }
            }
        }

        public void MoveSelected(Point oldPos, Point newPos, bool duplicate)
        {
            //if (Properties.Settings.Default.AllowMoreRenderUpdates == true) Editor.Instance.UpdateRender();
            oldPos = new Point(oldPos.X / TILE_SIZE, oldPos.Y / TILE_SIZE);
            newPos = new Point(newPos.X / TILE_SIZE, newPos.Y / TILE_SIZE);
            if (oldPos != newPos)
            {
                duplicate &= FirstDrag;
                FirstDrag = false;
                Dictionary<Point, ushort> newDict = new Dictionary<Point, ushort>();
                List<Point> newPoints = new List<Point>(SelectedTiles.Count);
                foreach (Point point in SelectedTiles.PopAll())
                {
                    Point newPoint = new Point(point.X + (newPos.X - oldPos.X), point.Y + (newPos.Y - oldPos.Y));
                    newPoints.Add(newPoint);
                    if (SelectedTilesValue.ContainsKey(point))
                    {
                        newDict[newPoint] = SelectedTilesValue[point];
                    }
                    else
                    {
                        // Not moved yet
                        newDict[newPoint] = _layer.Tiles[point.Y][point.X];
                        if (!duplicate) RemoveTile(point);
                    }
                }
                if (duplicate)
                {
                    Deselect();
                    // Create new actions group
                    Actions.Add(new ActionDummy());
                    RefreshTileCount();
                }
                SelectedTilesValue = newDict;
                SelectedTiles.AddPoints(newPoints);
                RefreshTileCount();
            }
        }

        public void MoveSelectedQuonta(Point change)
        {
            MoveSelected(Point.Empty, new Point(change.X * TILE_SIZE, change.Y * TILE_SIZE), false);
        }

        public void DeleteSelected()
        {
            bool removedSomething = SelectedTiles.Count > 0;
            foreach (Point p in SelectedTiles.PopAll())
            {
                // Remove only tiles that not moved, because we already removed the moved tiles
                if (!SelectedTilesValue.ContainsKey(p))
                {
                    RemoveTile(p);
                }
            }
            if (removedSomething)
            {
                Actions.Add(new ActionsGroupCloseMarker());
            }

            SelectedTilesValue.Clear();
            RefreshTileCount();

        }

        public void FlipPropertySelected(FlipDirection direction)
        {
            DetachSelected();
            List<Point> points = new List<Point>(SelectedTilesValue.Keys);

            if (points.Count == 0) return;

            if (points.Count == 1)
            {
                FlipIndividualTiles(direction, points);
                return;
            }

            IEnumerable<int> monoCoordinates;

            if (direction == FlipDirection.Horizontal)
            {
                monoCoordinates = points.Select(p => p.X);
            }
            else
            {
                monoCoordinates = points.Select(p => p.Y);
            }

            int min = monoCoordinates.Min();
            int max = monoCoordinates.Max();
            int diff = max - min;

            if (diff == 0)
            {
                FlipIndividualTiles(direction, points);
            }
            else
            {
                FlipGroupTiles(direction, points, min, max);
            }
        }

        private void FlipIndividualTiles(FlipDirection direction, IEnumerable<Point> points)
        {
            foreach (Point point in points)
            {
                SelectedTilesValue[point] ^= (ushort)direction;
            }
        }

        private void FlipGroupTiles(FlipDirection direction, IEnumerable<Point> points, int min, int max)
        {
            Dictionary<Point, ushort> workingTiles = new Dictionary<Point, ushort>();
            foreach (Point point in points)
            {
                ushort tileValue = SelectedTilesValue[point];
                Point newPoint; 

                if (direction == FlipDirection.Horizontal)
                {
                    int fromLeft = point.X - min;
                    int fromRight = max - point.X;

                    int newX = fromLeft < fromRight ? max - fromLeft : min + fromRight;
                    newPoint = new Point(newX, point.Y);
                }
                else
                {
                    int fromBottom = point.Y - min;
                    int fromTop = max - point.Y;

                    int newY = fromBottom < fromTop ? max - fromBottom : min + fromTop;
                    newPoint = new Point(point.X, newY);
                }

                workingTiles.Add(newPoint, tileValue ^= (ushort)direction);
            }

            SelectedTiles.Clear();
            SelectedTilesValue.Clear();
            SelectedTiles.AddPoints(workingTiles.Select(wt => wt.Key).ToList());
            SelectedTilesValue = workingTiles;
        }

        public void SetPropertySelected(int bit, bool state)
        {
            DetachSelected();

            List<Point> points = new List<Point>(SelectedTilesValue.Keys);
            foreach (Point point in points)
            {
                if (state)
                    SelectedTilesValue[point] |= (ushort)(1 << bit);
                else
                    SelectedTilesValue[point] &= (ushort)(~(1 << bit));
            }
        }

        public List<ushort> GetSelectedValues()
        {
            // Including temp selection
            List<ushort> selectedValues = new List<ushort>();
            foreach (Point point in SelectedTiles.GetAll())
            {
                if (TempSelectionDeselect && TempSelectionTiles.Contains(point)) continue;
                if (SelectedTilesValue.ContainsKey(point))
                {
                    selectedValues.Add(SelectedTilesValue[point]);
                }
                else
                {
                    // Not moved yet
                    selectedValues.Add(_layer.Tiles[point.Y][point.X]);
                }
            }
            foreach (Point point in TempSelectionTiles.GetAll())
            {
                if (SelectedTiles.Contains(point)) continue;
                selectedValues.Add(_layer.Tiles[point.Y][point.X]);
            }
            RefreshTileCount();
            return selectedValues;
        }

        public Dictionary<Point, ushort> CopyToClipboard(bool keepPosition = false)
        {
            if (SelectedTiles.Count == 0) return null;
            int minX = 0, minY = 0;

            Dictionary<Point, ushort> copiedTiles = new Dictionary<Point, ushort>(SelectedTilesValue);;
            foreach (Point point in SelectedTiles.GetAll())
            {
                if (!copiedTiles.ContainsKey(point))
                {
                    // Not moved yet
                    copiedTiles[point] = GetTile(point);
                }
            }
            if (!keepPosition)
            {
                minX = copiedTiles.Keys.Min(x => x.X);
                minY = copiedTiles.Keys.Min(x => x.Y);
            }
            return copiedTiles.ToDictionary(x => new Point(x.Key.X - minX, x.Key.Y - minY), x => x.Value);
        }

        public void PasteFromClipboard(Point newPos, Dictionary<Point, ushort> points)
        {
            newPos = new Point(newPos.X / TILE_SIZE, newPos.Y / TILE_SIZE);
            Deselect();
            foreach (KeyValuePair<Point, ushort> point in points)
            {
                Point tilePos = new Point(point.Key.X + newPos.X, point.Key.Y + newPos.Y);
                SelectedTiles.Add(tilePos);
                SelectedTilesValue[tilePos] = point.Value;
            }
            // Create new actions group
            Actions.Add(new ActionDummy());
            RefreshTileCount();
        }

        public void Select(Rectangle area, bool addSelection = false, bool deselectIfSelected = false)
        {
            if (!addSelection) Deselect();
            for (int y = Math.Max(area.Y / TILE_SIZE, 0); y < Math.Min(DivideRoundUp(area.Y + area.Height, TILE_SIZE), _layer.Height); ++y)
            {
                for (int x = Math.Max(area.X / TILE_SIZE, 0); x < Math.Min(DivideRoundUp(area.X + area.Width, TILE_SIZE), _layer.Width); ++x)
                {
                    if (addSelection || deselectIfSelected)
                    {
                        Point p = new Point(x, y);
                        if (SelectedTiles.Contains(p))
                        {
                            if (deselectIfSelected)
                            {
                                // Deselect
                                DeselectPoint(p);
                                RefreshTileCount();
                            }
                            // Don't add already selected tile, or if it was just deslected
                            continue;
                        }
                    }
                    if (_layer.Tiles[y][x] != 0xffff)
                    {
                        SelectedTiles.Add(new Point(x, y));
                        RefreshTileCount();
                    }
                }
            }
        }

        public void Select(Point point, bool addSelection = false, bool deselectIfSelected = false)
        {
            if (!addSelection) Deselect();
            point = new Point(point.X / TILE_SIZE, point.Y / TILE_SIZE);
            Editor.Instance.SelectedTileX = point.X;
            Editor.Instance.SelectedTileY = point.Y;
            if (point.X >= 0 && point.Y >= 0 && point.X < this._layer.Tiles[0].Length && point.Y < this._layer.Tiles.Length)
            {
                if (deselectIfSelected && SelectedTiles.Contains(point))
                {
                    // Deselect
                    DeselectPoint(point);
                    RefreshTileCount();
                }
                else if (this._layer.Tiles[point.Y][point.X] != 0xffff)
                {
                    // Just add the point
                    SelectedTiles.Add(point);
                    RefreshTileCount();
                }
            }
        }

        public void TempSelection(Rectangle area, bool deselectIfSelected)
        {
            TempSelectionTiles.Clear();
            TempSelectionDeselectTiles.Clear();
            TempSelectionDeselect = deselectIfSelected;
            for (int y = Math.Max(area.Y / TILE_SIZE, 0); y < Math.Min(DivideRoundUp(area.Y + area.Height, TILE_SIZE), _layer.Height); ++y)
            {
                for (int x = Math.Max(area.X / TILE_SIZE, 0); x < Math.Min(DivideRoundUp(area.X + area.Width, TILE_SIZE), _layer.Width); ++x)
                {
                    if (SelectedTiles.Contains(new Point(x, y)) || _layer.Tiles[y][x] != 0xffff)
                    {
                        TempSelectionTiles.Add(new Point(x, y));
                        if (SelectedTiles.Contains(new Point(x, y)) && TempSelectionTiles.Contains(new Point(x, y)))
                        {
                            TempSelectionDeselectTiles.Add(new Point(x, y));
                        }
                        RefreshTileCount();


                    }
                }
            }
        }

        public void EndTempSelection()
        {
            TempSelectionTiles.Clear();
            TempSelectionDeselectTiles.Clear();
            RefreshTileCount();
        }

        private void InvalidateChunk(int x, int y)
        {
            TileChunksTextures[y][x]?.Dispose();
            TileChunksTextures[y][x] = null;
        }

        private ushort GetTile(Point point)
        {
            return _layer.Tiles[point.Y][point.X];
            
        }

        private void SetTile(Point point, ushort value, bool addAction = true)
        {
            if (addAction)
                Actions.Add(new ActionChangeTile((x, y) => SetTile(x, y, false), point, _layer.Tiles[point.Y][point.X], value));
            _layer.Tiles[point.Y][point.X] = value;
            InvalidateChunk(point.X / TILES_CHUNK_SIZE, point.Y / TILES_CHUNK_SIZE);
        }

        private void RemoveTile(Point point)
        {
            SetTile(point, 0xffff);
            RefreshTileCount();
        }

        private void DeselectPoint(Point p)
        {
            if (SelectedTilesValue.ContainsKey(p))
            {
                // Or else it wasn't moved at all
                SetTile(p, SelectedTilesValue[p]);
                SelectedTilesValue.Remove(p);
            }
            SelectedTiles.Remove(p);
        }

        public void Deselect()
        {
            bool hasTiles = SelectedTilesValue.Count > 0;
            foreach (KeyValuePair<Point, ushort> point in SelectedTilesValue)
            {
                // ignore out of bounds
                if (point.Key.X < 0 || point.Key.Y < 0 || point.Key.Y >= _layer.Height || point.Key.X >= _layer.Width) continue;
                SetTile(point.Key, point.Value);
            }
            if (hasTiles)
                Actions.Add(new ActionsGroupCloseMarker());

            SelectedTiles.Clear();
            SelectedTilesValue.Clear();
            Editor.Instance.SelectedTilesCount = 0;
        }

        public bool IsPointSelected(Point point)
        {
            return SelectedTiles.Contains(new Point(point.X / TILE_SIZE, point.Y / TILE_SIZE));
        }

        public bool HasTileAt(Point point)
        {
            point = new Point(point.X / TILE_SIZE, point.Y / TILE_SIZE);
            if (point.X >= 0 && point.Y >= 0 && point.X < this._layer.Tiles[0].Length && point.Y < this._layer.Tiles.Length)
            {
                return _layer.Tiles[point.Y][point.X] != 0xffff;
            }
            return false;
        }

        public ushort GetTileAt(Point point)
        {
            point = new Point(point.X / TILE_SIZE, point.Y / TILE_SIZE);
            if (point.X >= 0 && point.Y >= 0 && point.X < this._layer.Tiles[0].Length && point.Y < this._layer.Tiles.Length)
            {
                if (SelectedTilesValue.ContainsKey(point)) return SelectedTilesValue[point];
                else return _layer.Tiles[point.Y][point.X];
            }
            return 0xffff;
        }

        private Rectangle GetTilesChunkArea(int x, int y)
        {
            int y_start = y * TILES_CHUNK_SIZE;
            int y_end = Math.Min((y + 1) * TILES_CHUNK_SIZE, _layer.Height);

            int x_start = x * TILES_CHUNK_SIZE;
            int x_end = Math.Min((x + 1) * TILES_CHUNK_SIZE, _layer.Width);


            return new Rectangle(x_start, y_start, x_end - x_start, y_end - y_start);
        }

        public void DrawTile(DevicePanel d, ushort tile, int x, int y, bool selected, int Transperncy)
        {
            bool flipX = ((tile >> 10) & 1) == 1;
            bool flipY = ((tile >> 11) & 1) == 1;
            if (Properties.Settings.Default.UseFasterSelectionRendering == true)
            {
                selected = false;
            }
            d.DrawBitmap(Editor.Instance.StageTiles.Image.GetTexture(d._device, new Rectangle(0, (tile & 0x3ff) * TILE_SIZE, TILE_SIZE, TILE_SIZE), flipX, flipY),
x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE, selected, Transperncy);

            if (Editor.Instance.showTileID == true)
            {
                d.DrawBitmap(Editor.Instance.StageTiles.IDImage.GetTexture(d._device, new Rectangle(0, (tile & 0x3ff) * TILE_SIZE, TILE_SIZE, TILE_SIZE), false, false),
                x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE, selected, Transperncy);
            }
            if (selected)
            {
                if (Properties.Settings.Default.UseFasterSelectionRendering == false)
                {
                    d.DrawLine(x * TILE_SIZE, y * TILE_SIZE, x * TILE_SIZE + TILE_SIZE, y * TILE_SIZE, System.Drawing.Color.Brown);
                    d.DrawLine(x * TILE_SIZE, y * TILE_SIZE, x * TILE_SIZE, y * TILE_SIZE + TILE_SIZE, System.Drawing.Color.Brown);
                    d.DrawLine(x * TILE_SIZE + TILE_SIZE, y * TILE_SIZE + TILE_SIZE, x * TILE_SIZE + TILE_SIZE, y * TILE_SIZE, System.Drawing.Color.Brown);
                    d.DrawLine(x * TILE_SIZE + TILE_SIZE, y * TILE_SIZE + TILE_SIZE, x * TILE_SIZE, y * TILE_SIZE + TILE_SIZE, System.Drawing.Color.Brown);
                }
                else
                {
                    d.DrawBitmap(Editor.Instance.StageTiles.EditorImage.GetTexture(d._device, new Rectangle(0, (1 & 0x3ff) * TILE_SIZE, TILE_SIZE, TILE_SIZE), false, false), x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE, false, Transperncy);
                }

            }
        }
        public void DrawTile(Graphics g, ushort tile, int x, int y)
        {
            ushort TileIndex = (ushort)(tile & 0x3ff);
            int TileIndexInt = (int)TileIndex;
            bool flipX = ((tile >> 10) & 1) == 1;
            bool flipY = ((tile >> 11) & 1) == 1;
            bool SolidTopA = ((tile >> 12) & 1) == 1;
            bool SolidLrbA = ((tile >> 13) & 1) == 1;
            bool SolidTopB = ((tile >> 14) & 1) == 1;
            bool SolidLrbB = ((tile >> 15) & 1) == 1;
          
            g.DrawImage(Editor.Instance.StageTiles.Image.GetBitmap(new Rectangle(0, TileIndex * TILE_SIZE, TILE_SIZE, TILE_SIZE), flipX, flipY),
                new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE));
            if (Editor.Instance.showCollisionA == true)
            {
                if (SolidLrbA || SolidTopA)
                {
                    //Get a bitmap of the collision
                    Bitmap cm = Editor.Instance.CollisionLayerA[TileIndexInt].Clone(new Rectangle(0, 0, 16, 16), System.Drawing.Imaging.PixelFormat.DontCare);

                    if (SolidTopA && !SolidLrbA)
                    {
                        for (int ix = 0; ix < cm.Width; ix++)
                        {
                            for (int iy = 0; iy < cm.Height; iy++)
                            {
                                System.Drawing.Color gotColor = cm.GetPixel(ix, iy);
                                if (gotColor == System.Drawing.Color.FromArgb(255, 255, 255, 255))
                                {
                                    cm.SetPixel(ix, iy, System.Drawing.Color.FromArgb(255, 255, 0));
                                }
                            }
                        }
                    }//Change Colour if Solidity = Top

                    if (SolidLrbA && !SolidTopA)
                    {
                        for (int ix = 0; ix < cm.Width; ix++)
                        {
                            for (int iy = 0; iy < cm.Height; iy++)
                            {
                                System.Drawing.Color gotColor = cm.GetPixel(ix, iy);
                                if (gotColor == System.Drawing.Color.FromArgb(255, 255, 255, 255))
                                {
                                    cm.SetPixel(ix, iy, System.Drawing.Color.FromArgb(255, 0, 0));
                                }
                            }
                        }
                    } //Change Colour if Solidity = All But Top

                    if (flipX) { cm.RotateFlip(RotateFlipType.RotateNoneFlipX); }

                    if (flipY) { cm.RotateFlip(RotateFlipType.RotateNoneFlipY); }

                    g.DrawImage(cm, new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE));
                }
            }
            if (Editor.Instance.showCollisionB == true)
            {
                if (SolidLrbB || SolidTopB)
                {
                    //Get a bitmap of the collision
                    Bitmap cm = Editor.Instance.CollisionLayerB[TileIndex].Clone(new Rectangle(0, 0, 16, 16), System.Drawing.Imaging.PixelFormat.DontCare);

                    if (SolidTopB && !SolidLrbB)
                    {
                        for (int ix = 0; ix < cm.Width; ix++)
                        {
                            for (int iy = 0; iy < cm.Height; iy++)
                            {
                                System.Drawing.Color gotColor = cm.GetPixel(ix, iy);
                                if (gotColor == System.Drawing.Color.FromArgb(255, 255, 255, 255))
                                {
                                    cm.SetPixel(ix, iy, System.Drawing.Color.FromArgb(255, 255, 0));
                                }
                            }
                        }
                    }//Change Colour if Solidity = Top

                    if (SolidLrbB && !SolidTopB)
                    {
                        for (int ix = 0; ix < cm.Width; ix++)
                        {
                            for (int iy = 0; iy < cm.Height; iy++)
                            {
                                System.Drawing.Color gotColor = cm.GetPixel(ix, iy);
                                if (gotColor == System.Drawing.Color.FromArgb(255, 255, 255, 255))
                                {
                                    cm.SetPixel(ix, iy, System.Drawing.Color.FromArgb(255, 0, 0));
                                }
                            }
                        }
                    } //Change Colour if Solidity = All But Top

                    if (flipX) { cm.RotateFlip(RotateFlipType.RotateNoneFlipX); }

                    if (flipY) { cm.RotateFlip(RotateFlipType.RotateNoneFlipY); }

                    g.DrawImage(cm, new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE));
                }
            }

            if (Editor.Instance.showTileID == true)
            {
                g.DrawImage(Editor.Instance.StageTiles.IDImage.GetBitmap(new Rectangle(0, TileIndex * TILE_SIZE, TILE_SIZE, TILE_SIZE), false, false),
                            new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE));
            }


        }



        public void Draw(Graphics g)
        {
            for (int y = 0; y < _layer.Height; ++y)
            {
                for (int x = 0; x < _layer.Width; ++x)
                {
                    if (this._layer.Tiles[y][x] != 0xffff)
                    {
                        DrawTile(g, _layer.Tiles[y][x], x, y);
                    }
                }
            }
        }

        private Texture GetTilesChunkTexture(DevicePanel d, int x, int y)
        {
            if (this.TileChunksTextures[y][x] != null) return this.TileChunksTextures[y][x];

            Rectangle rect = GetTilesChunkArea(x, y);

            using (Bitmap bmp = new Bitmap(rect.Width * TILE_SIZE, rect.Height * TILE_SIZE, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    for (int ty = rect.Y; ty < rect.Y + rect.Height; ++ty)
                    {
                        for (int tx = rect.X; tx < rect.X + rect.Width; ++tx)
                        {
                            if (this._layer.Tiles[ty][tx] != 0xffff)
                            {
                                DrawTile(g, _layer.Tiles[ty][tx], tx - rect.X, ty - rect.Y);
                                //DrawTile(g, _layer.Tiles[y][x], x, y);
                            }
                        }
                    }
                }
                this.TileChunksTextures[y][x] = TextureCreator.FromBitmap(d._device, bmp);
            }

            return this.TileChunksTextures[y][x];
        }

        private void DrawTilesChunk(DevicePanel d, int x, int y, int Transperncy)
        {
            Rectangle rect = GetTilesChunkArea(x, y);

            for (int ty = rect.Y; ty < rect.Y + rect.Height; ++ty)
            {
                for (int tx = rect.X; tx < rect.X + rect.Width; ++tx)
                {
                    Point p = new Point(tx, ty);
                    // We will draw those later
                    if (SelectedTiles.Contains(p) || TempSelectionTiles.Contains(p))
                    {
                        continue;
                    }
                    if (this._layer.Tiles[ty][tx] != 0xffff)
                    {
                        DrawTile(d, this._layer.Tiles[ty][tx], tx, ty, false, Transperncy);
                    }
                }
            }
        }

        private void DrawSelectedTiles(DevicePanel d, int x, int y, int Transperncy)
        {
            foreach (Point p in SelectedTiles.GetChunkPoint(x, y))
                if (SelectedTilesValue.ContainsKey(p))
                    DrawTile(d, SelectedTilesValue[p], p.X, p.Y, !TempSelectionDeselect || !TempSelectionTiles.Contains(p), Transperncy);


                else // It is still in the original place
                    DrawTile(d, _layer.Tiles[p.Y][p.X], p.X, p.Y, !TempSelectionDeselect || !TempSelectionTiles.Contains(p), Transperncy);

            foreach (Point p in TempSelectionTiles.GetChunkPoint(x, y)) {
                if (SelectedTiles.Contains(p))
                {
                    continue;
                }
                DrawTile(d, _layer.Tiles[p.Y][p.X], p.X, p.Y, true, Transperncy);
            }
        }

        public void Draw(DevicePanel d)
        {
            int Transperncy = (Editor.Instance.EditLayer != null && Editor.Instance.EditLayer != this) ? 0x32 : 0xFF;

            Rectangle screen = d.GetScreen();
            
            int start_x = screen.X / (TILES_CHUNK_SIZE * TILE_SIZE);
            int end_x = Math.Min(DivideRoundUp(screen.X + screen.Width, TILES_CHUNK_SIZE * TILE_SIZE), TileChunksTextures[0].Length);
            int start_y = screen.Y / (TILES_CHUNK_SIZE * TILE_SIZE);
            int end_y = Math.Min(DivideRoundUp(screen.Y + screen.Height, TILES_CHUNK_SIZE * TILE_SIZE), TileChunksTextures.Length);
            for (int y = start_y; y < end_y; ++y)
            {
                for (int x = start_x; x < end_x; ++x)
                {
                    Rectangle rect = GetTilesChunkArea(x, y);
                    if (SelectedTiles.IsChunkUsed(x, y) || TempSelectionTiles.IsChunkUsed(x, y))
                    {
                        // TODO: If the full chunk isDrawTilesChunk selected, cache it
                        // draw one by one
                        DrawTilesChunk(d, x, y, Transperncy);
                    }
                    else
                    {
                        d.DrawBitmap(GetTilesChunkTexture(d, x, y), rect.X * TILE_SIZE, rect.Y * TILE_SIZE, rect.Width * TILE_SIZE, rect.Height * TILE_SIZE, false, Transperncy);
                    }
                                        DrawSelectedTiles(d, x, y, Transperncy);
                    
                }
            }
        }

        /// <summary>
        /// Resizes both this EditorLayer, and the underlying SceneLayer
        /// </summary>
        /// <param name="width">The new width of the layer</param>
        /// <param name="height">The new height of the layer</param>
        public void Resize(ushort width, ushort height)
        {
            ushort oldWidth = Width;
            ushort oldHeight = Height;

            // first resize the underlying SceneLayer
            _layer.Resize(width, height);

            int oldWidthChunkSize = DivideRoundUp(oldWidth, TILES_CHUNK_SIZE);
            int newWidthChunkSize = DivideRoundUp(Width, TILES_CHUNK_SIZE);

            // now resize ourselves
            Array.Resize(ref TileChunksTextures, DivideRoundUp(Height, TILES_CHUNK_SIZE));
            for (int i = DivideRoundUp(oldHeight, TILES_CHUNK_SIZE); i < TileChunksTextures.Length; i++)
            {
                TileChunksTextures[i] = new Texture[oldWidthChunkSize];
            }

            for (int i = 0; i < TileChunksTextures.Length; i++)
            {
                Array.Resize(ref TileChunksTextures[i], newWidthChunkSize);
            }

            
            SelectedTiles = new PointsMap(Width, Height);
            TempSelectionTiles = new PointsMap(Width, Height);
        }

        public void Dispose()
        {
            foreach (Texture[] textures in TileChunksTextures)
                foreach (Texture texture in textures)
                    if (texture != null)
                        texture.Dispose();
            TileChunksTextures = null;
        }

        public void DisposeTextures()
        {
            foreach (Texture[] textures in TileChunksTextures)
            {
                for (int i = 0; i < textures.Length; ++i)
                {
                    if (textures[i] != null)
                    {
                        textures[i].Dispose();
                        textures[i] = null;
                    }
                }
            }
        }

        public void RefreshTileCount()
        {
            GlobalSelectedTiles = SelectedTiles.Count + TempSelectionTiles.Count;
            Editor.Instance.DeselectTilesCount = TempSelectionDeselectTiles.Count;
            Editor.Instance.SelectedTilesCount = GlobalSelectedTiles - Editor.Instance.DeselectTilesCount;
        }
    }
}
