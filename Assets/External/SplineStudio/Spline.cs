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
using UnityEngine;
using UnityEngine.Serialization;

namespace GraphicDNA.SplineStudio
{

    [ExecuteInEditMode]//, RequireComponent(typeof(LineRenderer))]
    public class Spline : MonoBehaviour
    {
        public enum eSplineType
        {
            Linear,
            Hermite,
            Bezier,
            CatmullRom,
        }
        public enum eInterpolationType
        {
            FixedDisplacement,
            FixedNumSteps,
        }
        public enum eRenderType
        {
            UnityLineRenderer,
            CustomMesh,
        }
        public enum eVisibility
        {
            /// <summary>
            /// The spline will never be rendered
            /// </summary>
            None,
            /// <summary>
            /// the spline will be visible only when inside the Unity Editor (and when not playing the game)
            /// </summary>
            EditorOnly,
            /// <summary>
            /// The spline will be always visible
            /// </summary>
            Always,
        }

        private LineRenderer mLineRender;
        private MeshRenderer mMeshRender;
        private MeshFilter mMeshFilter;

        /// <summary>
        /// Sets when the spline is rendered
        /// </summary>
        public eVisibility Visibility = eVisibility.Always;
        /// <summary>
        /// Selects how to render the spline in the editor
        /// </summary>
        public eRenderType RenderMode = eRenderType.CustomMesh;
        /// <summary>
        /// Material to be used for rendering
        /// </summary>
        public Material Material;
        public bool CustomMeshSwapUV;
        public int CustomMeshUVCoordsChannel = 0;
        public float CustomMeshUTileFactor = 1;
        public float CustomMeshVTileWorldSize = 1;
        public Vector3 CustomMeshOffset = Vector3.zero;
        /// <summary>
        /// Control points, with coordinates defined in local space
        /// </summary>
        public List<ControlPoint> ControlPoints = new List<ControlPoint>();
        /// <summary>
        /// Final, interpolated points in local space
        /// </summary>
        [HideInInspector]
        public List<FinalPoint> FinalPoints = new List<FinalPoint>();
        /// <summary>
        /// Used when InterpolationType == FixedNumSteps. Number of interpolation steps between each control point (doesn't apply if CurveType is set to Linear)
        /// </summary>
        public int InterpolationSteps = 20;
        /// <summary>
        /// Used when InterpolationType == FixedDisplacement. Displacement (in meters) between interpolation points (doesn't apply if CurveType is set to Linear)
        /// </summary>
        public float InterpolationWorldStep = 0.1f;
        /// <summary>
        /// Type of interpolation 
        /// </summary>
        public eInterpolationType InterpolationType = eInterpolationType.FixedDisplacement;
        /// <summary>
        /// True to enable moving handles in XZ inside Unity Editor
        /// </summary>
        public bool EditHandlesXZ = true;
        /// <summary>
        /// True to enable moving handles in Y inside Unity Editor
        /// </summary>
        public bool EditHandlesY = false;
        /// <summary>
        /// Type of curve
        /// </summary>
        public eSplineType CurveType = eSplineType.Bezier;
        public Color Color = Color.white;
        public float Width = 0.2f;
        public float Thickness = 0f;
        public bool GenerateSidesGeometry = true;
        public float TotalLength = 0f;
        public float mManualModeAddPlaneHeight = 0f;
        public bool IsClosed = false;
        public bool AppendPointsOnCreation = true;
        public bool AlwaysRenderControlPoints = false;
        public float InitialHandleDist = 4;
        [HideInInspector]
        public double mLastRefreshTime;

