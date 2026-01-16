using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace TrackGridlineLocation
{
    internal class GridlineUpdater : IUpdater
    {
        private string docTitle {get; set;}
        private Dictionary<ElementId, (string name, double p1x, double p1y, double  p2x, double p2y)> cachedXGrids {get; set;}
        private Dictionary<ElementId, (string name, double p1x, double p1y, double  p2x, double p2y)> cachedYGrids {get; set;}
        public void Execute(UpdaterData data)
        {
            if (handler.CheckTransactions()) return;
            try
            {
                Document doc = data.GetDocument();
                GridParameter.Add(doc);
                bool resetCache = doc.Title != docTitle;
                if(resetCache) setCache(doc);

                var addedGridIds = data.GetAddedElementIds().Where(id => doc.GetElement(id) is Grid).ToList();
                var modifiedGGridIds = data.GetModifiedElementIds().Where(id => doc.GetElement(id) is Grid).ToList();
                var deletedGridIds = data.GetDeletedElementIds();

                processDeletions(deletedGridIds);
                updateCache(doc, addedGridIds);
                updateCache(doc, modifiedGGridIds);

                if(resetCache) return;

                var added = data.GetAddedElementIds().Where(id => !(doc.GetElement(id) is Grid) && HasGridParameter(doc.GetElement(id)));
                var modified = data.GetModifiedElementIds().Where(id => !(doc.GetElement(id) is Grid) && HasGridParameter(doc.GetElement(id)));
                foreach (var eid in modified)
                {
                    var ele = doc.GetElement(eid);
                    if(ele is Grid) continue;
                    Options options = new Options();
                    options.ComputeReferences = true;
                    GeometryElement geo = ele.get_Geometry(options);
                    if (geo == null) continue;
                    setClosestGrid(ele);
                }
                foreach(var eid in added)
                {
                    var ele = doc.GetElement(eid);
                    if(ele is Grid) continue;
                    Options options = new Options();
                    options.ComputeReferences = true;
                    GeometryElement geo = ele.get_Geometry(options);
                    if (geo == null) continue;
                    setClosestGrid(ele);
                }
            }
            catch (Exception e)
            {
                Logger.Log(($"GridlineUpdater Error: {e.Message}\nStack: {e.StackTrace}"));
            }
        }
        private bool HasGridParameter(Element ele)
        {
            return ele?.get_Parameter(GridParameter.ParameterGuid) != null;
        }
        private void setCache(Document doc)
        {
            var grids = new FilteredElementCollector(doc)
                .OfClass(typeof(Grid))
                .WhereElementIsNotElementType()
                .ToElementIds();
            updateCache(doc, grids);
            docTitle = doc.Title;
        }
        private void processDeletions(ICollection<ElementId> IDs)
        {
            foreach(var id in IDs)
            {
                cachedXGrids?.Remove(id);
                cachedYGrids?.Remove(id);
            }
        }
        private void updateCache(Document doc, ICollection<ElementId> IDs)
        {
            if(IDs == null) return;

            var gridsToProcess = IDs.Select(id => doc.GetElement(id) as Grid).Where(g => g != null && g.Curve is Line);
            const double tolerance = 0.01;
            foreach(var grid in gridsToProcess)
            {
                var curve = grid.Curve as Line;
                var p1 = curve.GetEndPoint(0);
                var p2 = curve.GetEndPoint(1);
                double deltaX = Math.Abs(p2.X - p1.X);
                double deltaY = Math.Abs(p2.Y - p1.Y);

                var gridData = (grid.Name, p1.X, p1.Y, p2.X, p2.Y);
                cachedXGrids?.Remove(grid.Id);
                cachedYGrids?.Remove(grid.Id);
                if(deltaX > deltaY + tolerance) cachedXGrids[grid.Id] = gridData;
                else cachedYGrids[grid.Id] = gridData;
            }
        }
        public void setClosestGrid(Element ele)
        {
            Parameter par = ele.get_Parameter(GridParameter.ParameterGuid);
            if (par == null) return;
            string closestGridName = FindClosestGrid(ele);
            if (par.AsString() != closestGridName) par.Set(closestGridName);
        }
        public string FindClosestGrid(Element ele)
        {
            if (ele == null) return null;

            Options options = new Options();
            options.ComputeReferences = true;
            GeometryElement geo = ele.get_Geometry(options);
            if (geo == null) return null;

            var bbox = ele.get_BoundingBox(null);
            if (bbox == null) return null;

            double X = (bbox.Max.X + bbox.Min.X) / 2;
            double Y = (bbox.Max.Y + bbox.Min.Y) / 2;

            string xName = Closest(X, Y, cachedXGrids);
            string yName = Closest(X, Y, cachedYGrids);
            return $"{xName}{yName}";
        }
        private static string Closest(
            double X, double Y,
            Dictionary<ElementId, (string name, double p1x, double p1y, double  p2x, double p2y)> grids
            )
        {
            double min = double.MaxValue;
            string name = null;
            foreach (var g in grids.Values)
            {
                double dist = distanceBetween(X, Y, g);
                if (dist > min) continue;
                min = dist;
                name = g.name;
            }
            return name;            
        }
        private static double distanceBetween(double X, double Y, (string name, double x1, double y1, double x2, double y2) grid)
        {
            double abX = grid.x2 - grid.x1;
            double abY = grid.y2 - grid.y1;
            double apX = X - grid.x1;
            double apY = Y - grid.y1;
            double dot = (apX * abX) + (apY * abY);
            double ssl = (abX * abX) + (abY * abY);
            double t = Math.Max(Math.Min(dot / ssl, 1), 0);

            double cX = grid.x1 + (t * abX);
            double cY = grid.y1 + (t * abY);

            double dX = X - cX;
            double dY = Y - cY;
            return Math.Sqrt((dX * dX) + (dY * dY));
        }
        public void OnStartup(UIControlledApplication uiApp)
        {
            UpdaterRegistry.RegisterUpdater(this, true);
            
            ElementIsElementTypeFilter tf = new ElementIsElementTypeFilter(true);
            ElementCategoryFilter anno = new ElementCategoryFilter(BuiltInCategory.OST_Callouts, true);
            ElementCategoryFilter views = new ElementCategoryFilter(BuiltInCategory.OST_Views, true);
            ElementCategoryFilter viewers = new ElementCategoryFilter(BuiltInCategory.OST_Viewers, true);

            LogicalAndFilter filter = new LogicalAndFilter(new List<ElementFilter>() { tf, anno, views, viewers });
            UpdaterRegistry.AddTrigger(this.GetUpdaterId(), filter,  Element.GetChangeTypeAny());
            UpdaterRegistry.AddTrigger(this.GetUpdaterId(), filter,  Element.GetChangeTypeElementAddition());
        }
        public void OnShutdown(UIControlledApplication uiApp)
        {
            UpdaterRegistry.UnregisterUpdater(this.GetUpdaterId());
        }
        public GridlineUpdater(UIControlledApplication uiApp, TransactionHandler handler)
        {
            this.handler = handler;
            this.cachedXGrids ??= new Dictionary<ElementId, (string name, double p1x, double p1y, double p2x, double p2y)>();
            this.cachedYGrids ??= new Dictionary<ElementId, (string name, double p1x, double p1y, double p2x, double p2y)>();
            this.docTitle = "nothing";
            appId = uiApp.ActiveAddInId;
            updaterId = new UpdaterId(appId, guid);
        }
        TransactionHandler handler;
        private static Guid guid = new Guid("943ef8af-8f11-4dbd-b825-aa4e9e522a5c");
        static AddInId appId;
        static UpdaterId updaterId;
        public string GetAdditionalInformation() { return "";}
        public ChangePriority GetChangePriority() {return ChangePriority.Annotations;}
        public UpdaterId GetUpdaterId() {return updaterId;}
        public string GetUpdaterName() {return "Grid Tracker";}
    }
}