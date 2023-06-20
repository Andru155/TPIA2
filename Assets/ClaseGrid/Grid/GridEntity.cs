using System;
using UnityEngine;

//[ExecuteInEditMode]
public class GridEntity : MonoBehaviour
{
	public event Action<GridEntity> OnMoveCallback = delegate {};
	public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;
    Renderer _rend;

    private void Awake()
    {
        _rend = GetComponent<Renderer>();
    }   

 //   void Update() {
 //       if (onGrid)
 //           _rend.material.color = Color.red;
 //       else
 //           _rend.material.color = Color.gray;
	//	//Optimization: Hacer esto solo cuando realmente se mueve y no en el update
	//	transform.position += velocity * Time.deltaTime;
	//    OnMove(this);
	//}
    public void OnMove(Vector3 velocity)
    {
        this.velocity = velocity;
        OnMoveCallback(this);
    }
}
