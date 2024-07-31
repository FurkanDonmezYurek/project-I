using UnityEngine;
using UnityEngine.EventSystems;

public class MatchItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerEnterHandler, IPointerUpHandler
{
    static MatchItem hoverItem;
    
    public GameObject linePrefab; // Line prefab referansı
    public string itemName; // Eşleşme adı

    private GameObject line; // Çizgi GameObject
    private LineRenderer lineRenderer; // LineRenderer bileşeni

    public void OnPointerDown(PointerEventData eventData)
    {
        // Çizgi prefab'ını oluştur
        line = Instantiate(linePrefab, transform.position, Quaternion.identity, transform.parent);
        lineRenderer = line.GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2; // 2 pozisyon belirle
            lineRenderer.SetPosition(0, transform.position); // Başlangıç noktasını ayarla
            UpdateLine(eventData.position);
        }
        else
        {
            Debug.LogError("LineRenderer component is missing on the linePrefab.");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateLine(eventData.position); // Çizgiyi güncelle
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (hoverItem != null && !this.Equals(hoverItem) && itemName.Equals(hoverItem.itemName))
        {
            UpdateLine(hoverItem.transform.position); // Çizgiyi bitiş noktasına güncelle
            Destroy(hoverItem.gameObject); // HoverItem'ı yok et
            Destroy(this.gameObject); // Bu nesneyi yok et
            MatchLogic.AddPoint(); // Puan ekle
        }
        else
        {
            Destroy(line); // Eşleşme sağlanamazsa çizgiyi yok et
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverItem = this; // HoverItem'ı güncelle
    }

    void UpdateLine(Vector3 position)
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(1, position); // Çizginin bitiş noktasını güncelle
        }
        else
        {
            Debug.LogError("LineRenderer is null in UpdateLine.");
        }
    }
}
