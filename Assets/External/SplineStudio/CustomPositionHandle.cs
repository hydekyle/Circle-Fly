#if(UNITY_EDITOR)
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Internal;
using UnityEditor;

namespace UnityEditor
{
    #region SnapSettings
    internal class SnapSettingsSplineStudio : EditorWindow
    {
        private static float s_MoveSnapX;
        private static float s_MoveSnapY;
        private static float s_MoveSnapZ;

        private static float s_ScaleSnap;
        private static float s_RotationSnap;

        private static bool s_Initialized;

        private static void Initialize()
        {
            if (!s_Initialized)
            {
                s_MoveSnapX = EditorPrefs.GetFloat("MoveSnapX", 1f);
                s_MoveSnapY = EditorPrefs.GetFloat("MoveSnapY", 1f);
                s_MoveSnapZ = EditorPrefs.GetFloat("MoveSnapZ", 1f);

                s_ScaleSnap = EditorPrefs.GetFloat("ScaleSnap", .1f);
                s_RotationSnap = EditorPrefs.GetFloat("RotationSnap", 15);

                s_Initialized = true;
            }
        }

        public static Vector3 move
        {
            get
            {
                Initialize();
                return new Vector3(s_MoveSnapX, s_MoveSnapY, s_MoveSnapZ);
            }
            set
            {
                EditorPrefs.SetFloat("MoveSnapX", value.x);
                s_MoveSnapX = value.x;
                EditorPrefs.SetFloat("MoveSnapY", value.y);
                s_MoveSnapY = value.y;
                EditorPrefs.SetFloat("MoveSnapZ", value.z);
                s_MoveSnapZ = value.z;
            }
        }

        public static float scale
        {
            get
            {
                Initialize();
                return s_ScaleSnap;
            }
            set
            {
                EditorPrefs.SetFloat("ScaleSnap", value);
                s_ScaleSnap = value;
            }
        }

        public static float rotation
        {
            get
            {
                Initialize();
                return s_RotationSnap;
            }
            set
            {
                EditorPrefs.SetFloat("RotationSnap", value);
                s_RotationSnap = value;
            }
        }

        //[MenuItem("Edit/Snap Settings...")]
        //static void ShowSnapSettings()
        //{
        //    EditorWindow.GetWindowWithRect<SnapSettings>(new Rect(100, 100, 230, 130), true, "Snap settings");
        //}

        class Styles
        {
            public GUIStyle buttonLeft = "ButtonLeft";
            public GUIStyle buttonMid = "ButtonMid";
            public GUIStyle buttonRight = "ButtonRight";
            public GUIContent snapAllAxes = TrTextContent("Snap All Axes", "Snaps selected objects to the grid");
            public GUIContent snapX = TrTextContent("X", "Snaps selected objects to the grid on the x axis");
            public GUIContent snapY = TrTextContent("Y", "Snaps selected objects to the grid on the y axis");
            public GUIContent snapZ = TrTextContent("Z", "Snaps selected objects to the grid on the z axis");
            public GUIContent moveX = TrTextContent("Move X", "Grid spacing X");
            public GUIContent moveY = TrTextContent("Move Y", "Grid spacing Y");
            public GUIContent moveZ = TrTextContent("Move Z", "Grid spacing Z");
            public GUIContent scale = TrTextContent("Scale", "Grid spacing for scaling");
            public GUIContent rotation = TrTextContent("Rotation", "Grid spacing for rotation in degrees");
        }
        static Styles ms_Styles;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tooltip"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        internal static GUIContent TrTextContent(string text, string tooltip = null, Texture icon = null)
        {
            string text_k = text != null ? text : "";
            string tooltip_k = tooltip != null ? tooltip : "";
            string key = string.Format("{0}|{1}|{2}", text_k, tooltip_k, icon != null ? icon.name : "");

            GUIContent gc = new GUIContent();
            {
                gc = new GUIContent(text);
                if (tooltip != null)
                {
                    gc.tooltip = tooltip;
                }
                if (icon != null)
                {
                    gc.image = icon;
                }
                //s_GUIContents[key] = gc;
            }
            return gc;
        }


