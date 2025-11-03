using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static GenericUtils;
using EventUtils;

public class MessageDisplayWindow : Window
{
    [Header("Message Display Specifics")]
    [SerializeField] TMP_Text Body;
    const float rotation_size = 5;

    public void Setup(string username, MessageContents message){
        if(!ValidateMessage(message.Body(), 120)){
            SilentClose();
        }

        PlaySFX(OpenSFX);
        SilentOpen();

        dragRectTransform.eulerAngles = new Vector3(0,0,Random.Range(-rotation_size, rotation_size));

        string msg = Body.text;
        msg = msg.Replace("{0}", username);
        msg = msg.Replace("{1}", message.Header());
        msg = msg.Replace("MESSAGE HERE", message.Body());
        Body.text = msg;
    }
}
