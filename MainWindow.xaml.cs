using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using static AutoCeiling.CeilingComand;
using Line = Autodesk.Revit.DB.Line;

namespace AutoCeiling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Document doc;
        public UIDocument uiDoc;
        public UIApplication m_uiApp;

        public MainWindow(Document document, UIDocument uidoc, UIApplication m_uiapp)
        {
            doc = document;
            uiDoc = uidoc;
            m_uiApp = m_uiapp;

            InitializeComponent();
            DataContext = new ComboBoxViewModel(doc);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // Selection of rooms 
            IList<Reference> pickedRoomsNew = null;
            
            try
            {
                this.Hide();
                ISelectionFilter filter = new RoomSelectionFilter();
                IList<Reference> pickedRooms = uiDoc.Selection.PickObjects(ObjectType.Element, filter, "Rooms");
                pickedRoomsNew = pickedRooms;
                this.Show();
            }
            catch (Exception)
            {
                this.Close();
            }          
            IList<Room> roomList = new List<Room>();
            Transaction t = new Transaction(doc);
            try
            {
                t.Start("Create Ceiling");
                foreach (Reference roomRef in pickedRoomsNew)
                {
                    Element elemRoom = doc.GetElement(roomRef);
                    Room room = elemRoom as Room;
                    roomList.Add(room);
                    // Store list of boudnary lines for each room
                    CurveLoop listCurves = new CurveLoop();
                    // Get curve of each room
                    IList<IList<BoundarySegment>> segments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                    try
                    {
                        foreach (IList<BoundarySegment> segmentList in segments)
                        {
                            foreach (BoundarySegment boundarySegment in segmentList)
                            {
                                if (boundarySegment.ElementId == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    Curve boundaryCurve = boundarySegment.GetCurve();
                                    Line boundaryLine = boundaryCurve as Line;
                                    listCurves.Append(boundaryLine);
                                }
                            }
                            break;
                        }
                        // Select the ceiling based on the the ceiling type chosen by user in WPF 
                        FilteredElementCollector ceilingtypeCollector = new FilteredElementCollector(doc);
                        var listCeilingElem = ceilingtypeCollector.OfCategory(BuiltInCategory.OST_Ceilings).WhereElementIsElementType();
                        CeilingType ceilingType = listCeilingElem.First(q => q.Name == this.textCeilingType.Text) as CeilingType;
                        // Creation of celings 

                        Ceiling ciel = Ceiling.Create(doc, new List<CurveLoop> { listCurves }, ceilingType.Id, room.LevelId);
                        ciel.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).SetValueString(this.textHeight.Text);
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show(ee.Message.ToString());
                        this.Close();
                    }
                }
            }
            catch (Exception)
            {}
            t.Commit();
            this.Close();
        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
