using UnityEngine;

public class DropRoot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        DraggableNode node = collision.gameObject.GetComponent<DraggableNode>();
        if (node == null) return;

        node.AttachToRoot(this);
    }
}
