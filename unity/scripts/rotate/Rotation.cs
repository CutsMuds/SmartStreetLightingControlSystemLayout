using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public float speed;
    [SerializeField]
    GameObject poleX;

    [SerializeField]
    GameObject poleY;

    Transform xTransform;
    Transform yTransform;
    void Start()
    {
        xTransform = poleX.GetComponent<Transform>();
        yTransform = poleY.GetComponent<Transform>();
    }
    Vector2 lastMove = Vector2.zero;
    bool wasTouch = false;
    void Update()
    {
        if (IsTouch())
        {
            if (!wasTouch) lastMove = getPosition();
            wasTouch = true;

            Vector2 diff = lastMove - getPosition();
            
            if(xTransform.eulerAngles.x + diff.y * speed > 90)
            {
                diff.y = -(90 - xTransform.eulerAngles.x) / speed;
            }
            if(xTransform.eulerAngles.x + diff.y * speed < 5)
            {
                diff.y = (5 - xTransform.eulerAngles.x) / speed;
            }

            xTransform.Rotate(new Vector3(diff.y * speed, 0, 0));
            yTransform.Rotate(new Vector3(0, -diff.x * speed, 0));
            lastMove = getPosition();
        }
        else
        {
            wasTouch = false;
        }
    }

    bool IsTouch()
    {
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount == 1) return true;
            return false;
        }
        else
        {
            if (Input.GetMouseButton(0)) return true;
            return false;
        }
    }
    Vector2 getPosition()
    {
        if (Application.isMobilePlatform)
        {
            Touch touch = Input.GetTouch(0);
            return touch.position;
        }
        else return Input.mousePosition;
    }
}