using System;
using System.IO;
using Autodesk.Revit.DB;

namespace TrackGridlineLocation
{
    public class GridParameter
    {
        internal static Guid ParameterGuid = new Guid("f4ef909ab6a2483da4e313ef8d600852");
        private static ForgeTypeId ParameterType = SpecTypeId.String.Text;
        internal const string ParameterName = "CLOSEST GRID";
        private const string localname = "CC_SharedParams.txt";
        private const string Group = "Automatic";
        
        private static string GetMyDocs(string Subdir)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string subdir = directory + "\\" + Subdir;
            return subdir;
        }
        public static DefinitionFile GetDefinitionFile(Document doc)
        {
            string fullName = GetMyDocs(localname);
            if(!File.Exists(fullName))
            {
                using(FileStream stream = File.Create(fullName))
                {
                    stream.Close();
                }
            }
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;
            app.SharedParametersFilename = fullName;
            return app.OpenSharedParameterFile();
        }
        public static Definition CreateDefinition(DefinitionFile df)
        {
            Definition def;
            DefinitionGroup group;
            group = df.Groups.get_Item(Group);
            if (group == null)
            {
                group = df.Groups.Create(Group);
                return cd(group);
            }
            def = group.Definitions.get_Item(ParameterName);
            if (def == null) { return cd(group); }
            return def;
        }
        private static Definition cd(DefinitionGroup group)
        {
            return group.Definitions.Create(
                new ExternalDefinitionCreationOptions(ParameterName, ParameterType)
                {
                    GUID = ParameterGuid,
                    UserModifiable = true
                });
        }
        public static bool Add(Document doc)
        {
            if (doc.IsFamilyDocument) return false;
            DefinitionFile df = GetDefinitionFile(doc);
            Definition def = CreateDefinition(df);
            if (!doc.ParameterBindings.Contains(def))
            {
                InstanceBinding b = new InstanceBinding();
                Categories categories = doc.Settings.Categories;
                foreach (Category cat in categories) { if (cat.AllowsBoundParameters && !cat.IsTagCategory) b.Categories.Insert(cat); }
                doc.ParameterBindings.Insert(def, b);
                return true;
            }
            return false;
        }
    }
}