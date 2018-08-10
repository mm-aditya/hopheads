using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCollider : MonoBehaviour {

    private playerController pController;
    public playerController otherController;

    private BoxCollider2D box;
    private float boxHeight;
    private float boxWidth;

    private Vector2 origin;

    private Transform knob;

    private GameObject killer;
    private bool killed = false;

    private bool touchAreaHazard = false;
    private bool touchPlayerTop = false;
    private bool touchPlayerLeft = false;
    private bool touchPlayerRight = false;
    private Collision2D touchCol;

    void Start () {
        pController = transform.GetComponent<playerController>();
        box = transform.GetComponent<BoxCollider2D>();
        boxHeight = box.size.y * transform.localScale.y;
        boxWidth = box.size.x * transform.localScale.x;

        origin = transform.position;

        knob = transform.Find("knob");
    }

    private Dictionary<GameObject, string> objectDictFeet = new Dictionary<GameObject, string>();
    private Dictionary<string, GameObject> sideDictFeet = new Dictionary<string, GameObject>();

    private Dictionary<GameObject, string> objectDictBody = new Dictionary<GameObject, string>();
    private Dictionary<string, GameObject> sideDictBody = new Dictionary<string, GameObject>();

    bool ground = false;

    void OnCollisionEnter2D(Collision2D col)
    {

        string tag = col.collider.gameObject.tag;
        string mytag = col.otherCollider.gameObject.tag;

        if (mytag != "Body" && mytag != "Feet") return; //if this collider is not player, ignore

        string side = sideOfCollision(col.contacts);

        if (tag == "platform" && mytag == "Feet")
        {
            if (objectDictFeet.ContainsKey(col.gameObject)) sideDictFeet.Remove(objectDictFeet[col.gameObject]);
            objectDictFeet[col.gameObject] = "bottom";
            sideDictFeet["bottom"] = col.gameObject;
        }
        else if (tag == "platform" && mytag == "Body")
        {
            if (objectDictBody.ContainsKey(col.gameObject)) sideDictBody.Remove(objectDictBody[col.gameObject]);
            objectDictBody[col.gameObject] = side;
            sideDictBody[side] = col.gameObject;
        }

        if (tag == "Body" && mytag == "Body")
        {
            if (side == "right") touchPlayerRight = true;
            if (side == "left") touchPlayerLeft = true;
        }

        if ((mytag == "Body" && tag == "Feet") || (tag == "AreaHazard"))
        {
            registerKill(col);
            if (tag == "Feet") touchPlayerTop = true;
            if (tag == "AreaHazard") touchAreaHazard = true;
            touchCol = col;
        }

        else if (tag == "Powerup") registerPowerup(col);
        else if (tag == "Parry") registerParried(col, side);

        portalHandling(col.gameObject);
    }

    void OnCollisionExit2D(Collision2D col)
    {

        string tag = col.collider.gameObject.tag;
        string mytag = col.otherCollider.gameObject.tag;

        if (mytag != "Body" && mytag != "Feet") return;

        if (tag == "Feet") touchPlayerTop = false;
        if (tag == "Body") { touchPlayerLeft = false; touchPlayerRight = false; }
        if (tag == "AreaHazard") touchAreaHazard = false;

        if (tag != "platform") return;
        else if (tag == "platform")
        {
            if (mytag == "Body") { 
            string side = objectDictBody[col.gameObject];
            objectDictBody.Remove(col.gameObject);
            sideDictBody.Remove(side);
            } else { 
                string side = objectDictFeet[col.gameObject];
                objectDictFeet.Remove(col.gameObject);
                sideDictFeet.Remove(side);
            }
        }
    }

    string sideOfCollision(ContactPoint2D[] contacts)
    {
        Vector2 point = contacts[0].point;
        if (contacts.Length == 2) point = (contacts[0].point + contacts[1].point) / 2;

        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        knob.position = point;

        float angle = Vector2.SignedAngle(Vector2.up, pos-point);

        //print(angle);
        //if (angle >= -45 && angle < 45) return "bottom";
        if (angle >= 0 && angle < 135) return "right";
        if (angle >= 135 || angle < -135) return "top";
        if (angle < 0 && angle >= -315) return "left";
        return "left";
    }

    void registerKill(Collision2D col)
    {
        print("Registering kill");
        touchPlayerTop = false;
        touchAreaHazard = false;

        if (pController.getKilledSafeState() || col.otherCollider.GetComponentInParent<playerController>().getKilledSafeState() || killed || col.otherCollider.GetComponentInParent<playerCollider>().getKill()) {
            print("safe");
            return;
        }

        if (pController.powerup_getMushroom()){
            pController.powerup_DectivateMushroom();
            return;
        }

        killed = true;
        killer = col.collider.gameObject;
    }

    public void checkPlayerTopTouch() {
        if (touchPlayerTop) registerKill(touchCol);
    }
    public void checkAreaHazardTouch() {
        if (touchAreaHazard) registerKill(touchCol);
    }
    public bool getPlayerRightTouch()
    {
        return touchPlayerRight;
    }
    public bool getPlayerLeftTouch()
    {
        return touchPlayerLeft;
    }

    void registerPowerup(Collision2D col)
    {
        GameObject powerup = col.gameObject;

        if (powerup.name == "mushroom")
        {
            pController.powerup_ActivateMushroom();
        }

        if (powerup.name == "flip")
        {
            otherController.get_flipPowerup();
        }

        if (powerup.name == "slow")
        {
            otherController.get_slowPowerup();
        }

        pController.destroy_powerup(powerup);
    }

    void registerParried(Collision2D col, string side)
    {
        if (pController.getKilledSafeState() || col.otherCollider.GetComponentInParent<playerController>().getKilledSafeState() || killed || col.otherCollider.GetComponentInParent<playerCollider>().getKill()) return;
        col.otherCollider.gameObject.GetComponentInParent<playerController>().getParried(side);
        print(col.otherCollider.gameObject.name+" kenna parried from " +side);
    }

    public bool getKill() { return killed; }
    public void setKill(bool state) { killed = state; }
    public GameObject getKiller() { return killer; }

    public Vector3 getOtherPlayerPosition()
    {
        return otherController.transform.position;
    }

    public void reset(Vector3 pos)
    {
        transform.rotation = Quaternion.identity;
        transform.position = pos;
        killed = false;
        killer = null;
        touchPlayerTop = false;
        touchAreaHazard = false;
        pController.setKilledSafeState();
    }

    void portalHandling(GameObject portal)
    {
        if (portal.layer != 10) return; //check if portal

        Vector3 increment = portal.GetComponentInParent<portalHandler>().getTeleportIncrement(portal, pController.getPlayerSize());
        transform.position += increment;
    }

    public bool isBottom()
    {
        return sideDictFeet.ContainsKey("bottom");
    }
    public bool isRight()
    {
        return sideDictBody.ContainsKey("right");
    }
    public bool isLeft()
    {
        return sideDictBody.ContainsKey("left");
    }
    public bool isUp()
    {
        return sideDictBody.ContainsKey("up");
    }

}
