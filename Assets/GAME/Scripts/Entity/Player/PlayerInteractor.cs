using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private CanvasGroup hintCanvas;
    [SerializeField] private LayerMask interactLayer;
    private IInteractable current;
    private float verticalPadding = 1f;
    private Renderer targetRenderer;
    public void Initialized()
    {
        G.InputManager.Actions.Player.Interact.performed += OnHandleTryInteract;
    }

    private void Update()
    {
        if (Camera.main == null) return;

        HandleFocus();

        if (targetRenderer != null)
        {
            Vector3 top = targetRenderer.bounds.center + Vector3.up * (targetRenderer.bounds.extents.y + verticalPadding);

            float maxHeight = transform.position.y + 5f;

            if (top.y > maxHeight)
            {
                Vector3 hintPosition = new Vector3(top.x, transform.position.y + 5f, top.z);
                Vector3 directionToPlayer = (transform.position - targetRenderer.bounds.center).normalized;
                float pushDistance = targetRenderer.bounds.extents.x;
                hintCanvas.transform.position = hintPosition + directionToPlayer * pushDistance;
            }
            else
            {
                hintCanvas.transform.position = top;
            }

            hintCanvas.transform.LookAt(hintCanvas.transform.position + Camera.main.transform.forward);
        }
    }

    private void HandleFocus()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f + Camera.main.transform.forward * 1.5f;
        float radius = 4f;

        Collider[] hits = Physics.OverlapSphere(origin, radius, interactLayer);

        if (hits.Length > 0)
        {
            IInteractable interactable = hits[0].GetComponentInParent<IInteractable>();

            if (interactable.IsUsed())
            {
                current?.OnLoseFocus();
                HideHint();
                return;
            }

            current = interactable;
            current?.OnFocus();
            ShowHint(hits[0].transform);
        }
        else
        {
            if (current != null)
            {
                current.OnLoseFocus();
                HideHint();
                current = null;
            }
        }
    }

    private void ShowHint(Transform newTarget)
    {
        targetRenderer = newTarget.GetComponentInChildren<Renderer>();
        hintCanvas.alpha = 1f;
    }

    private void HideHint()
    {
        targetRenderer = null;
        hintCanvas.alpha = 0f;
    }

    private void OnHandleTryInteract(InputAction.CallbackContext ctx)
    {
        current?.Interact(this);
    }

    private void OnDestroy()
    {
        if (G.InputManager != null) G.InputManager.Actions.Player.Interact.performed -= OnHandleTryInteract;
    }
}