        public Material mDefaultMaterial;
        public Material DefaultMaterial
        {
            get
            {
                if (mDefaultMaterial.IsNull())
                {
                    Shader shader = Shader.Find("Standard");
                    mDefaultMaterial = new Material(shader);
                }

                return mDefaultMaterial;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            RefreshRenderObjs();
        }
        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            Refresh();

#if (UNITY_EDITOR)
            UnityEditor.Undo.undoRedoPerformed -= MyUndoCallback;
            UnityEditor.Undo.undoRedoPerformed += MyUndoCallback;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            //#if(UNITY_EDITOR)
            //        Refresh();
            //#endif
        }
#if (UNITY_EDITOR)
        /// <summary>
        /// 
        /// </summary>
        void MyUndoCallback()
        {
            this.Refresh();
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        private void DestroyCustomMeshObjs()
        {
            // Remove other renderers
            mMeshRender = this.GetComponent<MeshRenderer>();
            if (!mMeshRender.IsNull())
                GameObject.DestroyImmediate(mMeshRender);
            mMeshFilter = GetComponent<MeshFilter>();
            if (!mMeshFilter.IsNull())
                GameObject.DestroyImmediate(mMeshFilter);
            mMeshRender = null;
            mMeshFilter = null;
        }
        /// <summary>
        /// 
        /// </summary>
        private void DestroyLineRenderObjs()
        {
            // Disable Line Renderer
            mLineRender = GetComponent<LineRenderer>();
            if (!mLineRender.IsNull())
                GameObject.DestroyImmediate(mLineRender);
            mLineRender = null;
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshRenderObjs()
        {
            if (this.Visibility == eVisibility.None ||
               (this.Visibility == eVisibility.EditorOnly && (!Application.isEditor || Application.isPlaying)))
            {
                DestroyCustomMeshObjs();
                DestroyLineRenderObjs();
                return;
            }

            switch (this.RenderMode)
            {
                case eRenderType.UnityLineRenderer:
                    DestroyCustomMeshObjs();

                    // Create LineRenderer
                    mLineRender = GetComponent<LineRenderer>();
                    if (mLineRender.IsNull())
                        mLineRender = this.gameObject.AddComponent<LineRenderer>();
                    mLineRender.useWorldSpace = false;
                    mLineRender.material = Material.IsNull() ? DefaultMaterial : Material;
                    break;
                case eRenderType.CustomMesh:
                    DestroyLineRenderObjs();

                    // Set mesh renderer
                    mMeshRender = this.GetComponent<MeshRenderer>();
                    if (mMeshRender.IsNull())
                        mMeshRender = this.gameObject.AddComponent<MeshRenderer>();
                    mMeshRender.sharedMaterial = Material.IsNull() ? DefaultMaterial : Material;

                    // Set mesh to meshfilter
                    mMeshFilter = GetComponent<MeshFilter>();
                    if (mMeshFilter.IsNull())
                        mMeshFilter = this.gameObject.AddComponent<MeshFilter>();
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        public void ResetHandles(int i)
        {

            if (i >= this.ControlPoints.Count - 1)
            {
                this.ControlPoints[i].ControlHandleIn = (this.ControlPoints[i - 1].Position - this.ControlPoints[i].Position).normalized * InitialHandleDist;
                if (this.ControlPoints[i].ControlPointMode != ControlPoint.eControlPointMode.Free)
                    this.ControlPoints[i].HandleInChanged();
            }
            else if (i == 0)
            {
                this.ControlPoints[i].ControlHandleOut = (this.ControlPoints[i + 1].Position - this.ControlPoints[i].Position).normalized * InitialHandleDist;
                if (this.ControlPoints[i].ControlPointMode != ControlPoint.eControlPointMode.Free)
                    this.ControlPoints[i].HandleOutChanged();
            }
            else
            {
                Vector3 dirNext = (this.ControlPoints[i + 1].Position - this.ControlPoints[i].Position).normalized;
                Vector3 dirPrev = (this.ControlPoints[i].Position - this.ControlPoints[i - 1].Position).normalized;
                this.ControlPoints[i].ControlHandleOut = (dirNext + dirPrev).normalized * InitialHandleDist;
                if (this.ControlPoints[i].ControlPointMode != ControlPoint.eControlPointMode.Free)
                    this.ControlPoints[i].HandleOutChanged();
            }


            //if (i < this.ControlPoints.Count - 1)
            //{
            //    this.ControlPoints[i].ControlHandleOut = (this.ControlPoints[i + 1].Position - this.ControlPoints[i].Position).normalized * InitialHandleDist;
            //    if (this.ControlPoints[i].ControlPointMode != ControlPoint.eControlPointMode.Free)
            //        this.ControlPoints[i].HandleOutChanged();
            //}
            //if (i > 0)
            //{
            //    this.ControlPoints[i].ControlHandleIn = (this.ControlPoints[i - 1].Position - this.ControlPoints[i].Position).normalized * InitialHandleDist;
            //    if (this.ControlPoints[i].ControlPointMode != ControlPoint.eControlPointMode.Free)
            //        this.ControlPoints[i].HandleInChanged();
            //}


        }

        /// <summary>
        /// 
        /// </summary>
        public void Refresh()
        {            
            RefreshSplinePoints();

            RefreshFinalPointsInfo();

            RefreshVisuals();
 #if UNITY_EDITOR
            mLastRefreshTime = UnityEditor.EditorApplication.timeSinceStartup;
 #endif
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshVisuals()
        {
            if (this.Visibility == eVisibility.None)
                return;
            if (this.Visibility == eVisibility.EditorOnly && (!Application.isEditor || Application.isPlaying))
                return;

            switch (this.RenderMode)
            {
                case eRenderType.CustomMesh:
                    RefreshCustomMesh();
                    break;
                case eRenderType.UnityLineRenderer:
                    RefreshLineRenderer();
                    break;
            }
        }
        ///// <summary>
        ///// 
        ///// </summary>
        //private Vector3 OffsetVertex(Vector3 initialPos, Vector3 axisX, Vector3 axisY, Vector3 axisZ, float pSideOffset)
        //{
        //    return initialPos + (axisX * pSideOffset);
        //}
        /// <summary>
        /// 
        /// </summary>
        private void RefreshLineRenderer()
        {
            if (this.ControlPoints.Count < 2)
            {
                mLineRender.positionCount = 0;
                return;
            }


            mLineRender.startColor = Color;
            mLineRender.endColor = Color;
            mLineRender.startWidth = Width;
            mLineRender.endWidth = Width;

            // Refresh Line renderer
            mLineRender.positionCount = this.FinalPoints.Count;
            List<Vector3> positions = new List<Vector3>();
            foreach (FinalPoint pt in this.FinalPoints)
                positions.Add(pt.Position);

            mLineRender.SetPositions(positions.ToArray());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected void RefreshCustomMesh()
        {
            if (!mMeshFilter.IsNull() && !mMeshFilter.sharedMesh.IsNull())
                GameObject.DestroyImmediate(mMeshFilter.sharedMesh);
            mMeshFilter.sharedMesh = null;

            if (this.FinalPoints.Count < 2)
                return;


            Mesh newMesh = new Mesh();

            // Prevent mesh from being saved with the scene
            newMesh.hideFlags = HideFlags.DontSave;

            // If we are in the editor, optimize mesh to be dynamic 
#if (UNITY_EDITOR)
            newMesh.MarkDynamic();
#endif

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();
            List<Color> colors = new List<Color>();
            Bounds meshBounds = new Bounds();
            float halfWidh = Width * 0.5f;
            int vIdx = 0;
            float tileV = CustomMeshVTileWorldSize > 0 ? CustomMeshVTileWorldSize : 1;
            float v = 0;
            float stepV;
            FinalPoint pt1, pt2;
            Color col = new Color(1, 1, 1);
            float halfTileU = 0.5f * CustomMeshUTileFactor;
            Vector3 leftOffset = new Vector3(-halfWidh, 0, 0) + CustomMeshOffset;
            Vector3 rightOffset = new Vector3(halfWidh, 0, 0) + CustomMeshOffset;

            //          v1           pt2        v2
            //          |-----------X----------|
            //          |---        |          |
            //          |   ----    |          |
            //          |       ----|          |
            //          |           | -----    |
            //          |-----------X----------|
            //          v0           pt1        v3

            // Add First pair of vertices (V0, v3)
            {
                pt1 = FinalPoints[0];
                pt2 = FinalPoints[1];
                Vector3 v0 = pt1.GetOffsetPoint(leftOffset);
                Vector3 v3 = pt1.GetOffsetPoint(rightOffset);

                vertices.Add(v0);
                vertices.Add(v3);
                vIdx += 2;

                // uvs
                if (CustomMeshSwapUV)
                {
                    uvs.Add(new Vector2(v, 0));                             // v0
                    uvs.Add(new Vector2(v, CustomMeshUTileFactor));         // v3
                }
                else
                {
                    uvs.Add(new Vector2(0, v));                             // v0
                    uvs.Add(new Vector2(CustomMeshUTileFactor, v));         // v3
                }
                stepV = pt1.NextSegmentLen / tileV;
                v += stepV;

                // Colors
                colors.Add(col);
                colors.Add(col);

                // Bounds
                meshBounds.Encapsulate(v0);
                meshBounds.Encapsulate(v3);
            }

            // Iterate for the rest
            for (int i = 0; i < FinalPoints.Count - 1; i++)
            {
                pt1 = FinalPoints[i];
                pt2 = FinalPoints[i + 1];

                Vector3 v1 = FinalPoint.GetAveragedOffsetPoint(pt1, pt2, leftOffset);
                Vector3 v2 = FinalPoint.GetAveragedOffsetPoint(pt1, pt2, rightOffset);

                vertices.Add(v1);
                vertices.Add(v2);
                vIdx += 2;

                // Tri 1
                indices.Add(vIdx - 3); // v3 Index
                indices.Add(vIdx - 2); // v1 Index
                indices.Add(vIdx - 4); // v0 Index

                // Tri 2
                indices.Add(vIdx - 3); // v3 Index
                indices.Add(vIdx - 1); // v2 Index
                indices.Add(vIdx - 2); // v1 Index

                // uvs
                if (CustomMeshSwapUV)
                {
                    uvs.Add(new Vector2(v + stepV, 0));                     // v1
                    uvs.Add(new Vector2(v + stepV, CustomMeshUTileFactor)); // v2
                }
                else
                {
                    uvs.Add(new Vector2(0, v + stepV));                     // v1       
                    uvs.Add(new Vector2(CustomMeshUTileFactor, v + stepV)); // v2                
                }
                stepV = pt2.NextSegmentLen / tileV;
                v += stepV;

                // colors
                colors.Add(col);
                colors.Add(col);

                // Bounds
                meshBounds.Encapsulate(v1);
                meshBounds.Encapsulate(v2);
            }

            int frontSideVertCount = vertices.Count;
            int frontSideIndexCount = indices.Count;

            if (Thickness > 0)
            {
                v = 0;
                Vector3 leftOffsetBack = new Vector3(-halfWidh, -Thickness, 0) + CustomMeshOffset;
                Vector3 rightOffsetBack = new Vector3(halfWidh, -Thickness, 0) + CustomMeshOffset;

                // Add First pair of backside verts
                {
                    Vector3 v0B = pt1.GetOffsetPoint(rightOffsetBack);
                    Vector3 v3B = pt1.GetOffsetPoint(leftOffsetBack);

                    // Vertices
                    vertices.Add(v0B);
                    vertices.Add(v3B);
                    vIdx += 2;

                    // uvs
                    if (CustomMeshSwapUV)
                    {
                        uvs.Add(new Vector2(v, 0));
                        uvs.Add(new Vector2(v, CustomMeshUTileFactor));
                    }
                    else
                    {
                        uvs.Add(new Vector2(0, v));
                        uvs.Add(new Vector2(CustomMeshUTileFactor, v));
                    }
                    stepV = pt1.NextSegmentLen / tileV;
                    v += stepV;

                    // colors
                    colors.Add(col);
                    colors.Add(col);

                    // Bounds
                    meshBounds.Encapsulate(v0B);
                    meshBounds.Encapsulate(v3B);
                }

                // Iterate for the rest
                for (int i = 0; i < FinalPoints.Count - 1; i++)
                {
                    pt1 = FinalPoints[i];
                    pt2 = FinalPoints[i + 1];

                    Vector3 v1B = FinalPoint.GetAveragedOffsetPoint(pt1, pt2, rightOffsetBack);
                    Vector3 v2B = FinalPoint.GetAveragedOffsetPoint(pt1, pt2, leftOffsetBack);

                    // Vertices
                    vertices.Add(v1B);
                    vertices.Add(v2B);
                    vIdx += 2;

                    // Tri 1
                    indices.Add(vIdx - 3); // v3 Index
                    indices.Add(vIdx - 2); // v1 Index
                    indices.Add(vIdx - 4); // v0 Index

                    // Tri 2
                    indices.Add(vIdx - 3); // v3 Index
                    indices.Add(vIdx - 1); // v2 Index
                    indices.Add(vIdx - 2); // v1 Index

                    if (GenerateSidesGeometry)
                    {
                        int iv0F = vIdx - 4 - frontSideVertCount;
                        int iv1F = vIdx - 2 - frontSideVertCount;
                        int iv2F = vIdx - 1 - frontSideVertCount;
                        int iv3F = vIdx - 3 - frontSideVertCount;

                        int iv0B = vIdx - 4;
                        int iv1B = vIdx - 2;
                        int iv2B = vIdx - 1;
                        int iv3B = vIdx - 3;

                        // Tri 1 Side1
                        indices.Add(iv3B);
                        indices.Add(iv1F);
                        indices.Add(iv2B);
                        // Tri 2 Side1
                        indices.Add(iv3B);
                        indices.Add(iv0F);
                        indices.Add(iv1F);

                        // Tri 1 Side2
                        indices.Add(iv1B);
                        indices.Add(iv3F);
                        indices.Add(iv0B);
                        // Tri 2 Side2
                        indices.Add(iv1B);
                        indices.Add(iv2F);
                        indices.Add(iv3F);
                    }

                    // uvs
                    if (CustomMeshSwapUV)
                    {
                        uvs.Add(new Vector2(v + stepV, 0));
                        uvs.Add(new Vector2(v + stepV, CustomMeshUTileFactor));
                    }
                    else
                    {
                        uvs.Add(new Vector2(0, v + stepV));
                        uvs.Add(new Vector2(CustomMeshUTileFactor, v + stepV));
                    }

                    stepV = pt2.NextSegmentLen / tileV;
                    v += stepV;

                    // colors
                    colors.Add(col);
                    colors.Add(col);

                    // Bounds
                    meshBounds.Encapsulate(v1B);
                    meshBounds.Encapsulate(v2B);
                }



            }




            // Set Index format so we support more than 65535 verts
            newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            // Set Vertices
            newMesh.SetVertices(vertices);

            // Important: indices must be set with this method, so we are able to indicate the type of topology of the mesh (Points, not triangles).
            // If we use the property "mesh.indices =", the topology is assumed to be triangles 
            newMesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

            // UV Coords
            newMesh.SetUVs(CustomMeshUVCoordsChannel, uvs);

            // Use color information to pass in TreeSize and TextureAtlas index
            newMesh.SetColors(colors);

            // Unity uses mesh.bounds to determine if an object is visible
            newMesh.bounds = meshBounds;

            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();

            mMeshFilter.sharedMesh = newMesh;
            mMeshRender.sharedMaterial = Material.IsNull() ? DefaultMaterial : Material;

        }
        /// <summary>
        /// 
        /// </summary>
        public void TessellateControlPointsx2()
        {
            List<ControlPoint> newPoints = new List<ControlPoint>();
            for (int i = 0; i < this.ControlPoints.Count - 1; i++)
            {
                ControlPoint newCP = this.ControlPoints[i].Clone();
                newCP.Position = (this.ControlPoints[i].Position + this.ControlPoints[i + 1].Position) * 0.5f;
                newPoints.Add(newCP);
            }

            int idx = 1;
            foreach (ControlPoint cp in newPoints)
            {
                this.ControlPoints.Insert(idx, cp);
                this.ResetHandles(idx);
                idx += 2;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void RemoveDuplicatePoints()
        {
            List<ControlPoint> toDelete = new List<ControlPoint>();
            for (int i = 0; i < this.ControlPoints.Count - 1; i++)
            {
                ControlPoint cp1 = this.ControlPoints[i];
                ControlPoint cp2 = this.ControlPoints[i + 1];
                float distSq = (cp1.Position - cp2.Position).sqrMagnitude;
                if (distSq < 0.0001f)
                    toDelete.Add(cp1);
            }

            foreach (ControlPoint cp in toDelete)
                this.ControlPoints.Remove(cp);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPos"></param>
        private void AddFinalPoint(Vector3 pPos, float pCamber, int? pControlPointIdx)
        {
            FinalPoint pt = new FinalPoint();
            pt.Position = pPos;
            pt.Camber = pCamber;
            pt.ControlPointIdx = pControlPointIdx;
            FinalPoints.Add(pt);
            // RefreshFinalPointsInfo();
        }
        /// <summary>
        /// 
        /// </summary>
        private void RefreshFinalPointsInfo()
        {
            if (this.FinalPoints.Count <= 0)
                return;

            this.TotalLength = 0;
            for (int i = 0; i < this.FinalPoints.Count - 1; i++)
            {
                Vector3 segmentStart = this.FinalPoints[i].Position;
                Vector3 segmentEnd = this.FinalPoints[i + 1].Position;
                Vector3 segmentDif = (segmentEnd - segmentStart);
                float segmentLen = segmentDif.magnitude;

                Vector3 segmentDir = Vector3.zero;
                if (segmentLen > 0)
                    segmentDir = segmentDif / segmentLen;

                this.FinalPoints[i].NextSegmentLen = segmentLen;
                this.FinalPoints[i].SegmentAxisZ = segmentDir;
                this.FinalPoints[i].SegmentAxisY = Quaternion.AngleAxis(this.FinalPoints[i].Camber, segmentDir) * Vector3.up;
                this.FinalPoints[i].SegmentAxisX = Vector3.Cross(segmentDir, this.FinalPoints[i].SegmentAxisY);

                this.FinalPoints[i].DistanceFromOrigin = TotalLength;
                TotalLength += segmentLen;

                if (!this.FinalPoints[i].ControlPointIdx.HasValue && i > 0)
                    this.FinalPoints[i].ControlPointIdx = this.FinalPoints[i - 1].ControlPointIdx;
            }

            // Set data to LastPoint
            this.FinalPoints[this.FinalPoints.Count - 1].DistanceFromOrigin = TotalLength;
            if (this.IsClosed)
            {
                this.FinalPoints[this.FinalPoints.Count - 1].SegmentAxisZ = this.FinalPoints[0].SegmentAxisZ;
                this.FinalPoints[this.FinalPoints.Count - 1].SegmentAxisY = this.FinalPoints[0].SegmentAxisY;
                this.FinalPoints[this.FinalPoints.Count - 1].SegmentAxisX = this.FinalPoints[0].SegmentAxisX;
                this.FinalPoints[this.FinalPoints.Count - 1].NextSegmentLen = this.FinalPoints[0].NextSegmentLen;
            }
            else
            {
                this.FinalPoints[this.FinalPoints.Count - 1].SegmentAxisZ = this.FinalPoints[this.FinalPoints.Count - 2].SegmentAxisZ;
                this.FinalPoints[this.FinalPoints.Count - 1].SegmentAxisY = Quaternion.AngleAxis(this.FinalPoints[this.FinalPoints.Count - 1].Camber, this.FinalPoints[this.FinalPoints.Count - 1].SegmentAxisZ) * Vector3.up;
                this.FinalPoints[this.FinalPoints.Count - 1].SegmentAxisX = Vector3.Cross(this.FinalPoints[this.FinalPoints.Count - 1].SegmentAxisZ, this.FinalPoints[this.FinalPoints.Count - 1].SegmentAxisY);
                this.FinalPoints[this.FinalPoints.Count - 1].NextSegmentLen = 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStartIdx"></param>
        /// <param name="index"></param>
        /// <param name="position"></param>
        /// <param name="segmentDirection"></param>
        private void ReturnMilestoneParameters(int pStartIdx, Vector3 positionLocalOffset, out int index, out Vector3 position, out Vector3 segmentAxisX, out Vector3 segmentAxisY, out Vector3 segmentDirection)
        {
            index = pStartIdx;
            Vector3 localOffset = Vector3.zero;
            if (positionLocalOffset != Vector3.zero)
                localOffset = GetLocalOffset(this.FinalPoints[pStartIdx].SegmentAxisY, this.FinalPoints[pStartIdx].SegmentAxisZ, positionLocalOffset);
            position = this.transform.TransformPoint(this.FinalPoints[pStartIdx].Position + localOffset);
            segmentDirection = this.transform.TransformDirection(this.FinalPoints[pStartIdx].SegmentAxisZ);
            segmentAxisX = this.transform.TransformDirection(this.FinalPoints[pStartIdx].SegmentAxisX);
            segmentAxisY = this.transform.TransformDirection(this.FinalPoints[pStartIdx].SegmentAxisY);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public void GetWorldPositionFromT(float t, Vector3 positionLocalOffset, out int index, out Vector3 position, out Vector3 segmentAxisX, out Vector3 segmentAxisY, out Vector3 segmentDirection)
        {
            if (t <= 0)
            {
                ReturnMilestoneParameters(0, positionLocalOffset, out index, out position, out segmentAxisX, out segmentAxisY, out segmentDirection);
                return;
            }
            if (t >= 1)
            {
                ReturnMilestoneParameters(this.FinalPoints.Count - 1, positionLocalOffset, out index, out position, out segmentAxisX, out segmentAxisY, out segmentDirection);
                return;
            }

            float dist = this.TotalLength * Mathf.Clamp01(t);
            GetWorldPositionFromDist(dist, positionLocalOffset, out index, out position, out segmentAxisX, out segmentAxisY, out segmentDirection);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="localOffset"></param>
        /// <returns></returns>
        public static Vector3 GetLocalOffset(Vector3 axisY, Vector3 axisZ, Vector3 localOffset)
        {
            Quaternion q = Quaternion.LookRotation(axisZ, axisY);
            return q * localOffset;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPos"></param>
        /// <param name="pCamber"></param>
        private void AddControlPoint(Vector3 pPos, float pCamber)
        {
            // Ensure that we are not repeating the last point added
            if (ControlPoints.Count > 0)
            {
                Vector3 dif = pPos - ControlPoints[ControlPoints.Count - 1].Position;
                if (dif.sqrMagnitude < 0.001f)
                    return;
            }

            ControlPoints.Add(new ControlPoint(pPos, pCamber));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public void GetWorldPositionFromDist(float dist, Vector3 positionLocalOffset, out int index, out Vector3 position, out Vector3 segmentAxisX, out Vector3 segmentAxisY, out Vector3 segmentAxisZ)
        {
            if (dist <= 0)
            {
                ReturnMilestoneParameters(0, positionLocalOffset, out index, out position, out segmentAxisX, out segmentAxisY, out segmentAxisZ);
                return;
            }
            if (dist >= TotalLength)
            {
                ReturnMilestoneParameters(this.FinalPoints.Count - 1, positionLocalOffset, out index, out position, out segmentAxisX, out segmentAxisY, out segmentAxisZ);
                return;
            }

            int startIdx = 0;
            bool found = false;
            for (int i = 0; i < this.FinalPoints.Count - 1; i++)
            {
                if (this.FinalPoints[i + 1].DistanceFromOrigin > dist)
                {
                    found = true;
                    startIdx = i;
                    break;
                }
            }

            if (!found || startIdx >= FinalPoints.Count - 1)
            {
                ReturnMilestoneParameters(this.FinalPoints.Count - 1, positionLocalOffset, out index, out position, out segmentAxisX, out segmentAxisY, out segmentAxisZ);
                return;
            }

            index = startIdx;
            Vector3 localOffset = Vector3.zero;
            if (positionLocalOffset != Vector3.zero)
                localOffset = GetLocalOffset(this.FinalPoints[startIdx].SegmentAxisY, this.FinalPoints[startIdx].SegmentAxisZ, positionLocalOffset);


            Vector3 start = this.FinalPoints[startIdx].Position;
            Vector3 end = this.FinalPoints[startIdx + 1].Position;
            float difDist = dist - this.FinalPoints[startIdx].DistanceFromOrigin;
            float t = Mathf.Clamp01(difDist / this.FinalPoints[startIdx].NextSegmentLen);
            Vector3 localPos = Vector3.Lerp(start, end, t) + localOffset;
            position = this.transform.TransformPoint(localPos);
            segmentAxisZ = this.transform.TransformDirection(this.FinalPoints[startIdx].SegmentAxisZ);
            segmentAxisX = this.transform.TransformDirection(this.FinalPoints[startIdx].SegmentAxisX);
            segmentAxisY = this.transform.TransformDirection(this.FinalPoints[startIdx].SegmentAxisY);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private int GetNextPoint(int idx)
        {
            int newIdx = idx + 1;
            if (newIdx > ControlPoints.Count - 1)
                newIdx = newIdx % ControlPoints.Count;
            return newIdx;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private int GetPrevPoint(int idx)
        {
            int newIdx = idx - 1;
            while (newIdx < 0)
                newIdx += ControlPoints.Count;
            return newIdx;
        }


        #region Intersections & Utils
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="toRight"></param>
        /// <returns></returns>
        public static float GetAngleBetween(Vector3 from, Vector3 to, Vector3 toRight)
        {
            float forwardDot = Vector3.Dot(from, to);
            float rightDot = Vector3.Dot(from, toRight);

            // Keep dot in range to prevent rounding errors       
            forwardDot = Mathf.Clamp(forwardDot, -1.0f, 1.0f);
            float angle = Mathf.Acos(forwardDot);

            if (rightDot < 0.0f)
                angle *= -1.0f;

            return angle;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ClosestPointOnSegment(Vector3 p1, Vector3 p2, Vector3 point, ref bool pPointWasInsideSegment)
        {
            Vector3 nearest = Vector3.zero;

            float dot_ta, dot_tb;

            // Miro si el angulo entre la dir del segmento y point-p1 es obtuso. Si es así, el punto está
            // más alla de p1, por lo que el punto más cercano es p1
            Vector3 ca = point - p1;
            Vector3 segDir = p2 - p1;
            dot_ta = Vector3.Dot(ca, segDir);
            if (dot_ta <= 0)
            {
                nearest = p1;
                pPointWasInsideSegment = false;
                return nearest;
            }

            // Mirar si se pasa por p2
            Vector3 cb = point - p2;
            dot_tb = Vector3.Dot(cb, -segDir);
            if (dot_tb <= 0)
            {
                nearest = p2;
                pPointWasInsideSegment = false;
                return nearest;
            }

            // FIND THE REAL NEAREST POINT ON THE LINE SEGMENT -      BASED ON RATIO
            pPointWasInsideSegment = true;
            float dot_tatb = dot_ta + dot_tb;
            nearest = p1 + ((segDir * dot_ta) * (1f / dot_tatb));
            return nearest;
        }
        /// <summary>
        /// Calculates the closest point to a line segment, in 2D (designed to work with on-screen pixel coordinates)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pnt"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private Vector2 ClosestPointOnLine(Vector2 start, Vector2 end, Vector2 pnt, out float t)
        {
            var segmetnDir = (end - start);
            var len = segmetnDir.magnitude;
            segmetnDir.Normalize();
            var v = pnt - start;
            var d = Vector3.Dot(v, segmetnDir);
            t = d / len;
            if (d < 0)
                return start;
            if (d > len)
                return end;
            return start + segmetnDir * d;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="hitInfo"></param>
        /// <returns></returns>
        public bool MouseCast(Vector2 worldMouse, Transform pTransform, Camera pCamera, out Vector3 pHitPoint, out int pStartIdx)
        {
            pHitPoint = Vector3.zero;
            pStartIdx = 0;

            float minDist = float.MaxValue;
            Vector3 minPt = Vector3.zero;
            int minIdx = -1;
            bool foundPt = false;
            for (int i = 0; i < ControlPoints.Count - 1; i++)
            {
                float t = 0;

                Vector3 p1 = ControlPoints[i].Position;
                Vector3 p2 = ControlPoints[i + 1].Position;
                if (!pTransform.IsNull())
                {
                    p1 = pTransform.TransformPoint(p1);
                    p2 = pTransform.TransformPoint(p2);
                }

                Vector2 p1Screen = pCamera.WorldToScreenPoint(p1);
                Vector2 p2Screen = pCamera.WorldToScreenPoint(p2);

                Vector2 closestPt = ClosestPointOnLine(p1Screen, p2Screen, worldMouse, out t);
                if (t < 0 || t > 1)
                    continue;
                float dist = (closestPt - worldMouse).magnitude;
                if (dist < minDist)
                {
                    minIdx = i;
                    minDist = dist;
                    minPt = p1 + ((p2 - p1) * t);
                    foundPt = true;
                }
            }
            if (foundPt)
            {
                pHitPoint = minPt;
                pStartIdx = minIdx;
                return true;
            }
            return false;
        }
        #endregion

        #region Curve Algorithms
        /// <summary>
        /// 
        /// </summary>
        public void RefreshSplinePoints()
        {
            FinalPoints.Clear();

            // We need at least 2 control points to run the algorithm
            if (ControlPoints == null || ControlPoints.Count < 2)
                return;

            // Number of steps should be at least 2. If not, just a plain linear line is returned
            if (InterpolationSteps < 2)
            {
                for (int j = 0; j < ControlPoints.Count; j++)
                    AddFinalPoint(ControlPoints[j].Position, ControlPoints[j].Camber, j);
                return;
            }

            switch (this.CurveType)
            {
                case eSplineType.Linear:
                    AddPointsLinear();
                    break;
                case eSplineType.Hermite:
                    AddPointsHermite();
                    break;
                case eSplineType.Bezier:
                    AddPointsBezier();
                    break;
                case eSplineType.CatmullRom:
                    AddPointsCatmullRom();
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="j"></param>
        /// <param name="numSteps"></param>
        /// <param name="tStep"></param>
        private void GetStepParameters(int idxStart, int idxEnd, out int numSteps, out float tStep)
        {
            float segmentLen;
            if (this.InterpolationType == eInterpolationType.FixedNumSteps)
            {
                numSteps = InterpolationSteps;
                tStep = 1;
                if (InterpolationSteps > 0)
                    tStep = 1.0f / InterpolationSteps;
            }
            else
            {
                Vector3 segmentStart = ControlPoints[idxStart].Position;
                Vector3 segmentEnd = ControlPoints[idxEnd].Position;
                segmentLen = (segmentEnd - segmentStart).magnitude;

                numSteps = 1;
                if (InterpolationWorldStep > 0)
                    numSteps = (int)Math.Max(1, segmentLen / InterpolationWorldStep);

                tStep = 1;
                if (segmentLen > 0)
                    tStep = InterpolationWorldStep / segmentLen;
            }


        }
        /// <summary>
        /// 
        /// </summary>
        private void AddPointsLinear()
        {
            for (int j = 0; j < ControlPoints.Count; j++)
                AddFinalPoint(ControlPoints[j].Position, ControlPoints[j].Camber, j);

            if (IsClosed)
                AddFinalPoint(ControlPoints[0].Position, ControlPoints[0].Camber, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        private void AddSegmentHermite(int idxStart, int idxEnd, bool isLastSegment)
        {
            Vector3 p0, p1, m0, m1;
            float c0, c1, cm0, cm1;

            // determine control points of segment
            p0 = ControlPoints[idxStart].Position;
            p1 = ControlPoints[idxEnd].Position;
            c0 = ControlPoints[idxStart].Camber;
            c1 = ControlPoints[idxEnd].Camber;
            if (!IsClosed)
            {
                if (idxStart > 0)
                {
                    m0 = 0.5f * (ControlPoints[idxEnd].Position - ControlPoints[idxStart - 1].Position);
                    cm0 = 0.5f * (ControlPoints[idxEnd].Camber - ControlPoints[idxStart - 1].Camber);
                }
                else
                {
                    m0 = ControlPoints[idxEnd].Position - ControlPoints[idxStart].Position;
                    cm0 = ControlPoints[idxEnd].Camber - ControlPoints[idxStart].Camber;
                }
                if (idxEnd < ControlPoints.Count - 1)
                {
                    m1 = 0.5f * (ControlPoints[idxEnd + 1].Position - ControlPoints[idxStart].Position);
                    cm1 = 0.5f * (ControlPoints[idxEnd + 1].Camber - ControlPoints[idxStart].Camber);
                }
                else
                {
                    m1 = ControlPoints[idxEnd].Position - ControlPoints[idxStart].Position;
                    cm1 = ControlPoints[idxEnd].Camber - ControlPoints[idxStart].Camber;
                }
            }
            else
            {
                m0 = 0.5f * (ControlPoints[idxEnd].Position - ControlPoints[GetPrevPoint(idxStart)].Position);
                m1 = 0.5f * (ControlPoints[GetNextPoint(idxEnd)].Position - ControlPoints[idxStart].Position);

                cm0 = 0.5f * (ControlPoints[idxEnd].Camber - ControlPoints[GetPrevPoint(idxStart)].Camber);
                cm1 = 0.5f * (ControlPoints[GetNextPoint(idxEnd)].Camber - ControlPoints[idxStart].Camber);
            }

            // set points of Hermite curve
            Vector3 position;
            float camber;
            int numSteps;
            float tStep;
            float segmentLen;
            if (this.InterpolationType == eInterpolationType.FixedNumSteps)
            {
                numSteps = InterpolationSteps;
                tStep = 1.0f / InterpolationSteps;
            }
            else
            {
                segmentLen = (p1 - p0).magnitude;
                numSteps = (int)Math.Max(1, segmentLen / InterpolationWorldStep);
                tStep = InterpolationWorldStep / segmentLen;
            }

            float t = 0;
            for (int i = 0; i < numSteps; i++)
            {

                position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p0 +
                           (t * t * t - 2.0f * t * t + t) * m0 +
                           (-2.0f * t * t * t + 3.0f * t * t) * p1 +
                           (t * t * t - t * t) * m1;
                camber = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * c0 +
                         (t * t * t - 2.0f * t * t + t) * cm0 +
                         (-2.0f * t * t * t + 3.0f * t * t) * c1 +
                         (t * t * t - t * t) * cm1;
                AddFinalPoint(position, camber, t <= 0 ? idxStart : (int?)null);

                // We check if t reached 1.0 to see if the final point has been reached, so we don't want to increase it the last time
                if (i < numSteps - 1)
                    t += tStep;
            }

            // If interpolation didn't reach the last point, add the last point at the end
            if (t < 1 && isLastSegment)
                AddFinalPoint(ControlPoints[idxEnd].Position, ControlPoints[idxEnd].Camber, idxEnd);

        }
        /// <summary>
        /// Hermite curve formula:
        /// (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
        /// </summary>
        private void AddPointsHermite()
        {
            for (int j = 0; j < ControlPoints.Count - 1; j++)
            {
                // check control points
                if (ControlPoints[j] == null || ControlPoints[j + 1] == null || (j > 0 && ControlPoints[j - 1] == null) || (j < ControlPoints.Count - 2 && ControlPoints[j + 2] == null))
                    return;

                bool isLastSegment = false;
                if (!IsClosed)
                    isLastSegment = (j == ControlPoints.Count - 2);

                AddSegmentHermite(j, j + 1, isLastSegment);
            }
            if (IsClosed)
                AddSegmentHermite(ControlPoints.Count - 1, 0, true);
        }
        /// <summary>
        /// 
        /// </summary>
        private void AddSegmentBezier(int idxStart, int idxEnd, bool isLastSegment)
        {
            int numSteps;
            float tStep;
            GetStepParameters(idxStart, idxEnd, out numSteps, out tStep);

            float t = 0;
            for (int i = 0; i < numSteps; i++)
            {
                Vector3 position = CalculateBezierPoint(t, ControlPoints[idxStart].Position, ControlPoints[idxStart].Position + ControlPoints[idxStart].ControlHandleOut, ControlPoints[idxEnd].Position + ControlPoints[idxEnd].ControlHandleIn, ControlPoints[idxEnd].Position);
                float camber = Mathf.Lerp(ControlPoints[idxStart].Camber, ControlPoints[idxEnd].Camber, t);
                AddFinalPoint(position, camber, (t <= 0 ? idxStart : (int?)null));

                // We check if t reached 1.0 to see if the final point has been reached, so we don't want to increase it the last time
                if (i < numSteps - 1)
                    t += tStep;
            }

            // If interpolation didn't reach the last point, add the last point at the end
            if (t < 1 && isLastSegment)
                AddFinalPoint(ControlPoints[idxEnd].Position, ControlPoints[idxEnd].Camber, idxEnd);

        }
        /// <summary>
        /// 
        /// </summary>
        private void AddPointsBezier()
        {
            for (int j = 0; j < ControlPoints.Count - 1; j++)
            {
                bool isLastSegment = false;
                if (!IsClosed)
                    isLastSegment = (j == ControlPoints.Count - 2);

                AddSegmentBezier(j, j + 1, isLastSegment);
            }

            if (IsClosed)
                AddSegmentBezier(ControlPoints.Count - 1, 0, true);
        }
        /// <summary>
        /// Clamp the list positions to allow looping
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        int ClampListPosCatmullRom(int pos)
        {
            if (pos < 0)
                pos = ControlPoints.Count - 1;

            if (pos > ControlPoints.Count)
                pos = 1;
            else if (pos > ControlPoints.Count - 1)
                pos = 0;

            return pos;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idxStart"></param>
        /// <param name="idxEnd"></param>
        /// <param name="isLastSegment"></param>
        private void AddSegmentCatmullRom(int idxStart, int idxEnd, bool isLastSegment)
        {
            int numSteps;
            float tStep;
            GetStepParameters(idxStart, idxEnd, out numSteps, out tStep);

            //The 4 points we need to form a spline between p1 and p2
            Vector3 p0 = ControlPoints[ClampListPosCatmullRom(idxStart - 1)].Position;
            Vector3 p1 = ControlPoints[idxStart].Position;
            Vector3 p2 = ControlPoints[ClampListPosCatmullRom(idxEnd)].Position;
            Vector3 p3 = ControlPoints[ClampListPosCatmullRom(idxEnd + 1)].Position;

            float t = 0;
            for (int i = 0; i < numSteps; i++)
            {
                Vector3 position = CalculateCatmullRomPoint(t, p0, p1, p2, p3);
                float camber = Mathf.Lerp(ControlPoints[idxStart].Camber, ControlPoints[idxEnd].Camber, t);
                AddFinalPoint(position, camber, (t <= 0 ? idxStart : (int?)null));

                // We check if t reached 1.0 to see if the final point has been reached, so we don't want to increase it the last time
                if (i < numSteps - 1)
                    t += tStep;
            }

            // If interpolation didn't reach the last point, add the last point at the end
            if (t < 1 && isLastSegment)
                AddFinalPoint(ControlPoints[idxEnd].Position, ControlPoints[idxEnd].Camber, idxEnd);

        }
        /// <summary>
        /// 
        /// </summary>
        private void AddPointsCatmullRom()
        {
            for (int j = 0; j < ControlPoints.Count - 1; j++)
            {
                bool isLastSegment = false;
                if (!IsClosed)
                    isLastSegment = (j == ControlPoints.Count - 2);

                AddSegmentCatmullRom(j, j + 1, isLastSegment);
            }

            if (IsClosed)
                AddSegmentCatmullRom(ControlPoints.Count - 1, 0, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tSegment"></param>
        /// <param name="pStart"></param>
        /// <param name="pStartHandle"></param>
        /// <param name="pEndHandle"></param>
        /// <param name="pEnd"></param>
        /// <returns></returns>
        Vector3 CalculateBezierPoint(float tSegment, Vector3 pStart, Vector3 pStartHandle, Vector3 pEndHandle, Vector3 pEnd)
        {
            float u = 1 - tSegment;
            float tt = tSegment * tSegment;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * tSegment;
            Vector3 p = uuu * pStart;
            p += 3 * uu * tSegment * pStartHandle;
            p += 3 * u * tt * pEndHandle;
            p += ttt * pEnd;
            return p;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tSegment"></param>
        /// <param name="pStart"></param>
        /// <param name="pStartHandle"></param>
        /// <param name="pEndHandle"></param>
        /// <param name="pEnd"></param>
        /// <returns></returns>
        float CalculateBezierCamber(float tSegment, float pStart, Vector3 pStartHandle, Vector3 pEndHandle, float pEnd)
        {
            float u = 1 - tSegment;
            float tt = tSegment * tSegment;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * tSegment;
            float p = uuu * pStart;
            p += 3 * uu * tSegment;// * pStartHandle;
            p += 3 * u * tt;// * pEndHandle;
            p += ttt * pEnd;
            return p;
        }
        /// <summary>
        /// Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        Vector3 CalculateCatmullRomPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

            return pos;
        }
        #endregion

        #region Curve Creation    
        /// <summary>
        /// Builds a straight line
        /// </summary>
        /// <param name="pStart"></param>
        /// <param name="pLength"></param>
        /// <param name="pInterpolationSteps"></param>
        public void BuildStraight(Vector3 pDir, float pLength, int pInterpolationSteps, bool pAppend)
        {
            Vector3 startPoint = Vector3.zero;
            Vector3 dir = Vector3.Normalize(pDir);
            if (!pAppend)
            {
                this.ControlPoints.Clear();
                // Add Start Point
                AddControlPoint(startPoint, 0f);
            }
            else
            {
                startPoint = ControlPoints[ControlPoints.Count - 1].Position;

                // Tratar de seguir la dirección dejada por el último tramo
                if (ControlPoints.Count > 2)
                    dir = Vector3.Normalize(startPoint - ControlPoints[ControlPoints.Count - 2].Position);
            }

            float step = pLength / (float)pInterpolationSteps;
            Vector3 pt = startPoint;
            for (int i = 0; i < pInterpolationSteps; i++)
            {
                pt += dir * step;
                AddControlPoint(pt, 0f);
            }

            //mTotalLength = (ControlPoints[ControlPoints.Count - 1].Position - ControlPoints[0].Position).magnitude;

            this.Refresh();
        }
        
        /// <summary>
        /// Builds a constant radius curve
        /// </summary>
        /// <param name="pStart"></param>
        /// <param name="pLength"></param>
        /// <param name="pTotalCurveAngleRads"></param>
        /// <param name="pInterpolationSteps"></param>
        /// <returns>The resulting length (perimeter) of the curve</returns>
        public float BuildCurve(float pCurveRadius, float pCurveCircle_TotalDegs, float pCurveCamberDegs0_180, int pInterpolationSteps, bool pAppend)
        {
            Vector3 startPoint = Vector3.zero;
            Vector3 initialDir = Vector3.forward;
            Vector3 sideVector = pCurveCircle_TotalDegs >= 0 ? Vector3.right : -Vector3.right;

            float actualCamberDegs = pCurveCamberDegs0_180;
            if (pCurveCircle_TotalDegs > 0)
                actualCamberDegs *= -1;

            if (!pAppend)
            {
                this.ControlPoints.Clear();
                AddControlPoint(startPoint, actualCamberDegs);
            }
            else
            {
                startPoint = ControlPoints[ControlPoints.Count - 1].Position;

                // Try to follow last segment direction
                if (ControlPoints.Count > 2)
                {
                    initialDir = Vector3.Normalize(ControlPoints[ControlPoints.Count - 1].Position - ControlPoints[ControlPoints.Count - 2].Position);

                    sideVector = Vector3.Cross(initialDir, Vector3.up);
                    if (pCurveCircle_TotalDegs >= 0)
                        sideVector *= -1;
                }
            }
            sideVector.Normalize();

            //  Calculate radius
            //  Perimeter = 2 * pi * radius            
            //  Radius = P / (2 * pi) -Perimeter divided by 2 pi -
            float angularStepDegs = pCurveCircle_TotalDegs / (float)(pInterpolationSteps);
            Vector3 stPoint = startPoint + (sideVector * pCurveRadius);
            float curAngleDegs = 0;
            Vector3 firstPos = Vector3.zero;
            Vector3 finalPos = Vector3.zero;
            for (int i = 0; i <= pInterpolationSteps; i++)
            {
                Quaternion rot = Quaternion.AngleAxis(curAngleDegs, Vector3.up);                
                Vector3 dir =  rot * -sideVector;
                finalPos = stPoint + (dir * pCurveRadius);
                if (i == 0)
                    firstPos = finalPos;

                AddControlPoint(finalPos, actualCamberDegs);

                curAngleDegs += angularStepDegs;
            }

            return 0f;
        }
        /// <summary>
        /// Builds a clothoid curve
        /// </summary>
        /// <param name="pLength"></param>
        /// <param name="pLeftCurve"></param>
        /// <param name="pFinalCurveRadius"></param>
        /// <param name="pFinalCurveCamberDegs0_180"></param>
        /// <param name="pInterpolationSteps"></param>
        /// <param name="pAppend"></param>
        public void BuildClothoid(float pLength, bool pLeftCurve, bool pIsCurveEntry, float pFinalCurveRadius, float pFinalCurveCamberDegs0_180, int pInterpolationSteps, bool pAppend)
        {
            Vector3 startPoint = Vector3.zero;
            if (!pAppend)
            {
                this.ControlPoints.Clear();
            }
            else
            {
                startPoint = ControlPoints[ControlPoints.Count - 1].Position;
            }

            // Calcular el parámetro A (constante de la espiral o de la clotoide)
            // A^2 = R * L
            float A2 = (pFinalCurveRadius * pLength);
            float A = (float)Math.Sqrt(A2);

            float actualCamberDegs = pFinalCurveCamberDegs0_180;
            if (!pLeftCurve)
                actualCamberDegs *= -1;

            // Build new points, normalized and aligned with world coordinates
            List<ControlPoint> newPoints = new List<ControlPoint>();
            float step = pLength / (float)(pInterpolationSteps);
            float realStep = step;
            step /= A;
            float t = 0f;
            float totalLength = 0f;
            for (int i = 0; i < pInterpolationSteps; i++)
            {
                float x = A * (float)IntegrateClothoidPoint(0, t, 20, true);
                float y = A * (float)IntegrateClothoidPoint(0, t, 20, false);

                if (pLeftCurve)
                    x *= -1;
                if (!pIsCurveEntry)
                    y *= -1;

                // Calc Camber
                float length01 = Math.Min(1f, (float)i / ((float)pInterpolationSteps - 2));
                float curCamber = actualCamberDegs * length01;

                Vector3 point = new Vector3(x, 0, y);
                newPoints.Add(new ControlPoint(point, curCamber));

                if (i > 0)
                    totalLength += (newPoints[i].Position - newPoints[i - 1].Position).magnitude;

                t += step;
            }

            // Siempre asegurarnos de que el primer segmento (o el último, si es de salida) está alineado con el eje Z
            Vector3 offset;
            if (pIsCurveEntry)
                offset = new Vector3(0, 0, realStep);
            else offset = new Vector3(0, 0, -realStep);

            for (int i = 0; i < newPoints.Count; i++)
                newPoints[i].Position += offset;
            ControlPoint newpt = new ControlPoint(Vector3.zero, 0f);
            newPoints.Insert(0, newpt);

            // Calcular el Rotation Angle necesario para alinearlo con el previous segment, o con el eje Z (si es una clotoide de salida)
            Vector3 prevSegmentDir = Vector3.forward;
            Vector3 thisSegmentDir = Vector3.forward;
            if (pAppend && ControlPoints.Count > 2)
                prevSegmentDir = Vector3.Normalize(ControlPoints[ControlPoints.Count - 1].Position - ControlPoints[ControlPoints.Count - 2].Position);

            // Si no es una curva de entrada, hay que añadir al Rotation Angle la rotación que llevan implícita los primeros puntos (en una curva de entrada empiezan alineados con el eje Z, pero en una de salida no)
            // Todavía no está hecho el Reverse del path, por eso resto el penultimo-ultimo, en lugar de al reves
            if (!pIsCurveEntry && newPoints.Count > 1)
                thisSegmentDir = Vector3.Normalize(newPoints[newPoints.Count - 2].Position - newPoints[newPoints.Count - 1].Position);

            float rotationAngleDegs = GetAngleBetween(prevSegmentDir, thisSegmentDir, Vector3.right) * Mathf.Rad2Deg;
            if (rotationAngleDegs != 0f)
            {
                Quaternion rot  = Quaternion.AngleAxis(rotationAngleDegs, Vector3.up);

                foreach (ControlPoint pt in newPoints)
                    pt.Position = rot * pt.Position;
            }

            // Si no es una curva de entrada, hay que añadir al StartPoint la posiciónd el último punto, para que éste quede en el 0,0
            if (!pIsCurveEntry)
                startPoint -= newPoints[newPoints.Count - 1].Position;

            // Translate points to start at StartPoint
            for (int i = 0; i < newPoints.Count; i++)
                newPoints[i].Position += startPoint;

            // Si estoy añadiendo, y es una curva de salida, el primer punto de la que vamos a añadir se va a poner en la misma posición que el último de la anterior, creando dos puntos solapados. Por eso, 
            // elimino el último de la anterior, para que no se solapen
            if (pAppend)
                ControlPoints.RemoveAt(ControlPoints.Count - 1);

            // Add points to current collection
            if (pIsCurveEntry)
                ControlPoints.AddRange(newPoints);
            else
            {
                // Add in reverse order
                for (int i = newPoints.Count - 1; i >= 0; i--)
                    ControlPoints.Add(newPoints[i]);
            }

            this.Refresh();
        }
        /// <summary>
        /// Builds a complete curve, with two clothoids (for the entry and exit), plus a constant radius curve in the middle
        /// </summary>
        public void BuildFullCurve(float pClothoidSegmentsLength, float pTotalTurningDegrees, bool pLeftCurve, float pCurveRadius, float pCurveCamberDegs0_180, int pInterpolationStepsClothoidSegments, int pInterpolationStepsConstantRadiusSegment, bool pAppend)
        {
            Vector3 lastSegmentForward = Vector3.forward;
            if(pAppend && ControlPoints.Count > 2)
                lastSegmentForward = (ControlPoints[ControlPoints.Count - 1].Position - ControlPoints[ControlPoints.Count - 2].Position).normalized;

            // STEP 1 - Clothoid entry
            this.BuildClothoid(pClothoidSegmentsLength, pLeftCurve, true, pCurveRadius, pCurveCamberDegs0_180, pInterpolationStepsClothoidSegments, pAppend);

            // STEP 2 - Calculate number of degrees to turn in constant radius to complete curve
            Vector3 lastSegmentClothoid = (ControlPoints[ControlPoints.Count - 1].Position - ControlPoints[ControlPoints.Count - 2].Position).normalized;
            float clothoid_Degs = Mathf.Abs(GetAngleBetween(lastSegmentClothoid, lastSegmentForward, Vector3.right)) * Mathf.Rad2Deg;

            float degsToTurnConstatRadius = Math.Abs(pTotalTurningDegrees) - (Math.Abs(clothoid_Degs) * 2f);        // The total amount of degrees to complete with the constant radius curve, would be the total amount indicated in the parameter minus the amount of turn produced by the clothoid parts
            if (pLeftCurve)
                degsToTurnConstatRadius *= -1;

            // STEP 3 - Build constant rad part
            this.BuildCurve(pCurveRadius, degsToTurnConstatRadius, pCurveCamberDegs0_180, pInterpolationStepsConstantRadiusSegment, true);

            // STEP 4 - Clothoid exit
            this.BuildClothoid(pClothoidSegmentsLength, pLeftCurve, false, pCurveRadius, pCurveCamberDegs0_180, pInterpolationStepsClothoidSegments, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public double IntegrateClothoidPoint(double start, double end, long intervals, bool pUseSin)
        {
            // Loop through values of t to call our function  
            double value = 0;
            // Note: it would be more optimal to store the *increment* value here, and reuse it at every step of the loop  
            double increment = (end - start) / (double)intervals;

            for (long n = 0; n < intervals; n++)
            {
                // Calculate t, interpolating linearly between start and end values  
                // Note: it would be more computationally optimal to reuse the increment value we have already stored,   
                // however this could be less *accurate* mathematically than recalculating it every time, because on very  
                // large values of intervals this method could improve precision ... at least, I think!  
                // Also notice the use of (casting) needed to allow us to combine double precision values with integer ones  
                double t = start + ((end - start) * (double)n) / (double)intervals;

                // Now we call the function and fetch the result                  
                double result;

                if (pUseSin)
                    result = (float)Math.Sin(t * t * 0.5f);
                else result = (float)Math.Cos(t * t * 0.5f);

                // Multiply the result by the increment and add it to our value   
                value += result * increment;
            }
            return value;
        }
        #endregion
    }
}