
from Autodesk.Revit.DB.Architecture import *
from Autodesk.Revit.UI import *
from Autodesk.Revit.UI.Selection import *
from pyrevit import revit, DB
from Autodesk.Revit.DB import *
doc = __revit__.ActiveUIDocument.Document
uiDoc =  __revit__.ActiveUIDocument

class RoomSelectionFilter(ISelectionFilter):
    def AllowElement(self, e):
        if e.Category.Name == "Rooms":
            return True
        else:
            return False
    def AllowReference(self, ref, point):
        return False

filter = RoomSelectionFilter()
pickedRooms = uiDoc.Selection.PickObjects(ObjectType.Element,filter,"Select a rooms")            
t = Transaction(doc)
t.Start("Create Ceiling")

roomList = []
for reference in pickedRooms:
    room = doc.GetElement(reference)
    roomList.append(room)
    listCurves = CurveLoop()
    segments = room.GetBoundarySegments(SpatialElementBoundaryOptions())
    for sList in segments:
        for boundarySegment in sList:
            boundaryCurve = boundarySegment.GetCurve()
            listCurves.Append(boundaryCurve)
    
    ceilingtypeCollector = DB.FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Ceilings).WhereElementIsElementType().ToElements()

    
    for i in ceilingtypeCollector:
        print(i.Id)
        print(Element.Name.__get__(i))
    #Creation of celings 

    newCieling = Ceiling.Create(doc, [listCurves], ceilingtypeCollector[0].Id, room.LevelId) 
    newCieling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).SetValueString("3000") # Height of the ceiling 
    
t.Commit()