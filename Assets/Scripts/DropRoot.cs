using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DropRoot : MonoBehaviour
{
    [SerializeField] public GameObject attach_zone_ref;
    private Color attach_zone_color;
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    public List<DraggableNode> nodes = new List<DraggableNode>();
    public string trigger = null;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        attach_zone_color = attach_zone_ref.GetComponent<Image>().color;
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

        attach_zone_ref.transform.SetAsLastSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        SetAttachZone(false);
    }

    public void RemoveNode(DraggableNode node)
    {
        if (nodes.Contains(node))
        {
            nodes.Remove(node);
            attach_zone_ref.transform.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
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
