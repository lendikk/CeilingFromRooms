#region Namespaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows;
#endregion
namespace AutoCeiling
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CeilingComand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            UIApplication m_uiApp = commandData.Application;

            // Initialize WPF 
            MainWindow mainWindow = new MainWindow(doc, uiDoc, m_uiApp);
            mainWindow.ShowDialog();

            return Result.Succeeded;
        }
        public class RoomSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element e)
            {
                return e is Room;
            }
            public bool AllowReference(Reference r, XYZ p)
            {
                return true;
            }
        }
    }
    public class ComboBoxViewModel
    {
        public List<string> TypeCollection { get; set; }
        public ComboBoxViewModel(Document doc)
        {
            FilteredElementCollector ceilingtypeCollector = new FilteredElementCollector(doc);
            List<Element> listCeilingElem = ceilingtypeCollector.OfCategory(BuiltInCategory.OST_Ceilings).WhereElementIsElementType().ToList();
            TypeCollection = new List<string>();

            foreach (Element e in listCeilingElem)
            {
                string ceilingName = e.Name.ToString();
                TypeCollection.Add(ceilingName);
            }
        }
    }
}
