using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static HenrysUtils;

public class Window : MonoBehaviour, IDragHandler, IPointerDownHandler{

    [Header("Generic Variables")]
    public RectTransform dragRectTransform;
    [SerializeField] Canvas canvas;
    public SoundEffectLookup SFX_Lookup;
    [SerializeField] float offset = 60;
    public bool CloseOnSecondOpen = true;
    [SerializeField] bool OpenedByDefault = false;
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
            PlaySFX("UI_1", SFX_Lookup);
            SilentOpen();
        }
    }
    public void SilentOpen(){
        RandomisePosition();
        dragRectTransform.gameObject.SetActive(true);
    }

    public void Close(){
        PlaySFX("UI_2", SFX_Lookup);
        SilentClose();
    }
    public void SilentClose(){
        dragRectTransform.gameObject.SetActive(false);
    }

    public void RandomisePosition(){
        dragRectTransform.anchoredPosition = pos + new Vector2(Random.Range(-offset, offset), Random.Range(-offset, offset));
    }
}