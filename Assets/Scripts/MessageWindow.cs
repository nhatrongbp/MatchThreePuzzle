using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectXformMover))]
public class MessageWindow : MonoBehaviour
{
    public Image messageIcon;
    public Text messageText, buttonText;
    public void ShowMessage(Sprite sprite=null, string messageTxt="foo", string buttonTxt="foo"){
        messageIcon.sprite = sprite;
        messageText.text = messageTxt;
        buttonText.text = buttonTxt;
    }
}
