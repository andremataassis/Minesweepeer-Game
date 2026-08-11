using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DropRoot : MonoBehaviour
{
    [SerializeField] public GameObject attach_zone_ref;
    private BoxCollider2D collider;
    public List<DraggableNode> nodes;
    public string trigger = null;
    private Vector2 start_offset;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        start_offset = collider.offset;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attach_zone_ref.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void UpdateColliderPosition()
    {
        collider.offset = new Vector2(0, (start_offset.y)* nodes.Count + (start_offset.y));
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        DraggableNode node = collision.gameObject.GetComponent<DraggableNode>();
        if (node == null) return;

        attach_zone_ref.SetActive(true);
        attach_zone_ref.transform.position = collider.bounds.center;
        node.AttachToRoot(this);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        attach_zone_ref.SetActive(false);
    }

    public void AttachNode(DraggableNode node)
    {
        if (node.current_root != this) return;
        nodes.Add(node);
        UpdateColliderPosition();
    }

    public void RemoveNode(DraggableNode node)
    {
        if (node.current_root != this) return;
        nodes.Remove(node);
        ArrangeNodes();
    }

    private void ArrangeNodes()
    {
        UpdateColliderPosition();
        for(int i = 0; i < nodes.Count; i++) 
        {
            DraggableNode node = nodes[i];
            Vector3 offset = new Vector3(0, (start_offset.y) + i*start_offset.y, 0);
            node.gameObject.transform.position = transform.position + offset;
        }
    }

    public List<RobotCommand> GetCommandList()
    {
        List<RobotCommand> list = new List<RobotCommand>();
        for (int i = 0; i < nodes.Count; i++)
        {
            DraggableNode node = nodes[i];
            list.Add(node.command);
        }
        return list;
    }

    public void SetTrigger(string trigger_name) { trigger = trigger_name; }
}
