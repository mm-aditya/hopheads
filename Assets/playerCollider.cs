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

    private GameObject killer;
    private bool killed = false;

    private bool touchAreaHazard = false;
    private bool touchPlayerTop = true;
    private Collision2D touchCol;

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
        if (col.otherCollider.gameObject.tag != "Player") return; //if this collider is not player, ignore

        string tag = col.collider.gameObject.tag;

        string side = sideOfCollision(col.contacts);

        if (tag == "platform")
        {
            objectDict[col.gameObject] = side;
            sideDict[side] = col.gameObject;
        }

        if ((side == "top" && tag == "Player") || (tag == "AreaHazard"))
        {
            registerKill(col);
            if (tag == "Player") touchPlayerTop = true;
            if (tag == "AreaHazard") touchAreaHazard = true;
            touchCol = col;
        }

        else if (tag == "Powerup") registerPowerup(col);
        else if (tag == "Parry") registerParried(col, side);

        portalHandling(col.gameObject);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.otherCollider.gameObject.tag != "Player") return;

        string tag = col.collider.gameObject.tag;

        if (tag == "Player") touchPlayerTop = false;
        if (tag == "AreaHazard") touchAreaHazard = false;

        if (tag != "platform") return;
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

    void registerKill(Collision2D col)
    {
        if (pController.getKilledSafeState() || col.otherCollider.gameObject.GetComponent<playerController>().getKilledSafeState()) {
            print("safe");
            return;
        }

        if (pController.powerup_getMushroom()){
            pController.powerup_DectivateMushroom();
            return;
        }
        killed = true;
        killer = col.collider.gameObject;

        touchPlayerTop = false;
        touchAreaHazard = false;
    }

    public void checkPlayerTopTouch() {
        if (touchPlayerTop) registerKill(touchCol);
    }
    public void checkAreaHazardTouch() {
        if (touchAreaHazard) registerKill(touchCol);
    }

    void registerPowerup(Collision2D col)
    {
        GameObject powerup = col.gameObject;

        if (powerup.name == "mushroom")
        {
            pController.powerup_ActivateMushroom();
        }

        pController.destroy_powerup(powerup);
    }

    void registerParried(Collision2D col, string side)
    {
        col.otherCollider.gameObject.GetComponent<playerController>().getParried(side);
        print(col.otherCollider.gameObject.name+" kenna parried from " +side);
        
    }

    public bool getKill() { return killed; }
    public GameObject getKiller() { return killer; }

    public void reset(Vector3 pos)
    {
        transform.position = pos;
        killed = false;
        killer = null;
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
