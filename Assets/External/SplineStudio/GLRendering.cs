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
    public class GLRendering
    {
        // Cube Verts
        //
        //                 BTL -------- BTR
        //                /    |      |
        //             /       |      |
        //          /      BBL -------- BBR
        //       /                  /
        //  FTL -------- FTR    /
        //      |      |     /
        //      |      |  /
        //  FBL -------- FBR
        //
        private static Vector3 CubeFBL = new Vector3(-0.5f, -0.5f, -0.5f); // Front: BL
        private static Vector3 CubeFTL = new Vector3(-0.5f, 0.5f, -0.5f);  // Front: TL
        private static Vector3 CubeFTR = new Vector3(0.5f, 0.5f, -0.5f);   // Front: TR
        private static Vector3 CubeFBR = new Vector3(0.5f, -0.5f, -0.5f);  // Front: BR
        private static Vector3 CubeBBL = new Vector3(-0.5f, -0.5f, 0.5f);  // Back: BL
        private static Vector3 CubeBTL = new Vector3(-0.5f, 0.5f, 0.5f);   // Back: TL
        private static Vector3 CubeBTR = new Vector3(0.5f, 0.5f, 0.5f);    // Back: TR
        private static Vector3 CubeBBR = new Vector3(0.5f, -0.5f, 0.5f);   // Back: BR
        private static Vector3 CubeTopFaceCenter = new Vector3(0f, 0.5f, 0f);
        private static Vector3 CubeBottomFaceCenter = new Vector3(0f, -0.5f, 0f);
        /// <summary>
        /// Vertices for a cube 
        /// </summary>
        public static Vector3[] CubeVerts = new Vector3[]
        {
            // Front Face
            CubeFBL,
            CubeFTL,
            CubeFTR,
            CubeFBR,      

            // Back Face
            CubeBTR,
            CubeBBR,
            CubeBBL,
            CubeBTL,      

            // Left Face
            CubeBBL,
            CubeBTL,
            CubeFTL,
            CubeFBL,

            // Right Face
            CubeFBR,
            CubeFTR,
            CubeBTR,
            CubeBBR,

            // Top Face
            CubeFTL,
            CubeBTL,
            CubeBTR,
            CubeFTR,

            // Bottom Face
            CubeBBL,
            CubeFBL,
            CubeFBR,
            CubeBBR,

        };
        /// <summary>
        /// Vertices for a cube 
        /// </summary>
        private static Vector3[] PyramidLinesPointPairs = new Vector3[]
        {
            // Bottom Face
            CubeFBL,
            CubeFBR,
            CubeFBR,
            CubeBBR,
            CubeBBR,
            CubeBBL,
            CubeBBL,
            CubeFBL,

            // Lines to tip
            CubeFBL,
            CubeTopFaceCenter,
            CubeFBR,
            CubeTopFaceCenter,
            CubeBBR,
            CubeTopFaceCenter,
            CubeBBL,
            CubeTopFaceCenter,
        };
        private static Vector3[] CrossLinesPointPairs = new Vector3[]
        {
            CubeBottomFaceCenter,
            CubeTopFaceCenter,
        };
        private static Material mGLMaterialColorOnly = null;
        /// <summary>
        /// Returns the default material to fill shapes
        /// </summary>
        public static Material MaterialColorOnly
        {
            get
            {
                if (mGLMaterialColorOnly == null || mGLMaterialColorOnly.Equals(null))
                {
                    Shader shader = Shader.Find("Hidden/Internal-Colored");       // This shader takes only into account the color, not the texture
                    mGLMaterialColorOnly = new Material(shader);
                    mGLMaterialColorOnly.SetInt("_ZTest", 0);
                }

                return mGLMaterialColorOnly;
            }
        }
        private static Material mGLMaterialColorAndTexture = null;
        /// <summary>
        /// Returns the default material to fill shapes
        /// </summary>
        public static Material MaterialColorAndTexture
        {
            get
            {
                if (mGLMaterialColorAndTexture == null || mGLMaterialColorAndTexture.Equals(null))
                {
                    Shader shader = Shader.Find("UI/Default");                      // This shader takes combines the color with the texture
                    mGLMaterialColorAndTexture = new Material(shader);
                }

                return mGLMaterialColorAndTexture;
            }
        }
        /// <summary>
        /// Fills a Cube that can have any orientation. 
        /// </summary>
        /// <param name="pPosition">Center of the cube, in 3D</param>
        /// <param name="pSize">Full size of the cube, in 3D</param>
        /// <param name="pOrientation">Orientation of the cube</param>
        /// <param name="color">Color of the quad</param>
        public static void FillCube(Vector3 pPosition, Vector3 pSize, Quaternion? pOrientation = null, Color? color = null)
        {
            FillQuads(CubeVerts, color.HasValue ? color.Value : Color.white, pPosition, pSize, pOrientation);
            //FillQuadsArray
            //DrawLines(CrossLinesPointPairs, color.HasValue ? color.Value : Color.white, pPosition, pSize, pOrientation);
        }

        /// <summary>
        /// Fills a Pyramid that can have any orientation. 
        /// </summary>
        /// <param name="pPosition">Center of the cube, in 3D</param>
        /// <param name="pSize">Full size of the cube, in 3D</param>
        /// <param name="pOrientation">Orientation of the cube</param>
        /// <param name="color">Color of the quad</param>
        public static void DrawPyramid(Vector3 pPosition, Vector3 pScale, Quaternion? pOrientation = null, Color? color = null)
        {
            DrawLines(PyramidLinesPointPairs, color.HasValue ? color.Value : Color.white, pPosition, pScale, pOrientation);
        }
        /// <summary>
        /// Draws lines with thickness == 1 pixel
        /// </summary>
        /// <param name="pointPairs">Pairs of points in 3D</param>
        /// <param name="pColor">Color of all lines</param>
        public static void DrawLines(Vector3[] pointPairs, Color pColor, Vector3? pPositionOffset = null, Vector3? pScale = null, Quaternion? pOrientation = null)
        {
            // Clear the current render buffer, setting a new background colour, and set our
            // material for rendering.
            MaterialColorOnly.SetPass(0);

            // Backup matrices
            GL.PushMatrix();
            Matrix4x4 prevModelView = GL.modelview;

            Vector3 pos = pPositionOffset.HasValue ? pPositionOffset.Value : Vector3.zero;
            Vector3 scale = pScale.HasValue ? pScale.Value : Vector3.one;
            Quaternion orientation = pOrientation.HasValue ? pOrientation.Value : Quaternion.identity;
            GL.modelview = prevModelView * Matrix4x4.TRS(pos, orientation, scale);

            // Start drawing in OpenGL Lines, to draw the lines of the grid.
            GL.Begin(GL.LINES);
            GL.Color(pColor);
            for (int i = 0; i < pointPairs.Length - 1; i += 2)
            {
                GL.Vertex(pointPairs[i]);
                GL.Vertex(pointPairs[i + 1]);
            }

            // End lines drawing.
            GL.End();
            GL.modelview = prevModelView;
            GL.PopMatrix();
        }

        /// <summary>
        /// Fills quads that can have any orientation. Each quad is defined by its 4 vertices in the following order: BL, TL, TR, BR
        /// </summary>
        /// <param name="pVertices">Vertices of the Quad in the following order: BL, TL, TR, BR (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the quad</param>
        /// <param name="pOverrideTexture">Texture to overlay, if any</param>
        /// <param name="pTilingMultiplier">Tiling multiplier or null for default tiling (1, 1)</param>
        public static void FillQuads(Vector3[] pVertices, Color color, Vector3? pPositionOffset = null, Vector3? pScale = null, Quaternion? pOrientation = null)
        {
            MaterialColorOnly.SetPass(0);

            // Backup matrices
            GL.PushMatrix();
            Matrix4x4 prevModelView = GL.modelview;

            Vector3 pos = pPositionOffset.HasValue ? pPositionOffset.Value : Vector3.zero;
            Vector3 scale = pScale.HasValue ? pScale.Value : Vector3.one;
            Quaternion orientation = pOrientation.HasValue ? pOrientation.Value : Quaternion.identity;
            GL.modelview = prevModelView * Matrix4x4.TRS(pos, orientation, scale);

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.   
            GL.Begin(GL.QUADS);
            GL.Color(color);

            for (int i = 0; i <= pVertices.Length - 4; i += 4)
            {
                GL.Vertex(pVertices[i]);
                GL.Vertex(pVertices[i + 1]);
                GL.Vertex(pVertices[i + 2]);
                GL.Vertex(pVertices[i + 3]);
            }

            // End lines drawing.
            GL.End();
            GL.modelview = prevModelView;
            GL.PopMatrix();

        }
        /// <summary>
        /// Fills quads that can have any orientation. Each quad is defined by its 4 vertices in the following order: BL, TL, TR, BR
        /// </summary>
        /// <param name="pVertices">Vertices of the Quad in the following order: BL, TL, TR, BR (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the quad</param>
        /// <param name="pOverrideTexture">Texture to overlay, if any</param>
        /// <param name="pTilingMultiplier">Tiling multiplier or null for default tiling (1, 1)</param>
        public static void FillQuads(Vector3[] pVertices, Vector2[] pTexCoords, Color color, Vector3? pPositionOffset = null, Vector3? pScale = null, Quaternion? pOrientation = null, Texture2D pOverrideTexture = null)
        {
            MaterialColorAndTexture.mainTexture = pOverrideTexture;
            MaterialColorAndTexture.mainTextureOffset = Vector2.zero;
            MaterialColorAndTexture.mainTextureScale = Vector2.one;
            MaterialColorAndTexture.SetPass(0);

            // Backup matrices
            GL.PushMatrix();
            Matrix4x4 prevModelView = GL.modelview;

            Vector3 pos = pPositionOffset.HasValue ? pPositionOffset.Value : Vector3.zero;
            Vector3 scale = pScale.HasValue ? pScale.Value : Vector3.one;
            Quaternion orientation = pOrientation.HasValue ? pOrientation.Value : Quaternion.identity;
            GL.modelview = prevModelView * Matrix4x4.TRS(pos, orientation, scale);

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.   
            GL.Begin(GL.QUADS);
            GL.Color(color);

            for (int i = 0; i <= pVertices.Length - 4; i += 4)
            {
                GL.TexCoord2(pTexCoords[i].x, pTexCoords[i].y);
                GL.Vertex(pVertices[i]);

                GL.TexCoord2(pTexCoords[i + 1].x, pTexCoords[i + 1].y);
                GL.Vertex(pVertices[i + 1]);

                GL.TexCoord2(pTexCoords[i + 2].x, pTexCoords[i + 2].y);
                GL.Vertex(pVertices[i + 2]);

                GL.TexCoord2(pTexCoords[i + 3].x, pTexCoords[i + 3].y);
                GL.Vertex(pVertices[i + 3]);
            }


            // End lines drawing.
            GL.End();
            GL.modelview = prevModelView;
            GL.PopMatrix();

        }

        /// <summary>
        /// Draws a list of crosses
        /// </summary>
        public static void DrawCrosses(List<Vector3> pPositions, Color color)
        {
            GLRendering.MaterialColorOnly.SetPass(0);

            // Backup matrices
            GL.PushMatrix();

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.   
            GL.Begin(GL.LINES);
            GL.Color(color);

            float size = 0.15f;
            foreach (Vector3 v in pPositions)
            {
                size = Mathf.Min(0.25f, 0.125f * HandleUtility.GetHandleSize(v));

                GL.Vertex(v + (Vector3.up * size));
                GL.Vertex(v - (Vector3.up * size));
                GL.Vertex(v - (Vector3.left * size));
                GL.Vertex(v + (Vector3.left * size));
                GL.Vertex(v - (Vector3.forward * size));
                GL.Vertex(v + (Vector3.forward * size));
            }

            // End lines drawing.
            GL.End();
            GL.PopMatrix();
        }

    }
}
#endif