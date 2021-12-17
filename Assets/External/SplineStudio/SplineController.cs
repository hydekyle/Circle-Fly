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
    [ExecuteInEditMode]
    public class SplineController : MonoBehaviour
    {
        public enum eSpeedUnits
        {
            /// <summary>
            /// Meters per second
            /// </summary>
            MetersPerSecond,
            /// <summary>
            /// Kilometers per hour
            /// </summary>
            KilometersPerHour,
            /// <summary>
            /// Miles per hour
            /// </summary>
            MilesPerHour,
            /// <summary>
            /// Curve Percentage (t) increment per second
            /// </summary>
            PercentagePerSecond,
        }
        public enum eLoopMode
        {
            None,
            AutoRewind,
            PingPong
        }
        public enum eMilestoneMode
        {
            ControlPoints,
            InterpolatedPoints,
        }
        public enum eOrientationMode
        {
            None,
            FollowSpline,
            FollowTarget,
        }

        public Spline Spline;
        /// <summary>
        /// Location on the position corresponding to a particular curve percent, where 0 is the origin of the spline, and 1 is the last point
        /// </summary>
        [Range(0, 1)]
        public float CurvePercent;
        [Range(0, 1)]
        public float DampingTime;

        [Space(10)]
        public bool AffectPosition;
        public Vector3 PositionOffset;

        [Space(10)]
        public eOrientationMode OrientationMode = eOrientationMode.None;
        public Transform Target;
        public Vector3 AdditionalRotation;


        [Space(10)]
        public bool AutomaticWalking;
        public bool UpdateInUnityEditor;
        public eLoopMode LoopMode = eLoopMode.None;
        public eSpeedUnits SpeedUnits = eSpeedUnits.MetersPerSecond;
        public float Speed;
        private float TDelta = 0f;
        private Vector3 mCurrentAxisY;
        private Vector3 mCurrentAxisZ;
        private Vector3 mCurrentTarget;

        [Space(10)]
        public eMilestoneMode MilestonesBasedOn = eMilestoneMode.ControlPoints;
        public int CurrentMileStone;

        public UnityEngine.Events.UnityEvent MilestoneReached;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mps"></param>
        /// <returns></returns>
        private float Mps2Tps(float mps)
        {
            if (!Spline.IsNull() && Spline.TotalLength > 0)
                return mps / Spline.TotalLength;
            else return 0;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetTPerSeconds()
        {
            switch (SpeedUnits)
            {
                case eSpeedUnits.PercentagePerSecond:
                    return Speed;
                case eSpeedUnits.MetersPerSecond:
                    return Mps2Tps(Speed);
                case eSpeedUnits.KilometersPerHour:
                    return Mps2Tps(Speed / 3.6f);
                case eSpeedUnits.MilesPerHour:
                    return Mps2Tps(Speed * 0.44704f);
                default:
                    return 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {

            TDelta = GetTPerSeconds();
        }
        //    /// <summary>
        //    /// 
        //    /// </summary>
        //    private void FixedUpdate()
        //    {
        //#if (!UNITY_EDITOR)
        //        // If we are in play mode, use the normal damping time. If we are in the editor, set it to zero to reflect the changes immediately
        //        float dampingTime = 0;
        //        if (Application.isPlaying)
        //            dampingTime = this.DampingTime;

        //        InternalUpdate(Time.fixedDeltaTime, dampingTime);
        //#endif
        //    }
        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            // If we are in play mode, use the normal damping time. If we are in the editor, set it to zero to reflect the changes immediately
            float dampingTime = 0;
            if (Application.isPlaying)
                dampingTime = this.DampingTime;

            InternalUpdate(Time.deltaTime, dampingTime);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="deltaTime"></param>
        private void InternalUpdate(float deltaTime, float dampingTime)
        {
            if (Spline.IsNull())
                return;
            if (!AffectPosition && OrientationMode == eOrientationMode.None)
                return;

            // Automatic update of t
            if (AutomaticWalking)
            {
                bool update = true;
#if (UNITY_EDITOR)
                update = Application.isPlaying || UpdateInUnityEditor;
#endif
                if (update)
                    CurvePercent += TDelta * deltaTime;
            }

            // Loop
            switch (LoopMode)
            {
                case eLoopMode.AutoRewind:
                    if (TDelta > 0 && CurvePercent >= 1)
                    {
                        CurvePercent = 0;
                        TDelta = GetTPerSeconds();
                    }
                    else if (TDelta < 0 && CurvePercent <= 0)
                    {
                        CurvePercent = 1;
                        TDelta = GetTPerSeconds();
                    }
                    break;
                case eLoopMode.PingPong:
                    if (CurvePercent >= 1)
                        TDelta = -GetTPerSeconds();
                    else if (CurvePercent <= 0)
                        TDelta = GetTPerSeconds();
                    break;
                default:
#if (UNITY_EDITOR)
                    //Spline.Refresh();
                    TDelta = GetTPerSeconds();
#endif
                    break;

            }

            // Update parameters
            Vector3 pos;
            Vector3 axisX, axisY, axisZ;
            int idx;
            Spline.GetWorldPositionFromT(CurvePercent, PositionOffset, out idx, out pos, out axisX, out axisY, out axisZ);

            // Choose milestone idx based on type of milestones
            int mileStoneIdx = idx;
            switch (MilestonesBasedOn)
            {
                case eMilestoneMode.InterpolatedPoints:
                    mileStoneIdx = idx;
                    break;
                case eMilestoneMode.ControlPoints:
                    if (Spline.FinalPoints[idx].ControlPointIdx.HasValue)
                        mileStoneIdx = Spline.FinalPoints[idx].ControlPointIdx.Value;
                    break;
            }
            if (mileStoneIdx != CurrentMileStone)
            {
                CurrentMileStone = mileStoneIdx;
                MilestoneReached.Invoke();
            }



            // Damp parameters
            Vector3 v = Vector3.zero;
            if (dampingTime <= 0)
            {
                mCurrentAxisY = axisY;
                mCurrentAxisZ = axisZ;
                if (!Target.IsNull())
                    mCurrentTarget = Target.transform.position;
            }
            else
            {
                mCurrentAxisY = Vector3.SmoothDamp(mCurrentAxisY, axisY, ref v, dampingTime);
                mCurrentAxisZ = Vector3.SmoothDamp(mCurrentAxisZ, axisZ, ref v, dampingTime);
                if (!Target.IsNull())
                    mCurrentTarget = Vector3.SmoothDamp(mCurrentTarget, Target.transform.position, ref v, dampingTime);
            }

            if (AffectPosition)
            {
                if (dampingTime <= 0)
                    this.transform.position = pos;
                else
                {
                    Vector3 vel = Vector3.zero;
                    transform.position = Vector3.SmoothDamp(transform.position, pos, ref vel, dampingTime);
                }
            }



            Quaternion desiredRotation = Quaternion.identity;
            switch (OrientationMode)
            {
                case eOrientationMode.FollowSpline:
                    desiredRotation = Quaternion.LookRotation(mCurrentAxisZ, mCurrentAxisY) * Quaternion.Euler(AdditionalRotation);
                    break;
                case eOrientationMode.FollowTarget:
                    if (Target.IsNull())
                        desiredRotation = Quaternion.identity;
                    else desiredRotation = Quaternion.LookRotation((mCurrentTarget - this.transform.position).normalized, mCurrentAxisY) * Quaternion.Euler(AdditionalRotation);
                    break;
            }
            if (OrientationMode != eOrientationMode.None)
            {
                if (dampingTime <= 0)
                    this.transform.rotation = desiredRotation;
                else
                {
                    float rotationSpeedDegS = Mathf.Clamp01(1f - dampingTime) * 99f;
                    this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, desiredRotation, deltaTime * rotationSpeedDegS);
                }
            }

        }

    }
}