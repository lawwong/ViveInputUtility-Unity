//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace HTC.UnityPlugin.Vive
{
    [AddComponentMenu("HTC/VIU/Object Grabber/Attachable", 0)]
    public class Attachable : MonoBehaviour
    {
        public const float MIN_FOLLOWING_DURATION = 0.02f;
        public const float DEFAULT_FOLLOWING_DURATION = 0.04f;
        public const float MAX_FOLLOWING_DURATION = 0.5f;

        public enum AttachMode
        {
            NonPhysicsPose,
            NonphysicsPoint,
            PhysicsPose,
            PhysicsPoint,
        }

        public interface IPoseGetter
        {
            RigidPose GetPose();
        }

        private class TransformPoseProvider : IPoseGetter
        {
            public bool useLocal;
            public Transform target;
            public RigidPose GetPose() { return new RigidPose(target, useLocal); }
        }

        public Transform m_origin;
        public bool m_trackPosition = true;
        public bool m_trackRotation = true;
        public Vector3 m_offsetPos;
        public Vector3 m_offsetEular;
        public Vector3 m_pivot; // local pos

        // Rigidbody only
        [Tooltip("Need Rigidbody & non-trigger Collider component to be blockable")]
        public bool m_unblockable = true;
        public bool m_keepVelocityAfterDetached = true;
        // for object with rigidbody component only
        [Range(MIN_FOLLOWING_DURATION, MAX_FOLLOWING_DURATION)]
        [Tooltip("For object with Rigidbody component only")]
        public float m_followingDuration = DEFAULT_FOLLOWING_DURATION;
        [Tooltip("For object with Rigidbody component only")]
        public bool m_overrideMaxAngularVelocity = true;

        private TransformPoseProvider m_transProvider;
        private IPoseGetter m_target;
        private RigidPose m_prevPose;

        public bool isAttached { get { return m_target != null; } }
        public Rigidbody rigid { get; private set; }

        private bool moveByPhysics { get { return !m_unblockable && rigid != null && !rigid.isKinematic; } }
        private bool isAttachedLastFrame { get; set; }
        private RigidPose offsetPose { get { return new RigidPose(m_offsetPos, Quaternion.Euler(m_offsetEular)); } }
        private RigidPose pivotInversePose { get { return new RigidPose(m_pivot, Quaternion.identity).GetInverse(); } }

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
            m_transProvider = new TransformPoseProvider();
            isAttachedLastFrame = false;
        }

        private void Update()
        {
            if (!moveByPhysics)
            {
                if (isAttached)
                {
                    m_prevPose = new RigidPose(transform);

                    var targetPose = m_target.GetPose() * offsetPose * pivotInversePose;

                    if (rigid != null)
                    {
                        rigid.velocity = Vector3.zero;
                        rigid.angularVelocity = Vector3.zero;
                    }

                    if (m_trackPosition) { transform.position = targetPose.pos; }
                    if (m_trackRotation) { transform.rotation = targetPose.rot; }

                    isAttachedLastFrame = true;
                }
                else
                {
                    if (isAttachedLastFrame && m_keepVelocityAfterDetached && rigid != null && !rigid.isKinematic && m_prevPose != RigidPose.identity)
                    {
                        if (m_trackPosition)
                        {
                            rigid.velocity = Vector3.zero;
                            AddForce(m_prevPose.pos, transform.position, Time.deltaTime);
                        }

                        if (m_trackRotation)
                        {
                            rigid.angularVelocity = Vector3.zero;
                            AddTorce(m_prevPose.rot, transform.rotation, Time.deltaTime);
                        }
                    }

                    isAttachedLastFrame = false;
                }

            }
        }

        private void FixedUpdate()
        {
            if (moveByPhysics)
            {
                if (isAttached)
                {
                    var targetPose = m_target.GetPose() * offsetPose * pivotInversePose;

                    rigid.velocity = Vector3.zero;
                    rigid.angularVelocity = Vector3.zero;

                    if (m_trackPosition) { AddForce(transform.position, targetPose.pos, m_followingDuration); }
                    if (m_trackRotation) { AddTorce(transform.rotation, targetPose.rot, m_followingDuration); }

                    isAttachedLastFrame = true;
                }
                else
                {
                    if (isAttachedLastFrame && !m_keepVelocityAfterDetached)
                    {
                        rigid.velocity = Vector3.zero;
                        rigid.angularVelocity = Vector3.zero;
                    }

                    isAttachedLastFrame = false;
                }
            }
        }

        public bool IsAttachingTo(Transform target)
        {
            return isAttached && m_transProvider.target == target;
        }

        public bool IsAttachingTo(IPoseGetter target)
        {
            return m_target == target;
        }

        public void AttachToTransformPoint(Transform target)
        {
            //if (isAttached) { Detach(); }
            //m_transProvider.target = target;
            //m_transProvider.useLocal = false;
            //AttachToPoint(m_transProvider);
        }

        public void AttachToTransformPose(Transform target)
        {
            //if (isAttached) { Detach(); }
            //m_transProvider.target = target;
            //m_transProvider.useLocal = false;
            //AttachToPose(m_transProvider);
        }

        public void AttachToTransformPoint(Transform target, bool useLocal)
        {
            //if (isAttached) { Detach(); }
            //m_transProvider.target = target;
            //m_transProvider.useLocal = useLocal;
            //AttachToPoint(m_transProvider);
        }

        public void AttachToTransformPose(Transform target, bool useLocal)
        {
            //if (isAttached) { Detach(); }
            //m_transProvider.target = target;
            //m_transProvider.useLocal = useLocal;
            //AttachToPose(m_transProvider);
        }

        public void AttachToPoint(IPoseGetter target)
        {
            m_target = target;
        }

        public void AttachToPose(IPoseGetter target)
        {
            m_target = target;
        }

        public void Detach(Transform target)
        {
            m_transProvider.target = null;
            m_target = null;
        }

        public void Detach(IPoseGetter target)
        {

        }

        public void DetachAll()
        {
            //m_transProvider.target = null;
            //m_target = null;
        }

        private void AddForce(Vector3 from, Vector3 to, float duration)
        {
            var diffPos = to - from;
            var velocity = Mathf.Approximately(diffPos.sqrMagnitude, 0f) ? Vector3.zero : diffPos / duration;

            rigid.AddForceAtPosition(velocity, transform.TransformPoint(m_pivot), ForceMode.VelocityChange);
        }

        private void AddTorce(Quaternion from, Quaternion to, float duration)
        {
            float angle;
            Vector3 axis;
            (to * Quaternion.Inverse(from)).ToAngleAxis(out angle, out axis);
            while (angle > 180f) { angle -= 360f; }

            if (Mathf.Approximately(angle, 0f) || float.IsNaN(axis.x) || float.IsNaN(axis.y) || float.IsNaN(axis.z))
            {
                rigid.angularVelocity = Vector3.zero;
            }
            else
            {
                angle *= Mathf.Deg2Rad / duration; // convert to radius speed
                if (m_overrideMaxAngularVelocity && rigid.maxAngularVelocity < angle) { rigid.maxAngularVelocity = angle; }
                rigid.angularVelocity = axis * angle;
            }
        }
    }
}