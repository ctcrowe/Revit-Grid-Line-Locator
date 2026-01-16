using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;

namespace TrackGridlineLocation

{
    public class Interface : IExternalApplication
    {
        internal static string dllpath = typeof(Interface).Assembly.Location;
        internal TransactionHandler handler;
        internal GridlineUpdater gridlineUpdater;
        public Result OnStartup(UIControlledApplication app)
        {
            handler = new TransactionHandler();
            gridlineUpdater = new GridlineUpdater(app, handler);
            gridlineUpdater.OnStartup(app);
            return Result.Succeeded;
        }
        
        public Result OnShutdown(UIControlledApplication app)
        {
            gridlineUpdater.OnShutdown(app);
            return Result.Succeeded;
        }
    }
}