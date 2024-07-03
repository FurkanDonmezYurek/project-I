using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

public class MatchItem : MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerEnterHandler,IPointerUpHandler,IPointerClickHandler
{
    static MatchItem hoverItem;
    
    public GameObject linePrefab;
    public string itemName;

    private GameObject line;

    public void OnPointerDown(PointerEventData eventData)
    {
        line = Instantiate(linePrefab, transform.position, Quaternion.identity, transform.parent.parent);
        UpdateLine(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateLine(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!this.Equals(hoverItem) && itemName.Equals(hoverItem.itemName))
        {
            UpdateLine(hoverItem.transform.position);
            Destroy(hoverItem);
            Destroy(this);
            MatchLogic.AddPoint();
        }
        else
        {
            Destroy(line);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverItem = this;
    }
    
    
    
    void UpdateLine(Vector3 position)
    {
        //Update Direction
        Vector3 direction = position - transform.position;
        line.transform.right = direction;

        //Scale Update
        line.transform.localScale = new Vector3(direction.magnitude, 1, 1);

    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
