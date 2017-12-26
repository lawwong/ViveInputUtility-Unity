//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using UnityEngine;

namespace HTC.UnityPlugin.Vive.Misc
{
    [AddComponentMenu("HTC/VIU/Object Grabber/Attachable", 0)]
    public class Attachable : MonoBehaviour
    {
        public const float MIN_FOLLOWING_DURATION = 0.02f;
        public const float DEFAULT_FOLLOWING_DURATION = 0.04f;
        public const float MAX_FOLLOWING_DURATION = 0.5f;

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

        [SerializeField]
        private Vector3 m_anchorPos;
        [SerializeField]
        private Vector3 m_anchorEular;
        [Tooltip("Need Rigidbody with non-trigger Collider component to be blockable")]
        public bool m_unblockable = false;

        // for object with rigidbody component only
        public bool m_keepVelocityAfterDetached = true;
        [Range(MIN_FOLLOWING_DURATION, MAX_FOLLOWING_DURATION)]
        [Tooltip("For object with Rigidbody component only")]
        public float m_followingDuration = DEFAULT_FOLLOWING_DURATION;
        [Tooltip("For object with Rigidbody component only")]
        public bool m_overrideMaxAngularVelocity = true;

        private TransformPoseProvider m_transProvider;
        private IPoseGetter m_target;
        private RigidPose m_prevPose;

        private Rigidbody rigid { get; set; }
        public bool isAttached { get { return m_target != null; } }
        private bool isAttachedLastFrame { get; set; }
        private bool moveByPhysics { get { return !m_unblockable && rigid != null && !rigid.isKinematic; } }
        public Vector3 anchorPos { get { return m_anchorPos; } set { m_anchorPos = value; } }
        public Vector3 anchorEular { get { return m_anchorEular; } set { m_anchorEular = value; } }
        private RigidPose anchorInversePose { get { return new RigidPose(transform) * new RigidPose(transform.TransformPoint(anchorPos), transform.rotation * Quaternion.Euler(anchorEular)).GetInverse(); } }

        private void Awake()
        {
            UpdateRigidbody();
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
                    var targetPose = m_target.GetPose() * anchorInversePose;

                    if (rigid != null)
                    {
                        rigid.velocity = Vector3.zero;
                        rigid.angularVelocity = Vector3.zero;
                    }

                    transform.position = targetPose.pos;
                    transform.rotation = targetPose.rot;

                    isAttachedLastFrame = true;
                }
                else
                {
                    if (isAttachedLastFrame && m_keepVelocityAfterDetached && rigid != null && !rigid.isKinematic && m_prevPose != RigidPose.identity)
                    {
                        rigid.velocity = Vector3.zero;
                        rigid.angularVelocity = Vector3.zero;

                        AddForce(m_prevPose.pos, transform.position, Time.deltaTime);
                        AddTorce(m_prevPose.rot, transform.rotation, Time.deltaTime);

                        rigid = null;
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
                    var targetPose = m_target.GetPose() * anchorInversePose;

                    rigid.velocity = Vector3.zero;
                    rigid.angularVelocity = Vector3.zero;

                    AddForce(transform.TransformPoint(anchorPos), targetPose.pos, m_followingDuration);
                    //AddTorce(transform.rotation, targetPose.rot, m_followingDuration);

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

        /// <summary>
        /// Must called whenever rigidbody on this gameobject is added/destroyed
        /// </summary>
        public void UpdateRigidbody()
        {
            rigid = GetComponent<Rigidbody>();
        }

        public bool IsAttachingTo(Transform target)
        {
            return m_target == m_transProvider && m_transProvider.target == target;
        }

        public bool IsAttachingTo(IPoseGetter target)
        {
            return m_target == target;
        }

        public void AttachToTransform(Transform target)
        {
            AttachToTransform(target, false);
        }

        public void AttachToTransform(Transform target, bool useLocal)
        {
            m_transProvider.target = target;
            m_transProvider.useLocal = useLocal;
            AttachToPose(target == null ? null : m_transProvider);
        }

        public void AttachToPose(IPoseGetter target)
        {
            m_target = target;
        }

        public void Detach()
        {
            AttachToTransform(null);
        }

        private void AddForce(Vector3 from, Vector3 to, float duration)
        {
            var diffPos = to - from;
            var velocity = Mathf.Approximately(diffPos.sqrMagnitude, 0f) ? Vector3.zero : diffPos / duration;

            rigid.AddForceAtPosition(velocity, transform.TransformPoint(anchorPos), ForceMode.VelocityChange);
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