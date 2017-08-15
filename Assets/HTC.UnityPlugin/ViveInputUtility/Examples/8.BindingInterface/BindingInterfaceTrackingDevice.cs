using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class BindingInterfaceTrackingDevice : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
{
    [SerializeField]
    private Image m_imageModel;
    [SerializeField]
    private Button m_button;

    public uint deviceIndex { get; set; }
    public string deviceSN { get; private set; }

    public event Action<string> onClick;
    public event Action<string> onEnter;
    public event Action<string> onExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) { onEnter(deviceSN); }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onEnter != null) { onExit(deviceSN); }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
