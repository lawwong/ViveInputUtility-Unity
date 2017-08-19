using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TestDeviceViewController : MonoBehaviour
{
    public Vector2 selfSizeDelta;
    public Image rectImage;
    public float scale = 1f;
    public float border = 20f;
    public Vector2 viewerDelta = new Vector2(500f, 600f);
    public Rect boundRect;

    private RectTransform m_rectTransform;
    private RectTransform m_parentRectTransform;

    private RectTransform rectTransform { get { return m_rectTransform == null ? (m_rectTransform = GetComponent<RectTransform>()) : m_rectTransform; } }
    private RectTransform parentRectTransform { get { return m_parentRectTransform == null ? (m_parentRectTransform = rectTransform.parent.GetComponent<RectTransform>()) : m_parentRectTransform; } }

    public RectTransform[] devices = new RectTransform[0];
    public Vector3[] corners = new Vector3[4];
    public Transform[] reticles = new Transform[4];
    // Update is called once per frame
    void Update()
    {

        boundRect = new Rect()
        {
            xMin = float.MaxValue,
            xMax = float.MinValue,
            yMin = float.MaxValue,
            yMax = float.MinValue,
        };

        devices[0].GetWorldCorners(corners);
        reticles[0].position = corners[0];
        reticles[1].position = corners[1];
        reticles[2].position = corners[2];
        reticles[3].position = corners[3];

        foreach (var childTrans in devices)
        {
            childTrans.GetWorldCorners(corners);

            for (int i = 0; i < corners.Length; ++i)
            {
                corners[i] = transform.InverseTransformPoint(corners[i]);
                //corners[i] = parentRectTransform.InverseTransformPoint(corners[i]);
            }

            boundRect.xMin = Mathf.Min(boundRect.xMin, corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            boundRect.xMax = Mathf.Max(boundRect.xMax, corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            boundRect.yMin = Mathf.Min(boundRect.yMin, corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            boundRect.yMax = Mathf.Max(boundRect.yMax, corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        }

        selfSizeDelta = rectTransform.sizeDelta;

        //boundRect.xMin -= border;
        //boundRect.xMax += border;
        //boundRect.yMin -= border;
        //boundRect.yMax += border;

        rectImage.rectTransform.sizeDelta = new Vector2(boundRect.width, boundRect.height);
        rectImage.rectTransform.localPosition = boundRect.center;

        var myRect = rectTransform.rect;
        var myInnerWidth = viewerDelta.x - (border * 2f);
        var myInnerHeight = viewerDelta.y - (border * 2f);
        var boundRectCenter = boundRect.center;
        rectTransform.pivot = new Vector2((boundRectCenter.x - myRect.x) / myRect.width, (boundRectCenter.y - myRect.y) / myRect.height);
        rectTransform.localPosition = Vector3.zero;

        if (myInnerWidth / myInnerHeight >= boundRect.width / boundRect.height)
        {
            // myRect is wider then boundRect
            scale = myInnerHeight / boundRect.height;
        }
        else
        {
            scale = myInnerWidth / boundRect.width;
        }

        scale = Mathf.Clamp(scale, 0.1f, 5f);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
