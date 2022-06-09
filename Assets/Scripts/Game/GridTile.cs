using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public Material defaultMaterial;
    public Material OnSelectedMaterial;

    private Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        renderer.material = defaultMaterial;
    }

    private void OnMouseDown()
    {
        Destroy(gameObject);
    }

    private void OnMouseOver()
    {
        renderer.material = OnSelectedMaterial;
    }

    private void OnMouseExit()
    {
        renderer.material = defaultMaterial;
    }
}
