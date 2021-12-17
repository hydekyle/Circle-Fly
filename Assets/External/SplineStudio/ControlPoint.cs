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

namespace GraphicDNA.SplineStudio
{

    [Serializable]
    public class ControlPoint
    {
        public enum eControlPointMode
        {
            Free,
            Aligned,
            Mirrored,
        }

        /// <summary>
        /// Position in 3D (local coords)
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Camber angle (in Degrees) or tilt in the point, where 0 is horizontal
        /// </summary>
        public float Camber;
        /// <summary>
        /// Position of the Handle In, expressed as an offset from Position
        /// </summary>
        public Vector3 ControlHandleIn;
        /// <summary>
        /// Position of the Handle Out, expressed as an offset from Position
        /// </summary>
        public Vector3 ControlHandleOut;
        public eControlPointMode ControlPointMode = eControlPointMode.Mirrored;


        /// <summary>
        /// 
        /// </summary>
        public ControlPoint()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPosition"></param>
        /// <param name="pCamberDegs"></param>
        public ControlPoint(Vector3 pPosition, float pCamberDegs)
        {
            this.Position = pPosition;
            this.Camber = pCamberDegs;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ControlPoint Clone()
        {
            ControlPoint cp = new ControlPoint();
            cp.Position = this.Position;
            cp.ControlHandleIn = this.ControlHandleIn;
            cp.ControlHandleOut = this.ControlHandleOut;
            return cp;
        }

        /// <summary>
        /// 
        /// </summary>
        public void HandleOutChanged()
        {
            if (this.ControlPointMode == eControlPointMode.Mirrored)
                this.ControlHandleIn = this.ControlHandleOut * -1;
            else if (this.ControlPointMode == eControlPointMode.Aligned)
            {
                float dist = this.ControlHandleIn.magnitude;
                this.ControlHandleIn = (this.ControlHandleOut * -1).normalized * dist;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        public void HandleInChanged()
        {
            if (this.ControlPointMode == eControlPointMode.Mirrored)
                this.ControlHandleOut = this.ControlHandleIn * -1;
            else if (this.ControlPointMode == eControlPointMode.Aligned)
            {
                float dist = this.ControlHandleOut.magnitude;
                this.ControlHandleOut = (this.ControlHandleIn * -1).normalized * dist;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        public void SwapHandles()
        {
            Vector3 aux = this.ControlHandleIn;
            this.ControlHandleIn = this.ControlHandleOut;
            this.ControlHandleOut = aux;
        }
    }

}