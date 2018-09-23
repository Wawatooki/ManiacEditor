﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using ManiacEditor.Enums;
using RSDKv5;

namespace ManiacEditor
{
    class EditorEntities : IDrawable
    {
        public static bool FilterRefreshNeeded = false;
        public static int DefaultFilter = -1;

        List<EditorEntity> entities = new List<EditorEntity>();
        List<EditorEntity> selectedEntities = new List<EditorEntity>();
        List<EditorEntity> tempSelection = new List<EditorEntity>();

        Dictionary<ushort, EditorEntity> entitiesBySlot = new Dictionary<ushort, EditorEntity>();

        ushort nextFreeSlot = 0;

        public List<EditorEntity> Entities { get { return entities; } }
        public List<EditorEntity> SelectedEntities { get { return selectedEntities; } }

        public class TooManyEntitiesException : Exception { }

        public Actions.IAction LastAction;


        public EditorEntities(RSDKv5.Scene scene)
        {
            foreach (var obj in scene.Objects)
            {
                entities.AddRange(obj.Entities.Select(x => GenerateEditorEntity(x)));
            }
            FindDuplicateIds();
            entitiesBySlot = entities.ToDictionary(x => x.Entity.SlotID);
        }

        private void FindDuplicateIds()
        {
            var groupedById = entities.GroupBy(e => e.Entity.SlotID)
                                      .Where(g => g.Count()>1);
        }

        private ushort getFreeSlot(RSDKv5.SceneEntity preferred)
        {
            if (preferred != null && !entitiesBySlot.ContainsKey(preferred.SlotID)) return preferred.SlotID;
            while (entitiesBySlot.ContainsKey(nextFreeSlot))
            {
                ++nextFreeSlot;
            }
            if (nextFreeSlot == 2048)
            {
                if (entitiesBySlot.Count < 2048)
                {
                    // Next time search from beggining
                    nextFreeSlot = 0;
                }
                throw new TooManyEntitiesException();
            }
            return nextFreeSlot++;
        }

        public void Select(Rectangle area, bool addSelection = false, bool deselectIfSelected = false)
        {
            if (!addSelection) Deselect();
            foreach (var entity in entities)
            {
                if (entity.IsInArea(area))
                {
                    if (deselectIfSelected && selectedEntities.Contains(entity))
                    {
                        selectedEntities.Remove(entity);
                        entity.Selected = false;
                    }
                    else if (!selectedEntities.Contains(entity))
                    {
                        selectedEntities.Add(entity);
                        entity.Selected = true;
                    }
                }
            }
        }

        public void Select(Point point, bool addSelection = false, bool deselectIfSelected = false)
        {
            if (!addSelection) Deselect();
            // In reverse because we want to select the top one
            foreach (EditorEntity entity in entities.Reverse<EditorEntity>())
            {
                if (entity.ContainsPoint(point))
                {
                    if (deselectIfSelected && selectedEntities.Contains(entity))
                    {
                        selectedEntities.Remove(entity);
                        entity.Selected = false;
                    }
                    else
                    {
                        selectedEntities.Add(entity);
                        entity.Selected = true;
                    }
                    // Only the top
                    break;
                }
            }
        }

        public void SelectSlot(int slot)
        {
            Deselect();
            if (entitiesBySlot.ContainsKey((ushort)slot))
            {
                selectedEntities.Add(entitiesBySlot[(ushort)slot]);
                entitiesBySlot[(ushort)slot].Selected = true;
            }
        }

        private void AddEntities(List<EditorEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.Entity.Object.Entities.Add(entity.Entity);
                this.entities.Add(entity);
                entitiesBySlot[entity.Entity.SlotID] = entity;
            }
        }

