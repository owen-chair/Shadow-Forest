using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class OverlayCanvas : UdonSharpBehaviour
{
    private Canvas m_Canvas;
    public Image m_BloodEffect_Image;

    void Start()
    {
        if (this.m_Canvas == null)
        {
            this.m_Canvas = this.GetComponent<Canvas>();
        }

        if (this.m_BloodEffect_Image == null)
        {
            this.m_BloodEffect_Image = this.GetComponentInChildren<Image>();
        }
    }
}