        void OnGUI()
        {
            if (ms_Styles == null)
                ms_Styles = new Styles();

            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            Vector3 m = move;
            m.x = EditorGUILayout.FloatField(ms_Styles.moveX, m.x);
            m.y = EditorGUILayout.FloatField(ms_Styles.moveY, m.y);
            m.z = EditorGUILayout.FloatField(ms_Styles.moveZ, m.z);

            if (EditorGUI.EndChangeCheck())
            {
                if (m.x <= 0) m.x = move.x;
                if (m.y <= 0) m.y = move.y;
                if (m.z <= 0) m.z = move.z;
                move = m;
            }
            scale = EditorGUILayout.FloatField(ms_Styles.scale, scale);
            rotation = EditorGUILayout.FloatField(ms_Styles.rotation, rotation);

            GUILayout.Space(5);

            bool snapX = false, snapY = false, snapZ = false;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(ms_Styles.snapAllAxes, ms_Styles.buttonLeft)) { snapX = true; snapY = true; snapZ = true; }
            if (GUILayout.Button(ms_Styles.snapX, ms_Styles.buttonMid)) { snapX = true; }
            if (GUILayout.Button(ms_Styles.snapY, ms_Styles.buttonMid)) { snapY = true; }
            if (GUILayout.Button(ms_Styles.snapZ, ms_Styles.buttonRight)) { snapZ = true; }
            GUILayout.EndHorizontal();

