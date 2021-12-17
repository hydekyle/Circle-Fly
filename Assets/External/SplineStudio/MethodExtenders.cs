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

namespace GraphicDNA.SplineStudio
{
    public static class MethodExtenders
    {
        /// <summary>
        /// Provides an optimal way to check if a Unity Object is null
        /// </summary>
        /// <param name="pSrc"></param>
        /// <returns></returns>
        public static bool IsNull(this UnityEngine.Object pSrc)
        {
            // Checking with (object)pSrc is much faster than calling the == operator, because Unity's overloads the == operators and performs
            // additional checks. Unfortunately, that king of check works only if we are outside the Unity editor.
#if (UNITY_EDITOR)
            return (pSrc == null);
#else
        return ((object)pSrc == null);
#endif
        }
    }

}