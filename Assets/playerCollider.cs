using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCollider : MonoBehaviour {

    private playerController pController;

    private BoxCollider2D box;
    private float boxHeight;
    private float boxWidth;

    private Vector2 origin;

    private Transform knob;

    private bool killer = false;
    private bool killed = false;

    void Start () {
        pController = transform.GetComponent<playerController>();
        box = transform.GetComponent<BoxCollider2D>();
        boxHeight = box.size.y * transform.localScale.y;
        boxWidth = box.size.x * transform.localScale.x;

        origin = transform.position;

        knob = transform.Find("knob");
    }

    private Dictionary<GameObject, string> objectDict = new Dictionary<GameObject, string>();

    private Dictionary<string, GameObject> sideDict = new Dictionary<string, GameObject>();

    bool ground = false;

    void OnCollisionEnter2D(Collision2D col)
    {
        string tag = col.gameObject.tag;

        string side = sideOfCollision(col.contacts);
        //print(side);

        if (side == "top" && tag == "Player") killed = true; 
        else if (side == "bottom" && tag == "Player") killer = true;

        objectDict[col.gameObject] = side;
        sideDict[side] = col.gameObject;

        portalHandling(col.gameObject);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        string side = objectDict[col.gameObject];
        objectDict.Remove(col.gameObject);
        sideDict.Remove(side);
    }

    string sideOfCollision(ContactPoint2D[] contacts)
    {
        Vector2 point = contacts[0].point;
        if (contacts.Length == 2) point = (contacts[0].point + contacts[1].point) / 2;

        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        knob.position = point;

        float angle = Vector2.SignedAngle(Vector2.up, pos-point);

        //print(angle);
        if (angle >= -45 && angle < 45) return "bottom";
        if (angle >= 45 && angle < 135) return "right";
        if (angle >= 135 || angle < -135) return "top";
        if (angle < -45 && angle >= -315) return "left";
        return "left";
    }

    public bool getKiller()
    {
        return killer;
    }

    public bool getKilled()
    {
        return killed;
    }

    public void reset()
    {
        transform.position = origin;
        killer = false; killed = false;
    }

    void portalHandling(GameObject portal)
    {
        if (portal.layer != 10) return; //check if portal

        Vector3 increment = portal.GetComponentInParent<portalHandler>().getTeleportIncrement(portal, pController.getPlayerSize());
        transform.position += increment;
    }

    public bool isBottom()
    {
        return sideDict.ContainsKey("bottom");
    }
    public bool isRight()
    {
        return sideDict.ContainsKey("right");
    }
    public bool isLeft()
    {
        return sideDict.ContainsKey("left");
    }
    public bool isUp()
    {
        return sideDict.ContainsKey("up");
    }

}
