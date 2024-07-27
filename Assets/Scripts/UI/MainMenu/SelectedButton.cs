using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class SelectedButton : MonoBehaviour, IPointerClickHandler
{ 
    public void OnPointerClick(PointerEventData eventData)
    {
        string buttonName = eventData.pointerPress.name;
        //string buttonName = gameObject.name;
        saveCharacter(buttonName);
    }

    public void saveCharacter(string characterName)
    {
        PlayerPrefs.SetString("PlayerCharacter", characterName);
        string playerChr = PlayerPrefs.GetString("PlayerCharacter");
        print("Seçilen Karakter: " + playerChr);
    }
}
