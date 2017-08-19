using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using HTC.UnityPlugin.VRModuleManagement;

[RequireComponent(typeof(Image))]
public class TestTrackingDeviceIcon : MonoBehaviour
{
    public VRModuleDeviceModel model;

    private Image m_image;

    private void Awake()
    {
        m_image = GetComponent<Image>();
    }

    

    [ContextMenu("Reset Pivot")]
    private void ResetPivot()
    {
        //var image = GetComponent<Image>();
        //var sprite = image.sprite;
        //var spriteRect = sprite.rect;
        //var spritePivot = sprite.pivot;
        //image.SetNativeSize();
        //image.rectTransform.pivot = new Vector2(spritePivot.x / spriteRect.width, spritePivot.y / spriteRect.height);

        //var localScale = image.transform.localScale;
        //image.transform.localScale = Vector3.Scale(new Vector3(scale, scale, 1f), new Vector3(Mathf.Sign(localScale.x), Mathf.Sign(localScale.x), Mathf.Sign(localScale.x)));
    }
}
