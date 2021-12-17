/* Copyright (C) GraphicDNA - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Iñaki Ayucar <iayucar@simax.es>, September 2016
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 * IN THE SOFTWARE.
 */
#if(UNITY_EDITOR)
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GraphicDNA.SplineStudio
{
    [CustomEditor(typeof(Spline))]
    [CanEditMultipleObjects]
    public class SplineCustomEditor : Editor
    {
        private bool foldoutControlPoints;
        private bool foldoutCreation;
        //private SerializedProperty ColorProperty;
        //private SerializedProperty WidthProperty;
        private bool subObjectMode = false;
        private Vector2 startMouseRect;
        private bool manualCreationModeAdd = false;
        private bool manualCreationModeRefine = false;
        private bool manualAttachMode = false;
        private Spline manualAttachModeSelectedSpline = null;
        private ControlPoint.eControlPointMode controlModeForAllPointsSelection;
        private bool mouseDown = false;
        private bool selectingByRect = false;
        private bool ReadyForClone = true;
        private Rect selectionRect;
        private HashSet<ControlPoint> selectedSubEntities = new HashSet<ControlPoint>();
        private int creationOptionSelected = 0;
        private float creationStraightLen = 50;
        private float creationCurveRadius = 50;
        private float creationCurveTotalDegs = 90;
        private float creationCurveCamberDegs = 6;
        private float creationClothoidLen = 90;
        private bool creationClothoidIsLeft = true;
        private bool creationClothoidIsEntry = true;
        private int creationInterpolationSteps = 10;
        private bool mMultiSelection = false;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            //ColorProperty = serializedObject.FindProperty("Color");
            //WidthProperty = serializedObject.FindProperty("Width");

            Spline bb = (serializedObject.targetObject as Spline);
            if (bb != null)
                bb.Refresh();

            UnityEditor.Undo.undoRedoPerformed += MyUndoCallback;
            UnityEditor.Selection.selectionChanged += SelectionChanged;
        }
        /// <summary>
        /// 
        /// </summary>
        void SelectionChanged()
        {
            subObjectMode = false;
            Tools.hidden = false;
        }
        /// <summary>
        /// 
        /// </summary>
        void MyUndoCallback()
        {
            Spline bb = (serializedObject.targetObject as Spline);
            if (bb != null)
                bb.Refresh();
        }

        #region Selected SubEntities
        /// <summary>
        /// 
        /// </summary>
        private void SelectAllSubEntities(Spline obj)
        {
            if (subObjectMode)
            {
                SelectedSubEntitiesClear();
                for (int i = 0; i < obj.ControlPoints.Count; i++)
                    SelectedSubEntitiesAdd(obj.ControlPoints[i]);
            }
            else
            {
                SelectedSubEntitiesClear();
                SelectedSubEntitiesAddRange(obj.ControlPoints);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void SelectedSubEntitiesClear()
        {
            selectedSubEntities.Clear();
            SelectedSubEntitiesChanged();
        }
        /// <summary>
        /// 
        /// </summary>
        private void SelectedSubEntitiesAdd(ControlPoint b)
        {
            selectedSubEntities.Add(b);
            SelectedSubEntitiesChanged();
        }
        /// <summary>
        /// 
        /// </summary>
        private void SelectedSubEntitiesRemove(ControlPoint b)
        {
            if (selectedSubEntities.Contains(b))
                selectedSubEntities.Remove(b);
            SelectedSubEntitiesChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SelectedSubEntitiesAddRange(IEnumerable<ControlPoint> objs)
        {
            foreach (ControlPoint bb in objs)
            {
                if (!selectedSubEntities.Contains(bb))
                    selectedSubEntities.Add(bb);
            }

            SelectedSubEntitiesChanged();
        }
        /// <summary>
        /// 
        /// </summary>
        private void SelectedSubEntitiesChanged()
        {
            Repaint();
        }
        #endregion

        #region OnInspector GUI
        /// <summary>
        /// 
        /// </summary>
        private bool OnInspectorGUI_Creation(Spline bb)
        {
            bool anyChange = false;
            //float buttonHeight = 24;
            bool oldSubObject = subObjectMode;
            string[] optionsEntryExit = new string[] { "Entry", "Exit" };
            string[] optionsLeftRight = new string[] { "Left", "Right" };
            int curveEntryInt;
            int curveDirInt;


            GUILayout.Space(5);
            string[] options = new string[] { "Straight", "Constant Radius Curve", "Clothoid Curve", "Full Curve" };
            creationOptionSelected = EditorGUILayout.Popup("Segment Type", creationOptionSelected, options);

            switch (creationOptionSelected)
            {
                case 0:
                    // Straight
                    GUILayout.Label("Adds a straight segment to the spline", UnityTextures.italicTextStyle);
                    GUILayout.Space(5);

                    UITools.IntField(serializedObject, "Num Steps", creationInterpolationSteps, (obj, value) => { creationInterpolationSteps = value; });
                    UITools.FloatField(serializedObject, "Length", creationStraightLen, (obj, value) => { creationStraightLen = value; });
                    break;
                case 1:
                    // Constant Rad Curve
                    GUILayout.Label("Adds a curve segment with constant radius and camber");
                    GUILayout.Space(5);
                    UITools.IntField(serializedObject, "Num Steps", creationInterpolationSteps, (obj, value) => { creationInterpolationSteps = value; });
                    UITools.FloatField(serializedObject, "Radius", creationCurveRadius, (obj, value) => { creationCurveRadius = value; });
                    UITools.FloatField(serializedObject, "Total Degs", creationCurveTotalDegs, (obj, value) => { creationCurveTotalDegs = value; });
                    UITools.Slider(serializedObject, "Camber (degs)", creationCurveCamberDegs, 0, 180, (obj, value) => { creationCurveCamberDegs = value; });
                    break;
                case 2:
                    // Clothoid Curve
                    GUILayout.Label("Adds a Clothoid curve segment (with adaptive radius\nand camber)");
                    GUILayout.Space(5);

                    UITools.IntField(serializedObject, "Num Steps", creationInterpolationSteps, (obj, value) => { creationInterpolationSteps = value; });

                    curveEntryInt = creationClothoidIsEntry ? 0 : 1;
                    curveEntryInt = EditorGUILayout.Popup("Clothoid Type", curveEntryInt, optionsEntryExit);
                    creationClothoidIsEntry = curveEntryInt == 0;                    
                    curveDirInt = creationClothoidIsLeft ? 0 : 1;
                    curveDirInt = EditorGUILayout.Popup("Direction", curveDirInt, optionsLeftRight);
                    creationClothoidIsLeft = curveDirInt == 0;

                    UITools.FloatField(serializedObject, "Length", creationClothoidLen, (obj, value) => { creationClothoidLen = value; });
                    UITools.FloatField(serializedObject, "Radius", creationCurveRadius, (obj, value) => { creationCurveRadius = value; });
                    UITools.Slider(serializedObject, "Camber (degs)", creationCurveCamberDegs, 0, 180, (obj, value) => { creationCurveCamberDegs = value; });
                    break;
                case 3:
                    // Full Curve
                    GUILayout.Label("Adds a Full curve segment (with clothoid entry/exit\nand constant radius segment in the middle)");
                    GUILayout.Space(5);

                    UITools.IntField(serializedObject, "Num Steps", creationInterpolationSteps, (obj, value) => { creationInterpolationSteps = value; });

                    curveDirInt = creationClothoidIsLeft ? 0 : 1;
                    curveDirInt = EditorGUILayout.Popup("Direction", curveDirInt, optionsLeftRight);
                    creationClothoidIsLeft = curveDirInt == 0;

                    UITools.FloatField(serializedObject, "Clothoid Parts Length", creationClothoidLen, (obj, value) => { creationClothoidLen = value; });
                    UITools.FloatField(serializedObject, "Radius", creationCurveRadius, (obj, value) => { creationCurveRadius = value; });
                    UITools.FloatField(serializedObject, "Total Degs", creationCurveTotalDegs, (obj, value) => { creationCurveTotalDegs = value; });
                    UITools.Slider(serializedObject, "Camber (degs)", creationCurveCamberDegs, 0, 180, (obj, value) => { creationCurveCamberDegs = value; });
                    break;
            }


            //GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            anyChange |= UITools.Toggle(serializedObject, "Append to existing", bb.AppendPointsOnCreation, (obj, value) => { (obj as Spline).AppendPointsOnCreation = value; });
            if(GUILayout.Button("Create"))
            {
                bool creationAllowed = true;
                if (!bb.AppendPointsOnCreation)
                    creationAllowed = (EditorUtility.DisplayDialog("Warning", "Append to Existing option is not checked. If you continue, all current control points will be replaced with the new ones. Continue?", "Yes", "No"));

                if (creationAllowed)
                {
                    Undo.RegisterCompleteObjectUndo(bb, "Add Spline Segment");

                    switch (creationOptionSelected)
                    {
                        case 0:
                            // Straight
                            bb.BuildStraight(Vector3.forward, creationStraightLen, creationInterpolationSteps, bb.AppendPointsOnCreation);
                            break;
                        case 1:                            
                            // Constant Rad Curve
                            bb.BuildCurve(creationCurveRadius, creationCurveTotalDegs, creationCurveCamberDegs, creationInterpolationSteps, bb.AppendPointsOnCreation);
                            break;
                        case 2:
                            // Clothoid Curve
                            bb.BuildClothoid(creationClothoidLen, creationClothoidIsLeft, creationClothoidIsEntry, creationCurveRadius, creationCurveCamberDegs, creationInterpolationSteps, bb.AppendPointsOnCreation);
                            break;
                        case 3:
                            // Full Curve
                            bb.BuildFullCurve(creationClothoidLen, creationCurveTotalDegs, creationClothoidIsLeft, creationCurveRadius, creationCurveCamberDegs, creationInterpolationSteps, creationInterpolationSteps, bb.AppendPointsOnCreation);
                            break;
                    }

                    anyChange = true;

                    bb.ResetHandles(0);
                    bb.ResetHandles(bb.ControlPoints.Count - 1);
                }
            }
            GUILayout.EndHorizontal();

            return anyChange;
        }
        /// <summary>
        /// 
        /// </summary>
        private bool OnInspectorGUI_Buttons(Spline bb)
        {
            bool anyChange = false;
            float buttonHeight = 24;
            bool oldSubObject = subObjectMode;
            EditorGUI.BeginDisabledGroup(serializedObject.targetObjects.Length > 1);

            #region First Row
            EditorGUILayout.BeginHorizontal();
            subObjectMode = GUILayout.Toggle(subObjectMode, new GUIContent("Sub-Object", UnityTextures.mSubObjectModeIcon, "Allows selecting and manipulating control points"), "Button", GUILayout.Height(buttonHeight));
            if (oldSubObject != subObjectMode)
            {
                if (subObjectMode)
                {
                    //multipleCreationMode = false;
                }
                else
                {
                    manualCreationModeRefine = false;
                    manualCreationModeAdd = false;
                    manualAttachMode = false;
                    //selectByTypeMode = false;
                }
            }
            bool result = GUILayout.Toggle(manualCreationModeAdd, new GUIContent("  Add ", UnityTextures.mAddKeyFrameIcon, "Add new control points at the end of the spline by clicking on the scene view"), "Button", GUILayout.Height(buttonHeight));
            if (result != manualCreationModeAdd)
            {
                manualCreationModeAdd = result;
                if (manualCreationModeAdd)
                {
                    //multipleCreationMode = false;
                    //selectByTypeMode = false;
                    subObjectMode = true;
                    manualCreationModeRefine = false;
                    manualAttachMode = false;
                }
            }
            EditorGUI.BeginDisabledGroup(bb == null || bb.ControlPoints.Count < 2);
            result = GUILayout.Toggle(manualCreationModeRefine, new GUIContent(" Refine ", UnityTextures.mAddEventIcon, "Refine segments by by clicking on the scene view"), "Button", GUILayout.Height(buttonHeight));
            if (result != manualCreationModeRefine)
            {
                manualCreationModeRefine = result;
                if (manualCreationModeRefine)
                {
                    //multipleCreationMode = false;
                    //selectByTypeMode = false;
                    subObjectMode = true;
                    manualCreationModeAdd = false;
                    manualAttachMode = false;
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(serializedObject.targetObjects.Length > 1 || bb == null || (subObjectMode && selectedSubEntities.Count == 0));
            if (GUILayout.Button(new GUIContent(" Delete", UnityTextures.mDeleteIcon), GUILayout.Height(buttonHeight)))
            {
                if (subObjectMode)
                {
                    if (selectedSubEntities.Count == 0)
                    {
                        EditorUtility.DisplayDialog("Error", "No control points selected", "Ok");
                    }
                    else if (EditorUtility.DisplayDialog("Warning", string.Format("({0}) control points will be deleted. Continue?", selectedSubEntities.Count), "Yes", "No"))
                    {
                        foreach (ControlPoint pt in selectedSubEntities)
                            bb.ControlPoints.Remove(pt);
                        anyChange = true;
                        selectedSubEntities.Clear();
                    }

                }
                else
                {
                    if (EditorUtility.DisplayDialog("Warning", "All control points will be deleted. Continue?", "Yes", "No"))
                    {
                        bb.ControlPoints.Clear();
                        anyChange = true;
                        selectedSubEntities.Clear();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Second Row
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(bb == null || bb.ControlPoints.Count < 2 || subObjectMode);
            if (GUILayout.Button(new GUIContent("    x2 Tessellate   "), GUILayout.Height(buttonHeight)))
            {
                Undo.RegisterCompleteObjectUndo(bb, "Tessellate x2");
                bb.TessellateControlPointsx2();
                anyChange = true;
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(bb == null || subObjectMode);
            result = GUILayout.Toggle(manualAttachMode, new GUIContent("   Attach   ", UnityTextures.mEditColliderIcon, "Attach one curve to the end of this one"), "Button", GUILayout.Height(buttonHeight));
            //if (GUILayout.Button(new GUIContent("   Attach   ", UnityTextures.mEditColliderIcon), GUILayout.Height(buttonHeight)))
            if (result != manualAttachMode)
            {
                manualAttachMode = result;
                if (manualAttachMode)
                {
                    manualAttachModeSelectedSpline = null;
                    manualCreationModeAdd = false;
                    manualCreationModeRefine = false;
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(bb == null || bb.ControlPoints.Count < 2 || !subObjectMode || selectedSubEntities.Count < 2);
            if (GUILayout.Button(new GUIContent("  Detach     ", UnityTextures.mVerticalSplitIcon), GUILayout.Height(buttonHeight)))
            {
                Undo.RegisterCompleteObjectUndo(bb, "Detach Points");

                GameObject newGO = GameObject.Instantiate(bb.gameObject);

                Spline newSpline = newGO.GetComponent<Spline>();
                newSpline.ControlPoints.Clear();
                foreach (ControlPoint pt in bb.ControlPoints.ToArray())
                {
                    if (selectedSubEntities.Contains(pt))
                    {
                        bb.ControlPoints.Remove(pt);
                        newSpline.ControlPoints.Add(pt);
                    }

                }

                // To make the splines still connected, must clone one of the control points (the last of the previous spline, and the first of the new one)
                bb.ControlPoints.Add(newSpline.ControlPoints[0].Clone());

                bb.ResetHandles(0);
                bb.ResetHandles(bb.ControlPoints.Count - 1);
                newSpline.ResetHandles(0);
                newSpline.ResetHandles(newSpline.ControlPoints.Count - 1);

                anyChange = true;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(bb == null || bb.ControlPoints.Count < 2 || subObjectMode);
            if (GUILayout.Button(new GUIContent("Reverse", UnityTextures.mReverseIcon), GUILayout.Height(buttonHeight)))
            {
                Undo.RegisterCompleteObjectUndo(bb, "Reverse Points");
                Vector3 handleOutFirstPoint = bb.ControlPoints[0].ControlHandleOut;
                Vector3 handleInLastPoint = bb.ControlPoints[bb.ControlPoints.Count - 1].ControlHandleIn;
                bb.ControlPoints.Reverse();

                // Interchange handles between first and last point
                bb.ControlPoints[0].ControlHandleIn = Vector3.zero;
                bb.ControlPoints[0].ControlHandleOut = Vector3.zero;
                bb.ControlPoints[bb.ControlPoints.Count - 1].ControlHandleIn = Vector3.zero;
                bb.ControlPoints[bb.ControlPoints.Count - 1].ControlHandleOut = Vector3.zero;
                bb.ControlPoints[0].ControlHandleOut = handleInLastPoint;
                bb.ControlPoints[bb.ControlPoints.Count - 1].ControlHandleIn = handleOutFirstPoint;

                for (int i = 1; i < bb.ControlPoints.Count - 1; i++)
                    bb.ControlPoints[i].SwapHandles();
                anyChange = true;
            }
            EditorGUI.EndDisabledGroup();
            //if (GUILayout.Button(new GUIContent(" Control Mode", UnityTextures.mEditColliderIcon), GUILayout.Height(buttonHeight)))
            //{

            //}
            EditorGUILayout.EndHorizontal();
            #endregion

            EditorGUI.EndDisabledGroup();



            if (manualCreationModeAdd)
            {
                EditorGUILayout.BeginVertical(UnityTextures.rectStyle);
                GUILayout.Space(5);
                bb.mManualModeAddPlaneHeight = EditorGUILayout.FloatField("New Points Height: ", bb.mManualModeAddPlaneHeight);
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
            }
            else if (manualAttachMode)
            {
                EditorGUILayout.BeginVertical(UnityTextures.rectStyle);
                GUILayout.Space(7);
                EditorGUILayout.BeginHorizontal();
                manualAttachModeSelectedSpline = (Spline)EditorGUILayout.ObjectField("Spline To Attach", manualAttachModeSelectedSpline, typeof(Spline), true);
                EditorGUI.BeginDisabledGroup(manualAttachModeSelectedSpline.IsNull());
                if (GUILayout.Button("Attach", GUILayout.Width(50)))
                {
                    Undo.RecordObjects(new Object[] { bb, manualAttachModeSelectedSpline }, "Attach control points");
                    foreach (ControlPoint pt in manualAttachModeSelectedSpline.ControlPoints)
                    {
                        ControlPoint newCP = pt.Clone();
                        newCP.Position = bb.transform.InverseTransformPoint(manualAttachModeSelectedSpline.transform.TransformPoint(newCP.Position));
                        bb.ControlPoints.Add(newCP);
                    }
                    Undo.DestroyObjectImmediate(manualAttachModeSelectedSpline.gameObject);

                    bb.RemoveDuplicatePoints();

                    anyChange = true;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
            }

            return anyChange;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLabel"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        public void OnInspectorGUIControlPointsHeader()
        {
            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.LabelField("Idx", UnityTextures.boldTextStyle, GUILayout.Width(60));
            EditorGUILayout.LabelField("Control Mode", UnityTextures.boldTextStyle, GUILayout.MinWidth(40));

            EditorGUILayout.LabelField("Position XYZ", UnityTextures.boldTextStyle, GUILayout.Width(90));
            EditorGUILayout.LabelField("Camber", UnityTextures.boldTextStyleRightAlign, GUILayout.Width(70));

            EditorGUILayout.EndHorizontal();
            GUILayout.Box(GUIContent.none, UnityTextures.mlineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLabel"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        public bool OnInspectorGUIControlPoint(ControlPoint cp, int idx)
        {
            bool anyChange = false;
            EditorGUILayout.BeginHorizontal();

            string label = "";
            if (subObjectMode && selectedSubEntities.Contains(cp))
                label = string.Format("#{0}*", idx);
            else label = string.Format("#{0}", idx);
            
            // 2019.09.05 Added functionallity to select control points via the GUI
            //EditorGUILayout.LabelField(label, GUILayout.Width(60));
            if (GUILayout.Button(label, GUILayout.Width(40)))
            {
                bool selected = selectedSubEntities.Contains(cp);
                if (!mMultiSelection)
                {
                    SelectedSubEntitiesClear();
                }
                else if (selected)
                {
                    SelectedSubEntitiesRemove(cp);
                }

                if (!selected)
                {
                    SelectedSubEntitiesAdd(cp);
                }
            }

            anyChange |= UITools.EnumField(serializedObject, "", cp.ControlPointMode, (obj, value) => { cp.ControlPointMode = (ControlPoint.eControlPointMode)value; }, GUILayout.MinWidth(70));

            //EditorGUILayout.LabelField("Position:", GUILayout.Width(50));
            float x = EditorGUILayout.FloatField(cp.Position.x, GUILayout.Width(50));
            //EditorGUILayout.LabelField(",", GUILayout.Width(8));
            float y = EditorGUILayout.FloatField(cp.Position.y, GUILayout.Width(50));
            //EditorGUILayout.LabelField(",", GUILayout.Width(8));
            float z = EditorGUILayout.FloatField(cp.Position.z, GUILayout.Width(50));
            EditorGUILayout.LabelField(",", GUILayout.Width(8));
            float camber = EditorGUILayout.FloatField(cp.Camber, GUILayout.Width(50));

            if (x != cp.Position.x || y != cp.Position.y || z != cp.Position.z)
            {
                cp.Position = new Vector3(x, y, z);
                anyChange = true;
            }
            if (camber != cp.Camber)
            {
                cp.Camber = camber;
                anyChange = true;
            }

            //if(newOptionSelected != optionSelected)
            //{ 
            //    cp.ControlPointMode = (ControlPoint.eControlPointMode)newOptionSelected;
            //    anyChange = true;
            //}

            EditorGUILayout.EndHorizontal();

            return anyChange;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Important: If current event is an Undo, do nothing to prevent unity crash
            if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
                return;

            serializedObject.Update();
            UnityTextures.LoadStaticResources();
            Spline bb = (serializedObject.targetObject as Spline);

            GUI.backgroundColor = new Color(0.91f, 0.91f, 0.91f);
            bool anyChange = false;

          


            GUILayout.Space(10);
            string subObjStr = string.Format("({0} Control Points / {1} Selected)", bb.ControlPoints.Count.ToString(), selectedSubEntities.Count);
            GUILayout.BeginHorizontal();
            GUILayout.Label(subObjStr, UnityTextures.italicTextStyle);
            GUILayout.Label(string.Format("Curve Length: {0} m", bb.TotalLength.ToString("f2")), UnityTextures.italicTextStyleRightAlign);
            GUILayout.EndHorizontal();
            anyChange |= OnInspectorGUI_Buttons(bb);


            GUILayout.Space(5);
            if (serializedObject.targetObjects.Length == 1 && selectedSubEntities.Count > 0)
            {
                //UITools.Vector3Field(serializedObject, "Position", selectedSubEntities.First().Position, (obj, value) => { (obj as Spline) });
            }

            GUILayout.Space(5);
            anyChange |= UITools.EnumField(serializedObject, "Curve Type", bb.CurveType, (obj, value) => { (obj as Spline).CurveType = (Spline.eSplineType)value; });
            anyChange |= UITools.Toggle(serializedObject, "Closed", bb.IsClosed, (obj, value) => { (obj as Spline).IsClosed = value; });
            anyChange |= UITools.Toggle(serializedObject, "Control Points Visible", bb.AlwaysRenderControlPoints, (obj, value) => { (obj as Spline).AlwaysRenderControlPoints = value; });
            anyChange |= UITools.FloatField(serializedObject, "Initial Handle Dist", bb.InitialHandleDist, (obj, value) => { (obj as Spline).InitialHandleDist = value; });

            if (bb.CurveType == Spline.eSplineType.Bezier)
            {
                anyChange |= UITools.Toggle(serializedObject, "Edit Handles XZ", bb.EditHandlesXZ, (obj, value) => { (obj as Spline).EditHandlesXZ = value; });
                anyChange |= UITools.Toggle(serializedObject, "Edit Handles Y", bb.EditHandlesY, (obj, value) => { (obj as Spline).EditHandlesY = value; });
            }

            GUILayout.Space(10);
            GUILayout.Label("Interpolation", UnityTextures.boldTextStyle);
            anyChange |= UITools.EnumField(serializedObject, "Interpolation Type", bb.InterpolationType, (obj, value) => { (obj as Spline).InterpolationType = (Spline.eInterpolationType)value; });

            if (bb.InterpolationType == Spline.eInterpolationType.FixedNumSteps)
                anyChange |= UITools.IntField(serializedObject, "# Steps", bb.InterpolationSteps, (obj, value) => { (obj as Spline).InterpolationSteps = value; });
            else anyChange |= UITools.FloatField(serializedObject, "World Step", bb.InterpolationWorldStep, (obj, value) => { (obj as Spline).InterpolationWorldStep = value > 0 ? value : 1f; });

            GUILayout.Space(10);
            GUILayout.Label("Rendering", UnityTextures.boldTextStyle);
            anyChange |= UITools.MaterialField(serializedObject, "Material", bb.Material, (obj, value) => { (obj as Spline).Material = value; });

            bool colortintChanged = UITools.ColorField(serializedObject, "Color Tint", bb.Color, (obj, value) => { (obj as Spline).Color = value; });
            anyChange |= colortintChanged;
            if (colortintChanged)
            {
                foreach (Spline sp in serializedObject.targetObjects)
                {
                    if (!sp.Material.IsNull())
                        sp.Material.color = bb.Color;
                    if (!sp.DefaultMaterial.IsNull())
                        sp.mDefaultMaterial.color = bb.Color;
                }
            }

            anyChange |= UITools.FloatField(serializedObject, "Width", bb.Width, (obj, value) => { (obj as Spline).Width = value; });

            bool visibilityChanged = UITools.EnumField(serializedObject, "Visibility", (System.Enum)bb.Visibility, (obj, value) => { (obj as Spline).Visibility = (Spline.eVisibility)value; });
            anyChange |= visibilityChanged;
            bool renderModechanged = UITools.EnumField(serializedObject, "Render Mode", (System.Enum)bb.RenderMode, (obj, value) => { (obj as Spline).RenderMode = (Spline.eRenderType)value; });
            anyChange |= renderModechanged;
            if (renderModechanged || visibilityChanged)
            {
                foreach (Spline sp in serializedObject.targetObjects)
                {
                    sp.RefreshRenderObjs();

                    sp.Refresh();
                }

            }
            if (bb.RenderMode == Spline.eRenderType.CustomMesh)
            {
                anyChange |= UITools.FloatField(serializedObject, "Thickness", bb.Thickness, (obj, value) => { (obj as Spline).Thickness = value; });
                EditorGUI.BeginDisabledGroup(bb.Thickness <= 0);
                anyChange |= UITools.Toggle(serializedObject, "Generate Sides Geom", bb.GenerateSidesGeometry, (obj, value) => { (obj as Spline).GenerateSidesGeometry = value; });
                EditorGUI.EndDisabledGroup();

                anyChange |= UITools.Toggle(serializedObject, "Swap UV", bb.CustomMeshSwapUV, (obj, value) => { (obj as Spline).CustomMeshSwapUV = value; });
                anyChange |= UITools.IntField(serializedObject, "UV Coords Channel", bb.CustomMeshUVCoordsChannel, (obj, value) => { (obj as Spline).CustomMeshUVCoordsChannel = value; });
                anyChange |= UITools.FloatField(serializedObject, "U Tile Factor", bb.CustomMeshUTileFactor, (obj, value) => { (obj as Spline).CustomMeshUTileFactor = value; });
                anyChange |= UITools.FloatField(serializedObject, "V Tile World Size", bb.CustomMeshVTileWorldSize, (obj, value) => { (obj as Spline).CustomMeshVTileWorldSize = value; });
                anyChange |= UITools.Vector3Field(serializedObject, "Mesh Offset", bb.CustomMeshOffset, (obj, value) => { (obj as Spline).CustomMeshOffset = value; });
            }

            EditorGUI.BeginDisabledGroup(serializedObject.targetObjects.Length > 1);
            if (serializedObject.targetObjects.Length > 1)
                foldoutControlPoints = false;

            GUILayout.Space(10);
            foldoutCreation = EditorGUILayout.Foldout(foldoutCreation, "Segment Creation", true, UnityTextures.boldFoldOutTextStyle);
            if (foldoutCreation)
            {
                anyChange |= OnInspectorGUI_Creation(bb);
            }

            GUILayout.Space(10);
            foldoutControlPoints = EditorGUILayout.Foldout(foldoutControlPoints, "Control Points", true, UnityTextures.boldFoldOutTextStyle);
            if (foldoutControlPoints)
            {
                GUILayout.BeginHorizontal();
                UITools.EnumField(serializedObject, "Control Mode for All Points", controlModeForAllPointsSelection, (obj, value) => { controlModeForAllPointsSelection = (ControlPoint.eControlPointMode)value; }, GUILayout.MinWidth(60));
                if (GUILayout.Button("Set", GUILayout.Width(40)))
                {
                    ControlPoint.eControlPointMode mode = (ControlPoint.eControlPointMode)controlModeForAllPointsSelection;
                    foreach (ControlPoint cp in bb.ControlPoints)
                        cp.ControlPointMode = mode;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                OnInspectorGUIControlPointsHeader();
                int c = -1;
                foreach (ControlPoint cp in bb.ControlPoints)
                {
                    c++;
                    anyChange |= OnInspectorGUIControlPoint(cp, c);
                }
                if (subObjectMode)
                {
                    // 2019.09.10 Added functionallity:
                    EditorGUILayout.BeginHorizontal();
                    mMultiSelection = GUILayout.Toggle(mMultiSelection, new GUIContent("Multi selection", UnityTextures.mSubObjectModeIcon, "Allows selecting and manipulating control points"), "Button", GUILayout.Height(24));

                    if (GUILayout.Button("Clear selection", GUILayout.Width(116)))
                    {
                        SelectedSubEntitiesClear();
                    }
                    if (GUILayout.Button("Reverse selection"))
                    {
                        foreach (ControlPoint cp in bb.ControlPoints)
                        {
                            if (selectedSubEntities.Contains(cp))
                            {
                                SelectedSubEntitiesRemove(cp);
                            }
                            else
                            {
                                SelectedSubEntitiesAdd(cp);
                            }
                        }
                    }            
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Label("(*) Selected Control Points", UnityTextures.italicTextStyle);
                }
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.Space(10);
            //anyChange |= OnInspectorGUI_Settings(bb);

            if (anyChange)
            {
                foreach (Object obj in serializedObject.targetObjects)
                {
                    if (obj is Spline)
                        (obj as Spline).Refresh();
                }
            }

            serializedObject.ApplyModifiedProperties();

            Tools.hidden = false;
        }
        #endregion

        #region OnScene GUI
        /// <summary>
        /// 
        /// </summary>
        public static void RedrawScene()
        {
            SceneView.RepaintAll();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Vector3 calculateLocalCenter(Spline bb)
        {
            Vector3 centerLocal = new Vector3();
            if (selectedSubEntities.Count > 0)
            {
                foreach (ControlPoint p in selectedSubEntities)
                    centerLocal += p.Position;
                centerLocal *= (1f / (float)selectedSubEntities.Count);
            }
            else if (bb.ControlPoints.Count > 0)
            {
                foreach (ControlPoint p in bb.ControlPoints)
                    centerLocal += p.Position;
                centerLocal *= (1f / (float)bb.ControlPoints.Count);
            }

            return centerLocal;
        }
        /// <summary>
        /// 
        /// </summary>
        private bool OnSceneGuiMove(Spline bb)
        {
            bool anyChange = false;
            //int cloneItemIdx = -1;          // Solo permito clonar uno cada vez que pasa por aqui
            if (selectedSubEntities.Count == 0)
                return false;

            if (selectedSubEntities.Count == 1 && bb.CurveType == Spline.eSplineType.Bezier)
            {
                ControlPoint cp = this.selectedSubEntities.First();
                int idx = bb.ControlPoints.IndexOf(cp);

                float size = 0.5f * HandleUtility.GetHandleSize(bb.transform.TransformPoint(cp.Position));

                if (idx < bb.ControlPoints.Count - 1 || bb.IsClosed)
                {
                    Vector3 handlePoint = bb.transform.TransformPoint(cp.ControlHandleOut + cp.Position);

                    Vector3 newPositionHandle = handlePoint;
                    newPositionHandle = CustomHandlesSplineStudio.PositionHandle(handlePoint, Quaternion.identity, size, false, bb.EditHandlesXZ, bb.EditHandlesY);
                    if (newPositionHandle != handlePoint)
                    {
                        Undo.RegisterCompleteObjectUndo(bb, "Change Control Point Handle");
                        cp.ControlHandleOut = bb.transform.InverseTransformPoint(newPositionHandle) - cp.Position;
                        anyChange = true;

                        if (cp.ControlPointMode != ControlPoint.eControlPointMode.Free)
                            cp.HandleOutChanged();
                    }
                }
                if (idx > 0 || bb.IsClosed)
                {
                    Vector3 handlePoint = bb.transform.TransformPoint(cp.ControlHandleIn + cp.Position);

                    Vector3 newPositionHandle = handlePoint;
                    newPositionHandle = CustomHandlesSplineStudio.PositionHandle(handlePoint, Quaternion.identity, size, false, bb.EditHandlesXZ, bb.EditHandlesY);
                    if (newPositionHandle != handlePoint)
                    {
                        Undo.RegisterCompleteObjectUndo(bb, "Change Control Point Handle");
                        cp.ControlHandleIn = bb.transform.InverseTransformPoint(newPositionHandle) - cp.Position;
                        anyChange = true;


                        if (cp.ControlPointMode != ControlPoint.eControlPointMode.Free)
                            cp.HandleInChanged();
                    }

                }

            }

            // Calculate center
            Vector3 centerLocal = calculateLocalCenter(bb);
            Vector3 centerWorld = bb.transform.TransformPoint(centerLocal);

            Vector3 newPositionWorld = Handles.PositionHandle(centerWorld, Quaternion.identity);
            if (newPositionWorld != centerWorld)
            {
                anyChange = true;
                Vector3 newPositionLocalCoords = bb.transform.InverseTransformPoint(newPositionWorld);
                Vector3 offsetLocalCoords = newPositionLocalCoords - centerLocal;
                if (offsetLocalCoords != Vector3.zero)
                {
                    anyChange = true;
                    Undo.RegisterCompleteObjectUndo(bb, "Move Control Points");
                    int i = -1;
                    ControlPoint[] arrayclone = bb.ControlPoints.ToArray();
                    foreach (ControlPoint p in arrayclone)
                    {
                        i++;
                        if (!selectedSubEntities.Contains(p))
                            continue;
                        p.Position += offsetLocalCoords;

                        //if (Event.current.shift && selectedSubEntities.Count == 1)
                        //{
                        //    if (ReadyForClone)
                        //    {
                        //        ControlPoint newPt = bb.ControlPoints[i].Clone();
                        //        if (i < bb.ControlPoints.Count - 1)
                        //            bb.ControlPoints.Insert(i + 1, newPt);
                        //        else bb.ControlPoints.Add(newPt);

                        //        selectedSubEntities.Clear();
                        //        selectedSubEntities.Add(newPt);
                        //    }
                        //}
                    }
                }

                ReadyForClone = false;
            }

            return anyChange;
        }
        /// <summary>
        /// 
        /// </summary>
        private bool OnSceneGuiRotate(Spline bb)
        {
            bool anyChange = false;
            //int cloneItemIdx = -1;          // Solo permito clonar uno cada vez que pasa por aqui
            if (selectedSubEntities.Count == 0)
                return false;

            int controlId = GUIUtility.GetControlID(FocusType.Passive);     // Retrieve the control Id for the editor window     
            EventType eventType = Event.current.GetTypeForControl(controlId);


            // Calculate center
            Vector3 centerLocal = calculateLocalCenter(bb);
            Vector3 centerWorld = bb.transform.TransformPoint(centerLocal);

            // Find segment dir
            ControlPoint pt1 = selectedSubEntities.First();
            ControlPoint pt2;
            ControlPoint pt0;
            int idx = bb.ControlPoints.IndexOf(pt1);
            Vector3 segmentDir;
            if (idx == bb.ControlPoints.Count - 1)
            {
                pt2 = pt1;
                pt1 = bb.ControlPoints[idx - 1];
                segmentDir = (bb.transform.TransformPoint(pt2.Position) - bb.transform.TransformPoint(pt1.Position)).normalized;
            }
            else if (idx == 0)
            {
                pt2 = bb.ControlPoints[idx + 1];
                segmentDir = (bb.transform.TransformPoint(pt2.Position) - bb.transform.TransformPoint(pt1.Position)).normalized;
            }
            else
            {
                pt2 = bb.ControlPoints[idx + 1];
                pt0 = bb.ControlPoints[idx - 1];
                Vector3 dir1 = (bb.transform.TransformPoint(pt2.Position) - bb.transform.TransformPoint(pt1.Position)).normalized;
                Vector3 dir2 = (bb.transform.TransformPoint(pt1.Position) - bb.transform.TransformPoint(pt0.Position)).normalized;
                segmentDir = (dir1 + dir2) * 0.5f;
            }


            float size = HandleUtility.GetHandleSize(centerWorld);
            Quaternion newRotation = Handles.Disc(Quaternion.Euler(0, 0, -pt1.Camber), centerWorld, segmentDir, size, false, 1);
            if (newRotation != Quaternion.identity && eventType == EventType.MouseDrag && Event.current.button == 0)
            {
                float newValue = newRotation.eulerAngles.z;
                float offsetCamber = (-newValue - pt1.Camber);
                if (offsetCamber < -180)
                    offsetCamber = offsetCamber + 365;


                if (offsetCamber != 0)
                {
                    anyChange = true;
                    Undo.RegisterCompleteObjectUndo(bb, "Rotate Control Points");
                    int i = -1;
                    foreach (ControlPoint p in bb.ControlPoints)
                    {
                        i++;
                        if (!selectedSubEntities.Contains(p))
                            continue;
                        p.Camber += offsetCamber;
                    }
                }
            }

            return anyChange;
        }
        private float mCurrentScale = 1;
        /// <summary>
        /// 
        /// </summary>
        private bool OnSceneGuiScale(Spline bb)
        {
            bool anyChange = false;
            //int cloneItemIdx = -1;          // Solo permito clonar uno cada vez que pasa por aqui
            if (selectedSubEntities.Count == 0)
                return false;

            int controlId = GUIUtility.GetControlID(FocusType.Passive);     // Retrieve the control Id for the editor window     
            EventType eventType = Event.current.GetTypeForControl(controlId);


            // Calculate center
            Vector3 centerLocal = calculateLocalCenter(bb);
            Vector3 centerWorld = bb.transform.TransformPoint(centerLocal);

            float size = 2f * HandleUtility.GetHandleSize(centerWorld);
            mCurrentScale = 1;
            mCurrentScale = Handles.ScaleValueHandle(mCurrentScale, centerWorld, Quaternion.identity, size, Handles.CubeHandleCap, 0.1f);

            if (eventType != EventType.MouseDrag || Event.current.button != 0)
            {
                mCurrentScale = 1;
                return false;
            }

            float dif = (mCurrentScale - 1f) * 0.025f;
            if (dif != 0)
            {
                float newScale = 1f + dif;
                anyChange = true;
                Undo.RegisterCompleteObjectUndo(bb, "Scale Control Points");
                int i = -1;
                foreach (ControlPoint p in bb.ControlPoints)
                {
                    i++;
                    if (!selectedSubEntities.Contains(p))
                        continue;

                    float curLen = p.ControlHandleOut.magnitude;
                    curLen *= newScale;
                    curLen = Mathf.Max(0.0001f, curLen);
                    p.ControlHandleOut = p.ControlHandleOut.normalized * curLen;

                    curLen = p.ControlHandleIn.magnitude;
                    curLen *= newScale;
                    curLen = Mathf.Max(0.0001f, curLen);
                    p.ControlHandleIn = p.ControlHandleIn.normalized * curLen;

                }
            }
            else mCurrentScale = 1;

            return anyChange;
        }
        /// <summary>
        /// Called when Scene UI is managed. 
        /// </summary>
        void OnSceneGUI()
        {
            Tools.hidden = false;
            // Before anyoneElse processes this messages (and potentially mark the message as used), use them to reset some flags
            switch (Event.current.type)
            {
                case EventType.MouseUp:
                    ReadyForClone = true;
                    break;
            }


            // Check if we have something selected
            Spline bb = (target as Spline);
            if (bb == null || !Selection.Contains(bb.gameObject))
                return;
            if (Selection.objects.Length > 1)
                return;

            // Grab ID of current control
            bool anyChange = false;
            int controlId = GUIUtility.GetControlID(FocusType.Passive);     // Retrieve the control Id for the editor window     

            if (subObjectMode || bb.AlwaysRenderControlPoints)
            {
                // Disable regular transform tools gizmos
                //if (subObjectMode)
                //    Tools.hidden = true;

                // Draw lines between control points
                List<Vector3> pointPairs = new List<Vector3>();
                List<Vector3> pointPairsGray = new List<Vector3>();
                int i = 0;
                for (i = 0; i < bb.ControlPoints.Count; i++)
                {
                    Vector3 p1 = bb.transform.TransformPoint(bb.ControlPoints[i].Position);
                    if (i < bb.ControlPoints.Count - 1)
                    {
                        Vector3 p2 = bb.transform.TransformPoint(bb.ControlPoints[i + 1].Position);
                        pointPairs.Add(p1);
                        pointPairs.Add(p2);
                    }
                }
                GLRendering.DrawLines(pointPairs.ToArray(), subObjectMode ? Color.white : Color.gray);

                // Draw Control Points
                for (i = 0; i < bb.ControlPoints.Count; i++)
                {
                    Vector3 p1 = bb.transform.TransformPoint(bb.ControlPoints[i].Position);

                    bool isSelected = subObjectMode && selectedSubEntities.Contains(bb.ControlPoints[i]);
                    float size = Mathf.Min(0.5f, 0.2f * HandleUtility.GetHandleSize(p1));
                    float handleSize = Mathf.Min(0.35f, 0.125f * HandleUtility.GetHandleSize(p1));
                    Color regularColor = (i == 0) ? Color.yellow : new Color(0.1f, 0.1f, 1f, 0.75f);
                    if (!subObjectMode)
                        regularColor = Color.gray;
                    GLRendering.FillCube(p1, new Vector3(size, size, size), Quaternion.identity, isSelected ? Color.red : regularColor);

                    // Draw control handle in
                    if (subObjectMode && bb.CurveType == Spline.eSplineType.Bezier && selectedSubEntities.Contains(bb.ControlPoints[i]))
                    {
                        bool editHandlesEnabled = Tools.current == Tool.Move && (bb.EditHandlesY || bb.EditHandlesXZ);
                        Color handleColor = editHandlesEnabled ? Color.green : new Color(0f, 0f, 0f, 0.2f);
                        if (i > 0 || bb.IsClosed)
                        {
                            Vector3 handlePoint = bb.transform.TransformPoint(bb.ControlPoints[i].ControlHandleIn + bb.ControlPoints[i].Position);
                            GLRendering.FillCube(handlePoint, new Vector3(handleSize, handleSize, handleSize), Quaternion.identity, handleColor);
                            pointPairsGray.Add(p1);
                            pointPairsGray.Add(handlePoint);
                        }
                        if (i < bb.ControlPoints.Count - 1 || bb.IsClosed)
                        {
                            Vector3 handlePoint = bb.transform.TransformPoint(bb.ControlPoints[i].ControlHandleOut + bb.ControlPoints[i].Position);
                            GLRendering.FillCube(handlePoint, new Vector3(handleSize, handleSize, handleSize), Quaternion.identity, handleColor);
                            pointPairsGray.Add(p1);
                            pointPairsGray.Add(handlePoint);
                        }
                    }

                }

                if (pointPairsGray.Count > 0)
                    GLRendering.DrawLines(pointPairsGray.ToArray(), Color.gray);


                // Avoid drawing handles when creating manually
                if (subObjectMode && !manualCreationModeRefine && !manualCreationModeAdd)
                {

                    if (Tools.current == Tool.Move)
                        anyChange |= OnSceneGuiMove(bb);
                    else if (Tools.current == Tool.Rotate)
                        anyChange |= OnSceneGuiRotate(bb);
                    else if (Tools.current == Tool.Scale || Tools.current == Tool.Transform || Tools.current == Tool.Rect)
                        anyChange |= OnSceneGuiScale(bb);
                }

            }
            else
            {
                selectingByRect = false;
            }



            Tools.hidden = subObjectMode;

            anyChange |= ProcessMouseEvents(controlId);

            if (anyChange)
            {
                bb.Refresh();
                EditorUtility.SetDirty(this.target);
            }
        }
        #endregion

        #region Mouse Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlId"></param>
        /// <param name="paginationStartIdx"></param>
        /// <param name="paginationEndIdx"></param>
        private void ProcessMouseEventsSelectByMouse(int controlId)
        {
            // Only act if left button is pressed
            if (Event.current.button != 0)
                return;

            EventType type = Event.current.GetTypeForControl(controlId);

            Spline bb = (target as Spline);

            // Draw Selection Rectangle
            selectionRect = new Rect();
            if (subObjectMode && selectingByRect)
            {
                //Debug.Log("handles!");
                Handles.BeginGUI();

                Vector2 size = (Event.current.mousePosition - startMouseRect);
                Vector2 pos = startMouseRect;
                selectionRect = new Rect(pos, size);
                Handles.DrawSolidRectangleWithOutline(selectionRect, new Color(0.75f, 0.75f, 1f, 0.25f), Color.white);
                Handles.EndGUI();
            }

            switch (type)
            {
                case EventType.MouseDown:
                    // Tell the UI your event is the main one to use, it override the selection in  the scene view
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();                    // Don't forget to use the event
                    startMouseRect = Event.current.mousePosition;
                    mouseDown = true;
                    RedrawScene();
                    break;
                case EventType.MouseUp:
                    if (mouseDown)
                    {
                        if (!Event.current.control && !Event.current.alt)
                            SelectedSubEntitiesClear();

                        Rect rect = new Rect();
                        if (selectingByRect)
                        {
                            // Selection by rect finished
                            rect = selectionRect;
                            if (rect.width < 0)
                            {
                                rect.x -= Mathf.Abs(rect.width);
                                rect.width *= -1;
                            }
                            if (rect.height < 0)
                            {
                                rect.y -= Mathf.Abs(rect.height);
                                rect.height *= -1;
                            }
                        }
                        else if (!selectingByRect || rect.width < 5 || rect.height < 5)
                        {
                            // Selecting By Click (generate a new rect of 20px x 20 px around mouse Position)
                            Vector2 size = new Vector2(20, 20);
                            Vector2 pos = Event.current.mousePosition - (size * 0.5f);
                            rect = new Rect(pos, size);
                        }
                        selectingByRect = false;

                        int j = -1;
                        foreach (ControlPoint p in bb.ControlPoints)
                        {
                            j++;

                            Vector3 worldPos = bb.transform.TransformPoint(p.Position);
                            Vector3 screenPos = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(worldPos);

                            // If pressing Alt key, we want to remove from selection
                            if (Event.current.alt)
                            {
                                if (rect.Contains(new Vector2(screenPos.x, SceneView.lastActiveSceneView.camera.pixelHeight - screenPos.y)))
                                    SelectedSubEntitiesRemove(p);
                            }
                            else
                            {
                                if (rect.Contains(new Vector2(screenPos.x, SceneView.lastActiveSceneView.camera.pixelHeight - screenPos.y)))
                                    SelectedSubEntitiesAdd(p);
                            }
                        }
                    }
                    mouseDown = false;
                    RedrawScene();
                    break;
                case EventType.MouseDrag:
                    if (mouseDown)
                        selectingByRect = true;
                    if (GUIUtility.hotControl == controlId)
                    {
                        // Force repaint as mouse is dragged.
                        Event.current.Use();
                    }
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlId"></param>
        /// <param name="paginationStartIdx"></param>
        /// <param name="paginationEndIdx"></param>
        /// <returns></returns>
        private bool ProcessMouseEventsManualCreationModeRefine(int controlId)
        {
            Spline bb = (target as Spline);
            bool anyChange = false;
            Vector2 v = new Vector3(Event.current.mousePosition.x, SceneView.lastActiveSceneView.camera.pixelHeight - Event.current.mousePosition.y);

            Vector3 hitPoint;
            int startIdx;
            bool haveHit = false;
            if (bb.MouseCast(v, bb.transform, SceneView.lastActiveSceneView.camera, out hitPoint, out startIdx))
            {
                haveHit = true;

                // Debug.Log("Checking ray hit: " + ray);
                float size = 0.125f * HandleUtility.GetHandleSize(hitPoint);
                GLRendering.FillCube(hitPoint, new Vector3(size, size, size), Quaternion.identity, Color.red);
            }

            if (Event.current.button == 0)
            {
                if (Event.current.GetTypeForControl(controlId) == EventType.MouseUp)
                {
                    if (mouseDown)
                    {
                        if (haveHit)
                        {
                            Undo.RegisterCompleteObjectUndo(bb, "Add CP");

                            ControlPoint newCP = new ControlPoint();
                            newCP.Position = bb.transform.InverseTransformPoint(hitPoint);

                            int insertIdx = startIdx + 1;
                            bb.ControlPoints.Insert(insertIdx, newCP);
                            if (insertIdx == bb.ControlPoints.Count - 2)        // If it's inserted in the last segment, update handles of last point
                                bb.ResetHandles(bb.ControlPoints.Count - 1);
                            bb.ResetHandles(insertIdx);                         // Reset handles of new point

                            SelectedSubEntitiesClear();
                            SelectedSubEntitiesAdd(newCP);
                            anyChange = true;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error", "No collider could be found in the mouse position. Please add colliders to your scene that act as ground, and make sure you click in a place with a known ground hit (a red pyramid will appear)", "Ok");
                        }
                    }
                    mouseDown = false;
                }
                else if (Event.current.GetTypeForControl(controlId) == EventType.MouseDown)
                {
                    mouseDown = true;
                    // Tell the UI your event is the main one to use, it override the selection in  the scene view
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();                    // Don't forget to use the event
                }
            }

            if (haveHit)
                RedrawScene();

            return anyChange;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlId"></param>
        /// <param name="paginationStartIdx"></param>
        /// <param name="paginationEndIdx"></param>
        /// <returns></returns>
        private bool ProcessMouseEventsManualCreationModeAdd(int controlId)
        {
            Spline bb = (target as Spline);
            bool anyChange = false;

            Vector3 v = new Vector3(Event.current.mousePosition.x, SceneView.lastActiveSceneView.camera.pixelHeight - Event.current.mousePosition.y, 0);
            Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(v);

            // create a plane at 0,0,0 whose normal points to +Y:
            Plane hPlane = new Plane(Vector3.up, new Vector3(0, bb.mManualModeAddPlaneHeight, 0));
            // Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
            float distance = 0;
            Vector3 hitPoint = Vector3.zero;
            bool haveHit = false;
            // if the ray hits the plane...
            if (hPlane.Raycast(ray, out distance))
            {
                haveHit = true;
                hitPoint = ray.GetPoint(distance);

                // Debug.Log("Checking ray hit: " + ray);
                float size = 0.125f * HandleUtility.GetHandleSize(hitPoint);
                GLRendering.FillCube(hitPoint, new Vector3(size, size, size), Quaternion.identity, Color.red);
            }

            if (Event.current.button == 0)
            {
                if (Event.current.GetTypeForControl(controlId) == EventType.MouseUp)
                {
                    if (mouseDown)
                    {
                        if (haveHit)
                        {
                            Undo.RegisterCompleteObjectUndo(bb, "Add CP");

                            ControlPoint newCP = new ControlPoint();
                            newCP.Position = bb.transform.InverseTransformPoint(hitPoint);
                            bb.ControlPoints.Add(newCP);
                            if (bb.ControlPoints.Count >= 2)
                                bb.ResetHandles(bb.ControlPoints.Count - 2);    // Reset handles of previous last one, that now has inHandle too
                            if (bb.ControlPoints.Count >= 1)                     // Reset handles of new point
                                bb.ResetHandles(bb.ControlPoints.Count - 1);

                            SelectedSubEntitiesClear();
                            SelectedSubEntitiesAdd(newCP);
                            anyChange = true;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error", "No collider could be found in the mouse position. Please add colliders to your scene that act as ground, and make sure you click in a place with a known ground hit (a red pyramid will appear)", "Ok");
                        }
                    }
                    mouseDown = false;
                }
                else if (Event.current.GetTypeForControl(controlId) == EventType.MouseDown)
                {
                    mouseDown = true;
                    // Tell the UI your event is the main one to use, it override the selection in  the scene view
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();                    // Don't forget to use the event
                }
            }

            if (haveHit)
                RedrawScene();

            return anyChange;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlId"></param>
        private bool ProcessMouseEvents(int controlId)
        {
            bool anyChange = false;
            Spline bb = (target as Spline);

            // If Left-button pressed (Event.current.button only applies when current event is of Type MouseDown or MouseUp)
            if (manualCreationModeRefine)
                anyChange |= ProcessMouseEventsManualCreationModeRefine(controlId);
            else if (manualCreationModeAdd)
                anyChange |= ProcessMouseEventsManualCreationModeAdd(controlId);
            else if (subObjectMode)
            {
                if (Event.current.button == 0)
                    ProcessMouseEventsSelectByMouse(controlId);
            }

            return anyChange;
        }
        #endregion

    }
}
#endif