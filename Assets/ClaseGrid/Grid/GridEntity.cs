using System;
using UnityEngine;

//[ExecuteInEditMode]
public class GridEntity : MonoBehaviour
{
	public event Action<GridEntity> OnMoveCallback = delegate {};
	public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;
    Renderer _rend;

    public Color OnGridColor, notOnGridColor;
    private void Awake()
    {
        _rend = GetComponent<Renderer>();
    }

    private void LateUpdate()
    {
        _rend.material.color = onGrid ? OnGridColor : notOnGridColor;
    }
   
    public void OnMove(Vector3 velocity)
    {
        this.velocity = velocity;
        OnMoveCallback(this);
    }
}
