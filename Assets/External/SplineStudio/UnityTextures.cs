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
    public class UnityTextures
    {
        public static GUIStyle boldFoldOutTextStyle;
        public static GUIStyle boldTextStyle;
        public static GUIStyle boldTextStyleCenterAlign;
        public static GUIStyle boldTextStyleRightAlign;
        public static GUIStyle italicTextStyle;
        public static GUIStyle italicTextStyleRightAlign;
        public static GUIStyle italicTextStyleRed;
        public static GUIStyle smallTextStyleButton;
        public static GUIStyle rightAlignTextStyle;
        public static GUIStyle rectStyle;
        public static GUIStyle rectStyleNoPadding;
        public static GUIStyle boldTextStyleButton;
        public static GUIStyle mBillboardIdxLabelStyleSelected;
        public static GUIStyle mBillboardIdxLabelStyleNotSelected;
        public static GUIStyle mlineStyle = null;
        public static Texture mCreateIcon, mSubObjectModeIcon, mPlantIcon, mTessellateIcon, mReverseIcon, mEditColliderIcon, mVerticalSplitIcon, mAddEventIcon, mRandSizeIcon, mImageIcon, mDropDownIcon, mAddKeyFrameIcon, mAddIcon, mBillboardTypeIcon, mBoxColliderIcon, mDeleteIcon, mRefreshIcon, mSaveIcon;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIcon"></param>
        /// <returns></returns>
        private static Texture LoadIcon(UnityTextures.eIcon pIcon)
        {
            string path = System.IO.Path.GetFileNameWithoutExtension(UnityTextures.IconPaths[pIcon]);
            return (Texture)EditorGUIUtility.Load(path);
        }
        /// <summary>
        /// 
        /// </summary>
        public static void LoadStaticResources()
        {
            if (mCreateIcon.IsNull())
                mCreateIcon = LoadIcon(UnityTextures.eIcon.CreateTool);
            if (mPlantIcon.IsNull())
                mPlantIcon = LoadIcon(UnityTextures.eIcon.TerrainToolPlantBillboards);
            if (mSubObjectModeIcon.IsNull())
                mSubObjectModeIcon = LoadIcon(UnityTextures.eIcon.AssetStore);
            if (mRandSizeIcon.IsNull())
                mRandSizeIcon = LoadIcon(UnityTextures.eIcon.TerrainTreeSizeAuto);
            if (mAddIcon.IsNull())
                mAddIcon = LoadIcon(UnityTextures.eIcon.Add);
            if (mAddKeyFrameIcon.IsNull())
                mAddKeyFrameIcon = LoadIcon(UnityTextures.eIcon.AddKeyFrame);
            if (mImageIcon.IsNull())
                mImageIcon = LoadIcon(UnityTextures.eIcon.Image);
            if (mDropDownIcon.IsNull())
                mDropDownIcon = LoadIcon(UnityTextures.eIcon.DropDown2);
            if (mBillboardTypeIcon.IsNull())
                mBillboardTypeIcon = LoadIcon(UnityTextures.eIcon.BillboardType);
            if (mBoxColliderIcon.IsNull())
                mBoxColliderIcon = LoadIcon(UnityTextures.eIcon.BoxCollider);
            if (mDeleteIcon.IsNull())
                mDeleteIcon = LoadIcon(UnityTextures.eIcon.Delete);
            if (mRefreshIcon.IsNull())
                mRefreshIcon = LoadIcon(UnityTextures.eIcon.Refresh);
            if (mSaveIcon.IsNull())
                mSaveIcon = LoadIcon(UnityTextures.eIcon.Save);
            if (mVerticalSplitIcon.IsNull())
                mVerticalSplitIcon = LoadIcon(UnityTextures.eIcon.VerticalSplit);
            if (mEditColliderIcon.IsNull())
                mEditColliderIcon = LoadIcon(UnityTextures.eIcon.EditCollider);
            if (mReverseIcon.IsNull())
                mReverseIcon = LoadIcon(UnityTextures.eIcon.Reverse);
            if (mAddEventIcon.IsNull())
                mAddEventIcon = LoadIcon(UnityTextures.eIcon.AddEvent);
            if (mTessellateIcon.IsNull())
                mTessellateIcon = LoadIcon(UnityTextures.eIcon.Tessellate);



            if (boldTextStyle == null)
            {
                boldTextStyle = new GUIStyle(GUI.skin.label);
                boldTextStyle.fontStyle = FontStyle.Bold;
            }
            if (boldTextStyleCenterAlign == null)
            {
                boldTextStyleCenterAlign = new GUIStyle(GUI.skin.label);
                boldTextStyleCenterAlign.fontStyle = FontStyle.Bold;
                boldTextStyleCenterAlign.alignment = TextAnchor.MiddleCenter;
            }
            if (boldTextStyleRightAlign == null)
            {
                boldTextStyleRightAlign = new GUIStyle(GUI.skin.label);
                boldTextStyleRightAlign.fontStyle = FontStyle.Bold;
                boldTextStyleRightAlign.alignment = TextAnchor.MiddleRight;
            }

            if (boldFoldOutTextStyle == null)
            {
                boldFoldOutTextStyle = EditorStyles.foldout;
                boldFoldOutTextStyle.fontStyle = FontStyle.Bold;
            }
            if (italicTextStyle == null)
            {
                italicTextStyle = new GUIStyle(GUI.skin.label);
                italicTextStyle.fontStyle = FontStyle.Italic;
            }
            if (italicTextStyleRightAlign == null)
            {
                italicTextStyleRightAlign = new GUIStyle(GUI.skin.label);
                italicTextStyle.fontStyle = FontStyle.Italic;
                italicTextStyleRightAlign.alignment = TextAnchor.MiddleRight;
            }

            if (italicTextStyleRed == null)
            {
                italicTextStyleRed = new GUIStyle(GUI.skin.label);
                italicTextStyleRed.fontStyle = FontStyle.Bold;
                italicTextStyleRed.normal.textColor = new Color(0.9f, 0.2f, 0.2f);
            }

            if (smallTextStyleButton == null)
            {
                smallTextStyleButton = new GUIStyle(GUI.skin.button);
                smallTextStyleButton.fontSize = 8;
                smallTextStyleButton.margin = new RectOffset(0, 0, 3, 0);
            }

            if (rightAlignTextStyle == null)
            {
                rightAlignTextStyle = new GUIStyle(GUI.skin.label);
                rightAlignTextStyle.alignment = TextAnchor.MiddleRight;
            }

            if (rectStyle == null)
            {
                rectStyle = new GUIStyle(GUI.skin.box);
                rectStyle.margin = new RectOffset(2, 5, 2, 5);
                rectStyle.padding = new RectOffset(15, 2, 2, 2);
            }
            if (mlineStyle == null)
            {
                mlineStyle = new GUIStyle("box");
                mlineStyle.border.top = mlineStyle.border.bottom = 1;
                mlineStyle.margin.top = mlineStyle.margin.bottom = 1;
                mlineStyle.padding.top = mlineStyle.padding.bottom = 1;
            }
            if (rectStyleNoPadding == null)
            {
                rectStyleNoPadding = new GUIStyle(GUI.skin.box);
                rectStyleNoPadding.margin = new RectOffset(2, 5, 2, 5);
                rectStyleNoPadding.padding = new RectOffset(2, 2, 2, 2);
            }

            if (boldTextStyleButton == null)
            {
                boldTextStyleButton = new GUIStyle(GUI.skin.button);
                boldTextStyleButton.fontStyle = FontStyle.Bold;
            }

            if (mBillboardIdxLabelStyleSelected == null)
            {
                mBillboardIdxLabelStyleSelected = new GUIStyle();
                mBillboardIdxLabelStyleSelected.fontStyle = FontStyle.Bold;
                mBillboardIdxLabelStyleSelected.normal.textColor = Color.yellow;
            }
            if (mBillboardIdxLabelStyleNotSelected == null)
            {
                mBillboardIdxLabelStyleNotSelected = new GUIStyle();
                mBillboardIdxLabelStyleNotSelected.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 0.75f);
            }

        }

        public enum eIcon
        {
            Help,
            DropDown,
            Add,
            AddEvent,
            AddKeyFrame,
            NextKey,
            PrevKey,
            Play,
            Record,
            Pause,
            AssetStore,
            CameraGizmo,
            ColorCircle,
            Error,
            Info,
            Warning,
            EyeDropper,
            DropDown2,
            CreateTool,
            SelectTool,
            MoveTool,
            ScaleTool,
            RotateTool,
            TerrainToolPlants,
            TerrainToolPlantBillboards,
            TerrainToolRaise,
            TerrainToolSetHeight,
            TerrainToolTrees,
            TerrainTree,
            TerrainTreeDistribution,
            TerrainTreeDuplicate,
            TerrainTreeWind,
            TerrainTreeSizeAuto,
            Minus,
            Plus,
            Center,
            Global,
            Local,
            Pivot,
            Settings,
            BillboardType,
            BillboardTypeRand,
            Delete,
            Image,
            Refresh,
            BoxCollider,
            Save,
            VerticalSplit,
            EditCollider,
            Reverse,
            Tessellate,
        }

        /// <summary>
        /// Info:
        /// https://github.com/halak/unity-editor-icons
        /// https://gist.github.com/masa795/5797164
        /// </summary>
        public static Dictionary<eIcon, string> IconPaths = new Dictionary<eIcon, string>()
    {
        { eIcon.Help, "icons/_Help.png" },
        { eIcon.AddEvent, "icons/Animation.AddEvent.png" },
        { eIcon.AddKeyFrame, "icons/Animation.AddKeyframe.png" },
        { eIcon.NextKey, "icons/Animation.NextKey.png" },
        { eIcon.PrevKey, "icons/Animation.PrevKey.png" },
        { eIcon.Play, "icons/Animation.Play.png" },
        { eIcon.Record, "icons/Animation.Record.png" },
        { eIcon.AssetStore, "icons/Asset Store.png" },
        { eIcon.CameraGizmo, "icons/Camera Gizmo.png" },
        { eIcon.ColorCircle, "icons/ColorPicker.ColorCycle.png" },
        { eIcon.Error, "icons/console.erroricon.png" },
        { eIcon.Info, "icons/console.infoicon.png" },
        { eIcon.Warning, "icons/console.warnicon.png" },
        { eIcon.EyeDropper, "icons/d_eyeDropper.Large.png" },
        { eIcon.DropDown, "icons/d_icon dropdown.png" },
        { eIcon.SelectTool, "icons/recttool on.png" },
        { eIcon.CreateTool, "icons/treeeditor.addbranches.png" },
        { eIcon.MoveTool, "icons/MoveTool.png" },
        { eIcon.ScaleTool, "icons/ScaleTool.png" },
        { eIcon.RotateTool, "icons/RotateTool.png" },
        { eIcon.Pause, "icons/d_PauseButton.png" },
        { eIcon.TerrainToolPlants, "icons/d_TerrainInspector.TerrainToolPlants.png" },
        { eIcon.TerrainToolPlantBillboards, "icons/terraininspector.terraintoolloweralt.png" },
        { eIcon.TerrainToolRaise, "icons/d_TerrainInspector.TerrainToolRaise.png" },
        { eIcon.TerrainToolSetHeight, "icons/d_TerrainInspector.TerrainToolSetheight.png" },
        { eIcon.TerrainToolTrees, "icons/d_TerrainInspector.TerrainToolTrees.png" },
        { eIcon.TerrainTree, "icons/d_tree_icon.png" },
        { eIcon.TerrainTreeDistribution, "icons/d_TreeEditor.Distribution.png" },
        { eIcon.TerrainTreeDuplicate, "icons/d_TreeEditor.Duplicate.png" },
        { eIcon.TerrainTreeWind, "icons/d_TreeEditor.Wind.png"},
        { eIcon.TerrainTreeSizeAuto, "icons/scaletool.png" }, // "icons/treeeditor.leafscale.png" },
        { eIcon.Minus, "icons/d_Toolbar Minus.png" },
        { eIcon.Plus, "icons/Toolbar Plus.png" },
        { eIcon.Add, "icons/Toolbar Plus.png" },
        { eIcon.Center, "icons/d_ToolHandleCenter.png" },
        { eIcon.Global, "icons/d_ToolHandleGlobal.png" },
        { eIcon.Local, "icons/d_ToolHandleLocal.png" },
        { eIcon.Pivot, "icons/d_ToolHandlePivot.png" },
        { eIcon.Settings, "icons/SettingsIcon.png" },
        { eIcon.BillboardType, "icons/d_terraininspector.terraintooltrees.png" },
        { eIcon.BillboardTypeRand, "icons/d_terraininspector.terraintooltrees.png" },
        { eIcon.Delete, "icons/d_treeeditor.trash.png" },
        { eIcon.DropDown2, "icons/lookdevpaneoption.png" },
        { eIcon.Image, "icons/sceneviewfx.png" },
        { eIcon.Refresh, "icons/lookdevresetenv.png" },
        { eIcon.BoxCollider, "icons/prematcube.png" },
        { eIcon.Save, "icons/SaveActive.png" },
        { eIcon.VerticalSplit, "icons/VerticalSplit.png" },
        { eIcon.EditCollider, "icons/editcollider.png" },
        { eIcon.Reverse, "icons/LookDevMirrorViewsInactive@2x.png" },
        { eIcon.Tessellate, "icons/Grid Iconx.png" },

    };
    }
}
#endif
