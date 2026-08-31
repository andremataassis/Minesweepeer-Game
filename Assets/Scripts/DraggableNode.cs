using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DraggableNode : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 pointerOffset;
    private bool dragging = false;
    public DropRoot current_root = null;
    public RobotCommand command = RobotCommand.None;
    private DropRoot hovered_root;
    [Header("Sprites")]
    [SerializeField] public Sprite left_command_sprite;
    [SerializeField] public Sprite right_command_sprite;
    private Image image_component;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image_component = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;

        if (current_root != null)
        {
            transform.SetParent(current_root.transform.parent, true);
            current_root.RemoveNode(this);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        pointerOffset = localPoint;
    }


    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log(eventData == null);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rectTransform.anchoredPosition = localPoint - pointerOffset;

        CheckForDropRootUnderCursor(eventData);
    }

    public void AttachToRoot(DropRoot root)
    {
        current_root = root;
        current_root.AttachNode(this);
        hovered_root = null;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (hovered_root != null)
        {
            AttachToRoot(hovered_root);
        }
        else if (current_root != null)
        {
            DeattachFromRoot();
        }
    }

    private void CheckForDropRootUnderCursor(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        DropRoot foundRoot = null;

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == gameObject) continue;

            GameObject foundAttachZone = result.gameObject;
            if (foundAttachZone.name != "AttachZone") break;
            else
            {
                foundRoot = foundAttachZone.transform.parent.GetComponent<DropRoot>();
            }
        }

        if (foundRoot != hovered_root)
        {
            if (hovered_root != null) hovered_root.HandlePointerExit();
            hovered_root = foundRoot;
            if (hovered_root != null) hovered_root.HandlePointerEnter();
        }
    }

    private void DeattachFromRoot()
    {
        current_root.RemoveNode(this);
        current_root = null;
    }

    public void SetCommand(RobotCommand new_command)
    {
        command = new_command;
        image_component = GetComponent<Image>();
        switch (command)
        {
            case RobotCommand.TurnLeft:
                image_component.sprite = left_command_sprite;
                break;
            case RobotCommand.TurnRight:
                image_component.sprite = right_command_sprite;
                break;
        }
    }
}
