using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portalHandler : MonoBehaviour {

    private float right_to_left_x;
    private float left_to_right_x;
    private float horizontalDist;
    private float verticalDist;

    void Start () {
        Transform right = transform.GetChild(0);
        Transform left = transform.GetChild(1);
        Transform bottom = transform.GetChild(2);
        Transform top = transform.GetChild(3);

        float rightThicknessX = right.GetComponent<BoxCollider2D>().size.x * right.localScale.x;
        float leftThicknessX = left.GetComponent<BoxCollider2D>().size.x * left.localScale.x;
        float bottomThicknessY = bottom.GetComponent<BoxCollider2D>().size.y * bottom.localScale.y;
        float topThicknessY = top.GetComponent<BoxCollider2D>().size.y * top.localScale.y;

        horizontalDist = right.position.x - left.position.x - rightThicknessX / 2 - leftThicknessX / 2;
        verticalDist = top.position.y - bottom.position.y - topThicknessY / 2 - bottomThicknessY / 2;
    }

    public Vector3 getTeleportIncrement(GameObject portal, float playerSize)
    {
        if (portal.tag == "portalRight") return new Vector3(- (horizontalDist - playerSize), 0, 0);
        if (portal.tag == "portalLeft") return new Vector3((horizontalDist - playerSize), 0, 0);
        if (portal.tag == "portalBottom") return new Vector3(0, verticalDist - playerSize, 0);
        if (portal.tag == "portalTop") return new Vector3(0, -(verticalDist - playerSize), 0);
        return Vector3.zero;
    }
}
