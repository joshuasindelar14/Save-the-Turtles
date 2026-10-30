using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (!TryGetClick(out Vector2 screenPos)) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            Enemy e = hit.collider.GetComponentInParent<Enemy>();
            if (e != null)
            {
                e.Scare();
                return;
            }
        }
    }

    bool TryGetClick(out Vector2 pos)
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            pos = Pointer.current.position.ReadValue();
            return true;
        }

        pos = default;
        return false;
    }
}