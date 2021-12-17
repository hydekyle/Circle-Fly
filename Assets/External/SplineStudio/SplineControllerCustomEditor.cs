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
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;

namespace GraphicDNA.SplineStudio
{
    [CustomEditor(typeof(SplineController))]
    [CanEditMultipleObjects]
    public class SplineControllerCustomEditor : Editor
    {
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
            SplineController bb = (serializedObject.targetObject as SplineController);

            GUI.backgroundColor = new Color(0.91f, 0.91f, 0.91f);
            bool anyChange = false;

            GUILayout.Space(10);
            anyChange |= UITools.ObjectField<Spline>(serializedObject, "Spline", bb.Spline, (obj, value) => { (obj as SplineController).Spline = (Spline)value; });
            anyChange |= UITools.Slider(serializedObject, new GUIContent("Curve Percent", "Percent of the curve where the object is initially located"), bb.CurvePercent, 0f, 1f, (obj, value) => { (obj as SplineController).CurvePercent = value; });
            anyChange |= UITools.Slider(serializedObject, new GUIContent("Damping Time", "Increase this value to damp repositioning and reorientation of the object. Zero to disable damping"), bb.DampingTime, 0f, 1f, (obj, value) => { (obj as SplineController).DampingTime = value; });

            GUILayout.Space(10);
            GUILayout.Label("Position", UnityTextures.boldTextStyle);
            anyChange |= UITools.Toggle(serializedObject, new GUIContent("Affect Position", "Enables/Disables positioning of the object according to the spline interpolation"), bb.AffectPosition, (obj, value) => { (obj as SplineController).AffectPosition = value; });
            anyChange |= UITools.Vector3Field(serializedObject, new GUIContent("Position Offset", "Offset applied to the position interpolated from the spline (in local coordinates)"), bb.PositionOffset, (obj, value) => { (obj as SplineController).PositionOffset = value; });

            GUILayout.Space(10);
            GUILayout.Label("Rotation", UnityTextures.boldTextStyle);
            anyChange |= UITools.EnumField(serializedObject, "Orientation Mode", bb.OrientationMode, (obj, value) => { (obj as SplineController).OrientationMode = (SplineController.eOrientationMode)value; });
            anyChange |= UITools.Vector3Field(serializedObject, new GUIContent("Rotation Offset", "Offset applied to the orientation interpolated from the spline"), bb.AdditionalRotation, (obj, value) => { (obj as SplineController).AdditionalRotation = value; });
            if (bb.OrientationMode == SplineController.eOrientationMode.FollowTarget)
                anyChange |= UITools.ObjectField<Transform>(serializedObject, new GUIContent("Target", "Target transform to look at"), bb.Target, (obj, value) => { (obj as SplineController).Target = (Transform)value; });

            GUILayout.Space(10);
            GUILayout.Label("Walking", UnityTextures.boldTextStyle);
            anyChange |= UITools.Toggle(serializedObject, new GUIContent("Automatic Walking", "Enables/Disables automatic walking through the spline"), bb.AutomaticWalking, (obj, value) => { (obj as SplineController).AutomaticWalking = value; });
            anyChange |= UITools.Toggle(serializedObject, new GUIContent("Update in Unity Editor", "If disables, the object will only move when the game is running. If enabled, the object will also move when within the Unity Editor."), bb.UpdateInUnityEditor, (obj, value) => { (obj as SplineController).UpdateInUnityEditor = value; });
            anyChange |= UITools.EnumField(serializedObject, new GUIContent("Loop Mode", "Sets what happens when the object reaches the end of the spline"), bb.LoopMode, (obj, value) => { (obj as SplineController).LoopMode = (SplineController.eLoopMode)value; });
            anyChange |= UITools.EnumField(serializedObject, new GUIContent("Speed Units", "Sets the units to measure the speed"), bb.SpeedUnits, (obj, value) => { (obj as SplineController).SpeedUnits = (SplineController.eSpeedUnits)value; });
            anyChange |= UITools.FloatField(serializedObject, new GUIContent("Speed", "Speed amount, expressed in the units selected above"), bb.Speed, (obj, value) => { (obj as SplineController).Speed = value; });

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Milestones", UnityTextures.boldTextStyle);
            GUILayout.Label(string.Format("(Current Milestone: {0})", bb.CurrentMileStone), UnityTextures.italicTextStyle);
            GUILayout.EndHorizontal();
            anyChange |= UITools.EnumField(serializedObject, new GUIContent("Milestones Based On", "Sets the units to measure the speed"), bb.MilestonesBasedOn, (obj, value) => { (obj as SplineController).MilestonesBasedOn = (SplineController.eMilestoneMode)value; });
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("MilestoneReached"), true);

            GUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();

            if (anyChange)
            {
                foreach (Object obj in serializedObject.targetObjects)
                {
                    EditorUtility.SetDirty(obj);

                    //if (obj is SplineController)
                    //    (obj as SplineController).Refresh();
                }

                SceneView.RepaintAll();
            }


        }
    }
}
#endif