        private void DeleteEntities(List<EditorEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.Entity.Object.Entities.Remove(entity.Entity);
                this.entities.Remove(entity);
                entitiesBySlot.Remove(entity.Entity.SlotID);
                if (entity.Entity.SlotID < nextFreeSlot) nextFreeSlot = entity.Entity.SlotID;
            }
        }

        private void DuplicateEntities(List<EditorEntity> entities)
        {
            if (null == entities || !entities.Any()) return;
            
            var new_entities = entities.Select(x => GenerateEditorEntity(new RSDKv5.SceneEntity(x.Entity, getFreeSlot(x.Entity)))).ToList();
            if (new_entities.Count > 0)
                LastAction = new Actions.ActionAddDeleteEntities(new_entities.ToList(), true, x => AddEntities(x), x => DeleteEntities(x));
            AddEntities(new_entities);
            Deselect();
            selectedEntities.AddRange(new_entities);
            foreach (var entity in new_entities)
                entity.Selected = true;
        }

        public void MoveSelected(Point oldPos, Point newPos, bool duplicate)
        {
            Point diff = new Point(newPos.X - oldPos.X, newPos.Y - oldPos.Y);
            if (duplicate) DuplicateEntities(selectedEntities);
            foreach (var entity in selectedEntities)
            {
                if (Editor.Instance.showGrid == false)
                    entity.Move(diff);
                else
                {
                    entity.Move(diff);
                    //entity.SnapToGrid(diff);
                }
            }
        }

        public bool IsSelected()
        {
            return selectedEntities.Count > 0 || tempSelection.Count > 0;
        }

        public void DeleteSelected()
        {
            if (selectedEntities.Count > 0)
                LastAction = new Actions.ActionAddDeleteEntities(selectedEntities.ToList(), false, x => AddEntities(x), x => DeleteEntities(x));
            DeleteEntities(selectedEntities);
            Deselect();
        }

        public List<EditorEntity> CopyToClipboard(bool keepPosition = false)
        {
            if (selectedEntities.Count == 0) return null;
            short minX = 0, minY = 0;

            List<EditorEntity> copiedEntities = selectedEntities.Select(x => GenerateEditorEntity(new RSDKv5.SceneEntity(x.Entity, x.Entity.SlotID))).ToList();
            if (!keepPosition)
            {
                minX = copiedEntities.Min(x => x.Entity.Position.X.High);
                minY = copiedEntities.Min(x => x.Entity.Position.Y.High);
                if (Editor.Instance.showGrid == false)
                    copiedEntities.ForEach(x => x.Move(new Point(-minX, -minY)));
                else
                    copiedEntities.ForEach(x => x.Move(new Point(-minX, -minY)));
                    //copiedEntities.ForEach(x => x.SnapToGrid(new Point(-minX, -minY)));
            }

            return copiedEntities;
        }

        public void PasteFromClipboard(Point newPos, List<EditorEntity> entities)
        {
            DuplicateEntities(entities);
            foreach (var entity in selectedEntities)
            {
                // Move them
                if (Editor.Instance.showGrid == false)
                    entity.Move(newPos);
                else
                    entity.Move(newPos);
                    //entity.SnapToGrid(newPos);
            }
        }

        public void RecreatedPasteFromClipboard(Point newPos, List<EditorEntity> entities)
        {
            DuplicateEntities(entities);
            foreach (var entity in selectedEntities)
            {
                // Move them
                if (Editor.Instance.showGrid == false)
                    entity.Move(newPos);
                else
                    entity.Move(newPos);
                //entity.SnapToGrid(newPos);
            }
        }

        public EditorEntity GetEntityAt(Point point)
        {
            foreach (EditorEntity entity in entities.Reverse<EditorEntity>())
                if (entity.ContainsPoint(point))
                    return entity;
            return null;
        }

        public void TempSelection(Rectangle area, bool deselectIfSelected)
        {
            List<EditorEntity> newSelection = (from entity in entities where entity.IsInArea(area) select entity).ToList();

            foreach (var entity in (from entity in tempSelection where !newSelection.Contains(entity) select entity))
            {
                entity.Selected = selectedEntities.Contains(entity);
            }

            tempSelection = newSelection;

            foreach (var entity in newSelection)
            {
                entity.Selected = !deselectIfSelected || !selectedEntities.Contains(entity);
            }
        }
        public void Deselect()
        {
            foreach (var entity in entities)
            {
                entity.Selected = false;
            }
            selectedEntities.Clear();
        }

        public void EndTempSelection()
        {
            tempSelection.Clear();
        }

        public void Draw(Graphics g)
        {

        }

        public void Draw(DevicePanel d)
        {
            if (FilterRefreshNeeded)
                UpdateViewFilters();
            foreach (var entity in entities)
                entity.Draw(d);
        }

        /// <summary>
        /// Creates a new instance of the given SceneObject at the indicated position.
        /// </summary>
        /// <param name="sceneObject">Type of SceneObject to create an instance of.</param>
        /// <param name="position">Location to insert into the scene.</param>
        public void Add(RSDKv5.SceneObject sceneObject, RSDKv5.Position position)
        {
            var editorEntity = GenerateEditorEntity(new RSDKv5.SceneEntity(sceneObject, getFreeSlot(null)));
            editorEntity.Entity.Position = position;
            var newEntities = new List<EditorEntity> { editorEntity };
            LastAction = new Actions.ActionAddDeleteEntities(newEntities, true, x => AddEntities(x), x => DeleteEntities(x));
            AddEntities(newEntities);

            Deselect();
            editorEntity.Selected = true;
            selectedEntities.Add(editorEntity);
        }

        private EditorEntity GenerateEditorEntity(RSDKv5.SceneEntity sceneEntity)
        {
            try
            {
                // ideally this would be driven by configuration...one day
                // or can we assume anything with a "Go" and "Tag" Attributes is linked to another?
                if (sceneEntity.Object.Name.ToString().Equals("WarpDoor", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new LinkedEditorEntity(sceneEntity);
                }
            }
            catch
            {
                Debug.WriteLine("Failed to generate a LinkedEditorEntity, will create a basic one instead.");
            }

            EditorEntity entity = new EditorEntity(sceneEntity);

            if (entity.HasFilter() && DefaultFilter > -1)
            {
                entity.Entity.GetAttribute("filter").ValueUInt8 = (byte)DefaultFilter;
                DefaultFilter = -1;
            }

            entity.SetFilter();

            return entity;
        }

        public void UpdateViewFilters()
        {
            FilterRefreshNeeded = false;
            foreach (EditorEntity entity in entities)
                entity.SetFilter();
        }
        internal void Flip(FlipDirection direction)
        {
            var positions = selectedEntities.Select(se => se.Entity.Position);
            IEnumerable<Position.Value> monoCoordinatePositions;
            if (direction == FlipDirection.Horizontal)
            {
                monoCoordinatePositions = positions.Select(p => p.X);
            }
            else
            {
                monoCoordinatePositions = positions.Select(p => p.Y);
            }

            short min = monoCoordinatePositions.Min(m => m.High);
            short max = monoCoordinatePositions.Max(m => m.High);
            int diff = max - min;

            foreach (var entity in selectedEntities)
            {
                if (direction == FlipDirection.Horizontal)
                {
                    short xPos = entity.Entity.Position.X.High;
                    int fromLeft = xPos - min;
                    int fromRight = max - xPos;

                    int newX = fromLeft < fromRight ? max - fromLeft : min + fromRight;
                    entity.Entity.Position.X.High = (short)newX;
                }
                else
                {
                    short yPos = entity.Entity.Position.Y.High;
                    int fromBottom = yPos - min;
                    int fromTop = max - yPos;

                    int newY = fromBottom < fromTop ? max - fromBottom : min + fromTop;
                    entity.Entity.Position.Y.High = (short)newY;
                }

                entity.Flip(direction);
            }
        }
    }
}
