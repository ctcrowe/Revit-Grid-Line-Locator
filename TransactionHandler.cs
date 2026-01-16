using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;

namespace TrackGridlineLocation
{
    public class TransactionHandler
    {
        public bool hideTransaction {get; set;}
        public string transactionName{get; set;}
        public TransactionHandler()
        {
            this.transactionName = "NONE";
            this.hideTransaction = false;
        }
        public void OnStartup(UIControlledApplication app)
        {
            app.ControlledApplication.FailuresProcessing += FailuresProcessing;
        }
        public void OnShutdown(UIControlledApplication app)
        {
            app.ControlledApplication.FailuresProcessing -= FailuresProcessing;
        }
        void FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            var accessor = e.GetFailuresAccessor();
            transactionName = accessor.GetTransactionName();
        }
        public bool CheckTransactions()
        {
            hideTransaction = transactionName.Equals("Reload Latest");
            hideTransaction = hideTransaction || transactionName.Equals("Synchronize with Central");
            hideTransaction = hideTransaction || transactionName.Equals("Update project to latest changes.");
            hideTransaction = hideTransaction || transactionName.Equals("Reload Linked Instances");
            hideTransaction = hideTransaction || transactionName.Equals("Fix bad fabrication part guids");
            hideTransaction = hideTransaction || transactionName.Equals("Snaps");
            hideTransaction = hideTransaction || transactionName.Equals("Keynote Settings");
            hideTransaction = hideTransaction || transactionName.Equals("Keynoting Settings");
            hideTransaction = hideTransaction || transactionName.Equals("Assembly Code Settings");
            hideTransaction = hideTransaction || transactionName.Equals("Fix Content Doc Tree");
            hideTransaction = hideTransaction || transactionName.Equals("Upgrade for external parameters");
            hideTransaction = hideTransaction || transactionName.Equals("Resolve Missing Servers");
            hideTransaction = hideTransaction || transactionName.Equals("Save macro elements");
            hideTransaction = hideTransaction || transactionName.Equals("Notify AppInfo Internal");
            hideTransaction = hideTransaction || transactionName.Equals("Preview");
            hideTransaction = hideTransaction || transactionName.Equals("Stop sharing");
            hideTransaction = hideTransaction || transactionName.Equals("Save Modified Doc");
            hideTransaction = hideTransaction || transactionName.Equals("Relativize links");
            hideTransaction = hideTransaction || transactionName.Equals("Precast Update Configuration");
            hideTransaction = hideTransaction || transactionName.Equals("Modify type attributes");
            hideTransaction = hideTransaction || transactionName.Equals("ContentADoc__UpdateReplicasInSmallDocStep");
            hideTransaction = hideTransaction || transactionName.Equals("Same Place");
            hideTransaction = hideTransaction || transactionName.Equals("Floor");
            hideTransaction = hideTransaction || transactionName.Equals("Edit Sketch");
            hideTransaction = hideTransaction || transactionName.Equals("Line - Rectangle");
            hideTransaction = hideTransaction || transactionName.Equals("Floor Plan...");
            hideTransaction = hideTransaction || transactionName.Equals("Default 3D View");
            hideTransaction = hideTransaction || transactionName.Equals("[Ideate Sticky - Get settings]");
            hideTransaction = hideTransaction || transactionName.Equals("Selection Box");
            hideTransaction = hideTransaction || transactionName.Equals("Duplicate View");
            hideTransaction = hideTransaction || transactionName.Equals("Project Setup");
            hideTransaction = hideTransaction || transactionName.Equals("Activate viewport");
            hideTransaction = hideTransaction || transactionName.Equals("Revisions");
            hideTransaction = hideTransaction || transactionName.Equals("Toggle Pin");
            hideTransaction = hideTransaction || transactionName.Equals("Unpin");
            hideTransaction = hideTransaction || transactionName.Equals("Toggle EQ");
            hideTransaction = hideTransaction || transactionName.Equals("Edit dimension length");
            hideTransaction = hideTransaction || transactionName.Equals("Section");
            hideTransaction = hideTransaction || transactionName.Equals("Worksets");
            hideTransaction = hideTransaction || transactionName.Equals("Sheet");
            hideTransaction = hideTransaction || transactionName.Equals("Select a Work Plane");
            hideTransaction = hideTransaction || transactionName.Equals("NONE");
            return hideTransaction;
        }
    }
}