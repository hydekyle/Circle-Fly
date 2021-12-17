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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
#if(UNITY_EDITOR)

namespace GraphicDNA.SplineStudio
{
    public static class UITools
    {
        /// <summary>
        /// Shows a toggle in the UI
        /// This version of the method can work on single selection objects only. Takes the current value as parameter and returns the new value.
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns>The new value to be set to the property</returns>
        public static bool Toggle(UnityEngine.Object pObject, string pLabel, bool pCurrentValue, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            bool newValue = EditorGUILayout.Toggle(pLabel, pCurrentValue, options);
            if (EditorGUI.EndChangeCheck())
                Undo.RecordObject(pObject, pLabel + " changed");
            return newValue;
        }
        /// <summary>
        /// Shows a toggle in the UI
        /// This version of the method is designed to work on a SerializedObject, which can handle multiple selections. Returns true if the value has changed
        /// If the value changes, calls the Action to put the new value to each object in the multiple selection (serializedObject.targetObjects)
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns>true if the value has changed, false otherwise</returns>
        public static bool Toggle(SerializedObject obj, string pLabel, bool pCurrentValue, Action<UnityEngine.Object, bool> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            return Toggle(obj, new GUIContent(pLabel), pCurrentValue, pActionToRunIfValueChanges, options);
        }
        /// <summary>
        /// Shows a toggle in the UI
        /// This version of the method is designed to work on a SerializedObject, which can handle multiple selections. Returns true if the value has changed
        /// If the value changes, calls the Action to put the new value to each object in the multiple selection (serializedObject.targetObjects)
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns>true if the value has changed, false otherwise</returns>
        public static bool Toggle(SerializedObject obj, GUIContent pLabelContent, bool pCurrentValue, Action<UnityEngine.Object, bool> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            bool newValue = EditorGUILayout.Toggle(pLabelContent, pCurrentValue, options);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(obj.targetObjects, pLabelContent.text + " change");
                foreach (UnityEngine.Object bb in obj.targetObjects)
                {
                    if (pActionToRunIfValueChanges != null)
                        pActionToRunIfValueChanges(bb, newValue);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates an int field
        /// </summary>
        /// <param name="pObject">Serialized object currently being edited</param>
        /// <param name="pLabel">Label for the field</param>
        /// <param name="pCurrentValue">Current value</param>
        /// <returns>true if the property changed, false otherwise</returns>
        public static bool IntField(SerializedObject obj, string pLabel, int pCurrentValue, Action<UnityEngine.Object, int> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            return IntField(obj, new GUIContent(pLabel), pCurrentValue, pActionToRunIfValueChanges, options);
        }
        /// <summary>
        /// Creates an int field
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pContent"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pActionToRunIfValueChanges"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool IntField(SerializedObject obj, GUIContent pContent, int pCurrentValue, Action<UnityEngine.Object, int> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            int newValue = EditorGUILayout.IntField(pContent, pCurrentValue, options);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(obj.targetObjects, pContent.text + " change");
                foreach (UnityEngine.Object bb in obj.targetObjects)
                {
                    if (pActionToRunIfValueChanges != null)
                        pActionToRunIfValueChanges(bb, newValue);
                }
                return true;
            }

            return false;
        }
        public static bool EnumField(SerializedObject obj, string pLabel, Enum pCurrentValue, Action<UnityEngine.Object, Enum> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            return EnumField(obj, new GUIContent(pLabel), pCurrentValue, pActionToRunIfValueChanges, options);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns></returns>
        public static bool EnumField(SerializedObject obj, GUIContent pLabel, Enum pCurrentValue, Action<UnityEngine.Object, Enum> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();


            Enum newValue = EditorGUILayout.EnumPopup(pLabel, pCurrentValue, options);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(obj.targetObjects, pLabel + " change");
                foreach (UnityEngine.Object bb in obj.targetObjects)
                {
                    if (pActionToRunIfValueChanges != null)
                        pActionToRunIfValueChanges(bb, newValue);
                }
                return true;
            }

            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns></returns>
        public static bool FloatField(SerializedObject obj, string pLabel, float pCurrentValue, Action<UnityEngine.Object, float> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            return FloatField(obj, new GUIContent(pLabel), pCurrentValue, pActionToRunIfValueChanges, options);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns></returns>
        public static bool FloatField(SerializedObject obj, GUIContent pLabel, float pCurrentValue, Action<UnityEngine.Object, float> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            float newValue = EditorGUILayout.FloatField(pLabel, pCurrentValue, options);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(obj.targetObjects, pLabel + " change");
                foreach (UnityEngine.Object bb in obj.targetObjects)
                {
                    if (pActionToRunIfValueChanges != null)
                        pActionToRunIfValueChanges(bb, newValue);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pActionToRunIfValueChanges"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool ColorField(SerializedObject obj, string pLabel, Color pCurrentValue, Action<UnityEngine.Object, Color> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            return ColorField(obj, new GUIContent(pLabel), pCurrentValue, pActionToRunIfValueChanges, options);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <param name="pActionToRunIfValueChanges"></param>
        /// <returns></returns>
        public static bool ColorField(SerializedObject obj, GUIContent pLabel, Color pCurrentValue, Action<UnityEngine.Object, Color> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            Color newColor = EditorGUILayout.ColorField(pLabel, pCurrentValue, options);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(obj.targetObjects, pLabel + " change");
                foreach (UnityEngine.Object bb in obj.targetObjects)
                {
                    if (pActionToRunIfValueChanges != null)
                        pActionToRunIfValueChanges(bb, newColor);
                }

                return true;
            }

            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <param name="pActionToRunIfValueChanges"></param>
        /// <returns></returns>
        public static bool MaterialField(SerializedObject obj, string pLabel, Material pCurrentValue, Action<UnityEngine.Object, Material> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            return MaterialField(obj, new GUIContent(pLabel), pCurrentValue, pActionToRunIfValueChanges, options);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <param name="pActionToRunIfValueChanges"></param>
        /// <returns></returns>
        public static bool MaterialField(SerializedObject obj, GUIContent pLabel, Material pCurrentValue, Action<UnityEngine.Object, Material> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            object newObj = EditorGUILayout.ObjectField(pLabel, pCurrentValue, typeof(Material), false, options);
            Material newMat = newObj as Material;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(obj.targetObjects, pLabel + " change");
                foreach (UnityEngine.Object bb in obj.targetObjects)
                {
                    if (pActionToRunIfValueChanges != null)
                        pActionToRunIfValueChanges(bb, newMat);
                }

                return true;
            }

            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetObjects"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pMin"></param>
        /// <param name="pMax"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns></returns>
        public static bool Slider(SerializedObject obj, string pLabel, float pCurrentValue, float pMin, float pMax, Action<UnityEngine.Object, float> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            return Slider(obj, new GUIContent(pLabel), pCurrentValue, pMin, pMax, pActionToRunIfValueChanges, options);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetObjects"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pMin"></param>
        /// <param name="pMax"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns></returns>
        public static bool Slider(SerializedObject obj, GUIContent pLabelContent, float pCurrentValue, float pMin, float pMax, Action<UnityEngine.Object, float> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            float newValue = EditorGUILayout.Slider(pLabelContent, pCurrentValue, pMin, pMax, options);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(obj.targetObjects, pLabelContent.text + " change");
                foreach (UnityEngine.Object bb in obj.targetObjects)
                {
                    if (pActionToRunIfValueChanges != null)
                        pActionToRunIfValueChanges(bb, newValue);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLabel"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector2 Vector2Field(string pLabel, Vector2 vec, int pLabelWidth, string pSecondLabel = ",", int pSecondLabelWidth = 10)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(pLabel, GUILayout.Width(pLabelWidth));
            float x = EditorGUILayout.FloatField(vec.x, GUILayout.MinWidth(40));
            EditorGUILayout.LabelField(pSecondLabel, GUILayout.Width(pSecondLabelWidth));
            float y = EditorGUILayout.FloatField(vec.y, GUILayout.MinWidth(40));
            EditorGUILayout.LabelField("m", GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();
            return new Vector2(x, y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pLabelContent"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pActionToRunIfValueChanges"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool Vector3Field(SerializedObject obj, string pLabelContent, Vector3 pCurrentValue, Action<UnityEngine.Object, Vector3> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            return Vector3Field(obj, new GUIContent(pLabelContent), pCurrentValue, pActionToRunIfValueChanges, options);
        }
        /// <summary>
        /// Shows a Vector3 in the UI
        /// This version of the method is designed to work on a SerializedObject, which can handle multiple selections. Returns true if the value has changed
        /// If the value changes, calls the Action to put the new value to each object in the multiple selection (serializedObject.targetObjects)
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns>true if the value has changed, false otherwise</returns>
        public static bool Vector3Field(SerializedObject obj, GUIContent pLabelContent, Vector3 pCurrentValue, Action<UnityEngine.Object, Vector3> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newValue = EditorGUILayout.Vector3Field(pLabelContent, pCurrentValue, options);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(obj.targetObjects, pLabelContent.text + " change");
                foreach (UnityEngine.Object bb in obj.targetObjects)
                {
                    if (pActionToRunIfValueChanges != null)
                        pActionToRunIfValueChanges(bb, newValue);
                }

                return true;
            }
            return false;
        }
        /// <summary>
        /// Shows a Vector3 in the UI
        /// This version of the method is designed to work on a SerializedObject, which can handle multiple selections. Returns true if the value has changed
        /// If the value changes, calls the Action to put the new value to each object in the multiple selection (serializedObject.targetObjects)
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns>true if the value has changed, false otherwise</returns>
        public static bool ObjectField<T>(SerializedObject obj, string pLabel, UnityEngine.Object pCurrentValue, Action<UnityEngine.Object, UnityEngine.Object> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
            where T : UnityEngine.Object
        {
            return ObjectField<T>(obj, new GUIContent(pLabel), pCurrentValue, pActionToRunIfValueChanges, options);
        }
        /// <summary>
        /// Shows a Vector3 in the UI
        /// This version of the method is designed to work on a SerializedObject, which can handle multiple selections. Returns true if the value has changed
        /// If the value changes, calls the Action to put the new value to each object in the multiple selection (serializedObject.targetObjects)
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pLabel"></param>
        /// <param name="pCurrentValue"></param>
        /// <param name="pUndoOperationName"></param>
        /// <returns>true if the value has changed, false otherwise</returns>
        public static bool ObjectField<T>(SerializedObject obj, GUIContent pLabelContent, UnityEngine.Object pCurrentValue, Action<UnityEngine.Object, UnityEngine.Object> pActionToRunIfValueChanges = null, params GUILayoutOption[] options)
        where T : UnityEngine.Object
        {
            EditorGUI.BeginChangeCheck();
            T newValue = (T)EditorGUILayout.ObjectField(pLabelContent, pCurrentValue, typeof(T), true, options);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(obj.targetObjects, pLabelContent.text + " change");
                foreach (UnityEngine.Object bb in obj.targetObjects)
                {
                    if (pActionToRunIfValueChanges != null)
                        pActionToRunIfValueChanges(bb, newValue);
                }

                return true;
            }
            return false;
        }


    }
}
#endif