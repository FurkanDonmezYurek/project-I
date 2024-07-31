using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems; 

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image bowl; // Tabağı DragHandler'da kullanmak için ekle
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        Debug.Log("Berry drag başlatıldı.");
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        Debug.Log("Berry sürükleniyor.");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        Debug.Log("Berry drag bitirildi.");

        // Eğer böğürtlen tabakla kesişiyorsa, böğürtleni tabağa yerleştir
        if (RectTransformUtility.RectangleContainsScreenPoint(
                bowl.rectTransform, 
                Input.mousePosition, 
                canvas.worldCamera))
        {
            // Böğürtlen tabağa yerleştirildiğinde yapılacak işlemler
            gameObject.SetActive(false);
            FindObjectOfType<BerryPlate>().PlaceBerryInBowl();
            Debug.Log("Berry tabağa yerleştirildi.");
        }
    }
}