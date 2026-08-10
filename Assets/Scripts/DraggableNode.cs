using UnityEngine;

public class DraggableNode : MonoBehaviour
{
    private Vector3 mOffset;
    private float mZCoord;
    private bool dragging = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        // Store offset between object center and cursor point
        mOffset = gameObject.transform.position - GetMouseWorldPos();
        dragging = true;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + mOffset;
    }

    private void OnMouseUp()
    {
        dragging = false;
    }
    public void AttachToRoot(DropRoot root)
    {
        if (dragging == true) return;
        transform.SetParent(root.transform);
        transform.position = root.GetComponent<Collider2D>().bounds.center;
    }
}
