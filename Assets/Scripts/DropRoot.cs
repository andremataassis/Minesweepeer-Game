using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DropRoot : MonoBehaviour
{
    [SerializeField] public GameObject attach_zone_ref;
    private Color attach_zone_color;
    public List<DraggableNode> nodes = new List<DraggableNode>();
    public string trigger = null;
    private float rectT_height;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        attach_zone_color = attach_zone_ref.GetComponent<Image>().color;
        rectT_height = rectTransform.rect.height;
        attach_zone_ref.GetComponent<RectTransform>().sizeDelta = rectTransform.sizeDelta;
        ArrangeChildren();
    }

    void Start()
    {
        SetAttachZone(false);
    }

    private void SetAttachZone(bool on)
    {
        if (!on) attach_zone_ref.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        else attach_zone_ref.GetComponent<Image>().color = attach_zone_color;
    }

    public void HandlePointerEnter()
    {
        SetAttachZone(true);
    }

    public void HandlePointerExit()
    {
        SetAttachZone(false);
    }

    public void AttachNode(DraggableNode node)
    {
        if (!nodes.Contains(node))
        {
            nodes.Add(node);
        }

        node.transform.SetParent(this.transform, false);
        node.GetComponent<RectTransform>().localPosition = attach_zone_ref.transform.localPosition;

        attach_zone_ref.transform.SetAsLastSibling();

        ArrangeChildren();
        SetAttachZone(false);
    }

    public void ArrangeChildren()
    {
        int i = 1;
        foreach(Transform child in transform)
        {
            child.GetComponent<RectTransform>().localPosition = new Vector3(0f, rectT_height * 0.8f * -i);
            i += 1;
        }
    }

    public void RemoveNode(DraggableNode node)
    {
        if (nodes.Contains(node))
        {
            nodes.Remove(node);
            attach_zone_ref.transform.SetAsLastSibling();
        }
        ArrangeChildren();
    }

    public List<RobotCommand> GetCommandList()
    {
        List<RobotCommand> list = new List<RobotCommand>();
        for (int i = 0; i < nodes.Count; i++)
        {
            list.Add(nodes[i].command); 
        }
        return list;
    }

    public void SetTrigger(string trigger_name)
    {
        trigger = trigger_name;
    }
}
