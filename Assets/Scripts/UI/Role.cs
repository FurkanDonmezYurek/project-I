using TMPro;
using UnityEngine;

public class Role : MonoBehaviour
{
    public TextMeshProUGUI RoleTxt;  // TextMeshPro kullanıyorsanız TextMeshProUGUI kullanmalısınız.
    private AnimationManager _animationManager;
    public Animator Animator;

    private void Start()
    {
        _animationManager = GetComponent<AnimationManager>();

        if (_animationManager == null)
        {
            Debug.LogError("AnimationManager component not found!");
        }

        // Rol metnini başlangıçta güncelle
        UpdateRoleText();
    }

    private void UpdateRoleText()
    {
        if (_animationManager != null)
        {
            RoleTxt.text = $"Current Role Index: {_animationManager.CurrentRoleIndex}";
        }
    }

    // Rol değiştiğinde bu metodu çağırabilirsiniz
    public void OnRoleChanged()
    {
        _animationManager.UpdateAnimationState();
        UpdateRoleText();
    }
}