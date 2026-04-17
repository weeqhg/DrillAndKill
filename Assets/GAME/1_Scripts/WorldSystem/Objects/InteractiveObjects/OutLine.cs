using UnityEngine;

public class OutLine : MonoBehaviour
{
    [SerializeField] private GameObject outlineObject;

    public void SetActive(bool state)
    {
        if (outlineObject != null)
            outlineObject.SetActive(state);
    }
}
