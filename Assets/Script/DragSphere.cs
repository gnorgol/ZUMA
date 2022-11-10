using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class DragSphere : MonoBehaviour
{
    private Vector3 mousePositionOffset;
    private Vector3 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    private void OnMouseDown()
    {
        if (EditorLevelManager.Instance.FunctionEditorLevelActive == FunctionEditorLevel.Move)
        {
            mousePositionOffset = gameObject.transform.position - GetMouseWorldPosition();
            EventManager.Instance.Raise(new MoveSphereEvent());
        }
        
    }
    private void OnMouseDrag()
    {
        if (EditorLevelManager.Instance.FunctionEditorLevelActive == FunctionEditorLevel.Move)
        {
            transform.position = GetMouseWorldPosition() + mousePositionOffset;
            EventManager.Instance.Raise(new MoveSphereEvent() );
        }
        
    }
}
