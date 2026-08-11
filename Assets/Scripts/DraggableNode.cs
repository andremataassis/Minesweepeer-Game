using UnityEngine;

public class DraggableNode : MonoBehaviour
{
    private Vector3 mOffset;
    private float mZCoord;
    private bool dragging = false;
    public DropRoot current_root = null;
    public RobotCommand command = RobotCommand.None;

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
        DeattachFromRoot();
        dragging = false;
    }
    public void AttachToRoot(DropRoot root)
    {
        if (dragging == true || current_root == root) return;
        transform.SetParent(root.transform);
        transform.position = root.GetComponent<Collider2D>().bounds.center;
        current_root = root;
        current_root.AttachNode(this);
    }

    public void DeattachFromRoot()
    {
        if (current_root == null) return;
        transform.SetParent(current_root.transform.parent);
        current_root.RemoveNode(this);
        current_root = null;
    }

    public void SetCommand(RobotCommand new_command) { command = new_command; }
}