            if (snapX | snapY | snapZ)
            {
                Vector3 scaleTmp = new Vector3(1.0f / move.x, 1.0f / move.y, 1.0f / move.z);

                Undo.RecordObjects(Selection.transforms, "Snap " + (Selection.transforms.Length == 1 ? Selection.activeGameObject.name : " selection") + " to grid");
                foreach (Transform t in Selection.transforms)
                {
                    Vector3 pos = t.position;
                    if (snapX) pos.x = Mathf.Round(pos.x * scaleTmp.x) / scaleTmp.x;
                    if (snapY) pos.y = Mathf.Round(pos.y * scaleTmp.y) / scaleTmp.y;
                    if (snapZ) pos.z = Mathf.Round(pos.z * scaleTmp.z) / scaleTmp.z;
                    t.position = pos;
                }
            }
        }
    }
    #endregion  

    /// <summary>
    ///   <para>Custom 3D GUI controls and drawing in the scene view.</para>
    /// </summary>
    public sealed class CustomHandlesSplineStudio
    {
        internal enum FilterMode
        {
            Off,
            ShowFiltered,
            ShowRest,
        }

        private enum PlaneHandle
        {
            xzPlane,
            xyPlane,
            yzPlane,
        }
        private static Color lineTransparency = new Color(1f, 1f, 1f, 0.75f);
        private static Dictionary<string, int> Dict = null;
        private static Vector3[] verts = new Vector3[4]
        {
      Vector3.zero,
      Vector3.zero,
      Vector3.zero,
      Vector3.zero
        };
        private static bool s_FreeMoveMode = false;
        private static Vector3 s_PlanarHandlesOctant = Vector3.one;
        public static Color XAxisColor = new Color(0.8588235f, 0.2431373f, 0.1137255f, 0.93f);
        public static Color YAxisColor = new Color(0.6039216f, 0.9529412f, 0.282353f, 0.93f);
        public static Color ZAxisColor = new Color(0.227451f, 0.4784314f, 0.972549f, 0.93f);
        public static Color CenterColor = new Color(0.8f, 0.8f, 0.8f, 0.93f);
        public static Color SelectedColor = new Color(0.9647059f, 0.9490196f, 0.1960784f, 0.89f);
        public static Color SecondaryColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        internal static Color staticColor = new Color(0.5f, 0.5f, 0.5f, 0.0f);
        internal static float staticBlend = 0.6f;
        internal static float backfaceAlphaMultiplier = 0.2f;
        internal static Color s_ColliderHandleColor = new Color(145f, 244f, 139f, 210f) / (float)byte.MaxValue;
        internal static Color s_ColliderHandleColorDisabled = new Color(84f, 200f, 77f, 140f) / (float)byte.MaxValue;
        internal static Color s_BoundingBoxHandleColor = new Color((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue, 150f) / (float)byte.MaxValue;
        internal static int s_SliderHash = "SliderHash".GetHashCode();
        internal static int s_Slider2DHash = "Slider2DHash".GetHashCode();
        internal static int s_FreeRotateHandleHash = "FreeRotateHandleHash".GetHashCode();
        internal static int s_RadiusHandleHash = "RadiusHandleHash".GetHashCode();
        internal static int s_xAxisMoveHandleHash = "xAxisFreeMoveHandleHash".GetHashCode();
        internal static int s_yAxisMoveHandleHash = "yAxisFreeMoveHandleHash".GetHashCode();
        internal static int s_zAxisMoveHandleHash = "xAxisFreeMoveHandleHash".GetHashCode();
        internal static int s_FreeMoveHandleHash = "FreeMoveHandleHash".GetHashCode();
        internal static int s_xzAxisMoveHandleHash = "xzAxisFreeMoveHandleHash".GetHashCode();
        internal static int s_xyAxisMoveHandleHash = "xyAxisFreeMoveHandleHash".GetHashCode();
        internal static int s_yzAxisMoveHandleHash = "yzAxisFreeMoveHandleHash".GetHashCode();
        internal static int s_ScaleSliderHash = "ScaleSliderHash".GetHashCode();
        internal static int s_ScaleValueHandleHash = "ScaleValueHandleHash".GetHashCode();
        internal static int s_DiscHash = "DiscHash".GetHashCode();
        internal static int s_ButtonHash = "ButtonHash".GetHashCode();
        private static bool s_Lighting = true;
        internal static Matrix4x4 s_Matrix = Matrix4x4.identity;
        internal static Matrix4x4 s_InverseMatrix = Matrix4x4.identity;
        private static Vector3[] s_RectangleCapPointsCache = new Vector3[5];
        private const int kMaxDottedLineVertices = 1000;
        private const float k_BoneThickness = 0.08f;

        #region Props
        /// <summary>
        ///   <para>Are handles lit?</para>
        /// </summary>
        public static bool lighting
        {
            get
            {
                return CustomHandlesSplineStudio.s_Lighting;
            }
            set
            {
                CustomHandlesSplineStudio.s_Lighting = value;
            }
        }      

        /// <summary>
        ///   <para>Matrix for all handle operations.</para>
        /// </summary>
        public static Matrix4x4 matrix
        {
            get
            {
                return CustomHandlesSplineStudio.s_Matrix;
            }
            set
            {
                CustomHandlesSplineStudio.s_Matrix = value;
                CustomHandlesSplineStudio.s_InverseMatrix = value.inverse;
            }
        }

        /// <summary>
        ///   <para>The inverse of the matrix for all handle operations.</para>
        /// </summary>
        public static Matrix4x4 inverseMatrix
        {
            get
            {
                return CustomHandlesSplineStudio.s_InverseMatrix;
            }
        }      

        //internal static Color realHandleColor
        //{
        //    get
        //    {
        //        return CustomHandles.Color * new Color(1f, 1f, 1f, 0.5f) + (!CustomHandles.s_Lighting ? new Color(0.0f, 0.0f, 0.0f, 0.0f) : new Color(0.0f, 0.0f, 0.0f, 0.5f));
        //    }
        //}

        private static bool currentlyDragging
        {
            get
            {
                return GUIUtility.hotControl != 0;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="pIsObjSelected"></param>
        /// <returns></returns>
        public static Vector3 PositionHandle(Vector3 position, Quaternion rotation, bool pIsObjSelected, bool pAllowXZ, bool pAllowY)
        {
            return PositionHandle(position, rotation, HandleUtility.GetHandleSize(position), pIsObjSelected, pAllowXZ, pAllowY);
        }
        /// <summary>
        ///   <para>Make a 3D Scene view position handle.</para>
        /// </summary>
        /// <param name="position">Center of the handle in 3D space.</param>
        /// <param name="rotation">Orientation of the handle in 3D space.</param>
        /// <returns>
        ///         <para>The new position. If the user has not performed any operation, it will return the same value as you passed it in postion.
        /// 
        /// Note: Use HandleUtility.GetHandleSize where you might want to have constant screen-sized handles.</para>
        ///       </returns>
        public static Vector3 PositionHandle(Vector3 position, Quaternion rotation, float pHandleSize, bool pIsObjSelected, bool pAllowXZ, bool pAllowY)
        {
            return CustomHandlesSplineStudio.DoPositionHandle(position, rotation, pHandleSize, pIsObjSelected, pAllowXZ, pAllowY);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="pHandleSize"></param>
        /// <param name="pIsObjSelected"></param>
        /// <returns></returns>
        public static Vector3 DoPositionHandle(Vector3 position, Quaternion rotation, float pHandleSize, bool pIsObjSelected, bool pAllowXZ, bool pAllowY)
        {
            Event current = Event.current;
            switch (current.type)
            {
                case EventType.KeyDown:
                    if (current.keyCode == KeyCode.V && !CustomHandlesSplineStudio.currentlyDragging)
                    {
                        CustomHandlesSplineStudio.s_FreeMoveMode = true;
                        break;
                    }
                    break;
                case EventType.KeyUp:
                    position = CustomHandlesSplineStudio.DoPositionHandle_Internal(position, rotation, pHandleSize, pIsObjSelected, pAllowXZ, pAllowY);
                    if (current.keyCode == KeyCode.V && !current.shift && !CustomHandlesSplineStudio.currentlyDragging)
                        CustomHandlesSplineStudio.s_FreeMoveMode = false;
                    return position;
                case EventType.Layout:
                    if (!CustomHandlesSplineStudio.currentlyDragging)// && !Tools.vertexDragging)
                    {
                        CustomHandlesSplineStudio.s_FreeMoveMode = current.shift;
                        break;
                    }
                    break;
            }
            return CustomHandlesSplineStudio.DoPositionHandle_Internal(position, rotation, pHandleSize, pIsObjSelected, pAllowXZ, pAllowY);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="pHandleSize"></param>
        /// <param name="pIsObjSelected"></param>
        /// <returns></returns>
        private static Vector3 DoPositionHandle_Internal(Vector3 position, Quaternion rotation, float pHandleSize, bool pIsObjSelected, bool pAllowXZ, bool pAllowY)
        {
            float handleSize = pHandleSize;
            Color backupColor = Handles.color;

            bool flag = EditorApplication.isPlaying && pIsObjSelected;

            if (pAllowXZ && pAllowY)
            {
                Handles.color = !flag ? CustomHandlesSplineStudio.XAxisColor : Color.Lerp(CustomHandlesSplineStudio.XAxisColor, CustomHandlesSplineStudio.staticColor, CustomHandlesSplineStudio.staticBlend);
                GUI.SetNextControlName("xAxis");
                position = Handles.Slider(position, rotation * Vector3.right, handleSize, new Handles.CapFunction(Handles.ArrowHandleCap), SnapSettingsSplineStudio.move.x);
                Handles.color = !flag ? CustomHandlesSplineStudio.YAxisColor : Color.Lerp(CustomHandlesSplineStudio.YAxisColor, CustomHandlesSplineStudio.staticColor, CustomHandlesSplineStudio.staticBlend);
                GUI.SetNextControlName("yAxis");
                position = Handles.Slider(position, rotation * Vector3.up, handleSize, new Handles.CapFunction(Handles.ArrowHandleCap), SnapSettingsSplineStudio.move.y);
                Handles.color = !flag ? CustomHandlesSplineStudio.ZAxisColor : Color.Lerp(CustomHandlesSplineStudio.ZAxisColor, CustomHandlesSplineStudio.staticColor, CustomHandlesSplineStudio.staticBlend);
                GUI.SetNextControlName("zAxis");
                position = Handles.Slider(position, rotation * Vector3.forward, handleSize, new Handles.CapFunction(Handles.ArrowHandleCap), SnapSettingsSplineStudio.move.z);
            }
            else if(pAllowXZ)
            {
                GUI.SetNextControlName("XZ Axis");
                position = Handles.Slider2D(position, rotation * Vector3.up, rotation * Vector3.forward, rotation * Vector3.right, handleSize, (Handles.CapFunction)null, SnapSettingsSplineStudio.move.z);
            }
            else if(pAllowY)
            {
                Handles.color = !flag ? CustomHandlesSplineStudio.YAxisColor : Color.Lerp(CustomHandlesSplineStudio.YAxisColor, CustomHandlesSplineStudio.staticColor, CustomHandlesSplineStudio.staticBlend);
                GUI.SetNextControlName("yAxis");
                position = Handles.Slider(position, rotation * Vector3.up, handleSize, new Handles.CapFunction(Handles.ArrowHandleCap), SnapSettingsSplineStudio.move.y);

            }



            Handles.color = backupColor;
            return position;
        }     
    }
}
#endif