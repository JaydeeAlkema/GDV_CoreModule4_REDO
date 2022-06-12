using UnityEngine;

public class GridTile : MonoBehaviour, ISelectable
{
    public Material defaultMaterial;
    public Material OnSelectedMaterial;

    private Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        renderer.material = defaultMaterial;
    }

    private void OnMouseOver()
    {
        renderer.material = OnSelectedMaterial;
    }

    private void OnMouseExit()
    {
        renderer.material = defaultMaterial;
    }

    public string Select()
    {
        return gameObject.name;
    }
}
