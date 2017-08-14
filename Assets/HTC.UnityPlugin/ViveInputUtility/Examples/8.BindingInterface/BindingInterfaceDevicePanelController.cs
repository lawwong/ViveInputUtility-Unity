using UnityEngine;
using System.Collections;

public class BindingInterfaceDevicePanelController : MonoBehaviour
{
    [SerializeField]
    private Animator m_animator;

    public void SetAnimatorIsEditing(bool value)
    {
        m_animator.SetBool("isEditing", value);
    }
}
