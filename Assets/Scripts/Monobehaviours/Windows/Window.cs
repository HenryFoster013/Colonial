using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static GenericUtils;

public class Window : MonoBehaviour, IDragHandler, IPointerDownHandler{

    [Header("Positioning")]
    public RectTransform dragRectTransform;
    [SerializeField] Canvas canvas;
    [SerializeField] float offset = 60;
    
    [Header("Openly / Closing")]
    public bool CloseOnSecondOpen = true;
    [SerializeField] bool OpenedByDefault = false;
    public bool DestroyOnClose = false;

    [Header("Sound Effects")]
    public SoundEffectLookup SFX_Lookup;
    public SoundEffect OpenSFX;
    public SoundEffect CloseSFX;

    Vector2 pos;

    void Start(){
        pos = dragRectTransform.anchoredPosition;
        if(OpenedByDefault)
            SilentOpen();
        else
            SilentClose();
    }

    public void OnDrag(PointerEventData eventData){
        dragRectTransform.anchoredPosition += eventData.delta/canvas.scaleFactor;
    }
    public void OnPointerDown(PointerEventData eventData){
        dragRectTransform.SetAsLastSibling();
    }

    public virtual void Open(){
        if(CloseOnSecondOpen && dragRectTransform.gameObject.activeSelf){
            Close();
        }
        else{
            PlaySFX(OpenSFX);
            SilentOpen();
        }
    }
    public void SilentOpen(){
        RandomisePosition();
        dragRectTransform.gameObject.SetActive(true);
    }

    public void Close(){
        PlaySFX(CloseSFX);
        SilentClose();
        if(DestroyOnClose)
            Destroy(this.gameObject);
    }
    public void SilentClose(){
        dragRectTransform.gameObject.SetActive(false);
    }

    public void RandomisePosition(){
        dragRectTransform.anchoredPosition = pos + new Vector2(Random.Range(-offset, offset), Random.Range(-offset, offset));
    }
}