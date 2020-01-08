using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringAgent : MonoBehaviour
{

    public Vector3 desiredVelocity;
    public Vector3 seekVelocity;
    public Vector3 fleeVelocity;
    public Vector3 totalVelocity;
    public Vector3 pos;
    public Vector3 nextPos;
    public Vector3 velocity;

    public string currentlyAvoiding; //this is jsut to debug, delete after

    public float rayDistance = 1.5f;

    public Vector3 agentBounds;

    public float speed;
    public float fleeClamp = 1;
    public float fleeClampScalar = 2;

    public static float minZ = -6.75f;
    public static float maxZ = -2;
    public static float minX = -11;
    public static float maxX = 17.83f;

    void Awake()
    {
        agentBounds = this.gameObject.GetComponent<Collider>().bounds.size;
        desiredVelocity = new Vector3();
        fleeVelocity = new Vector3();
        totalVelocity = new Vector3();
        velocity = new Vector3((maxX + 2) - transform.position.x, transform.position.y, transform.position.z);
        seekVelocity = new Vector3(velocity.x, velocity.y, velocity.z);
    }

    public Vector3 u, v, uu, w; //TO DEBUG DELETE AFTER
    public float differenceMag; //TO DEBUG DELETE AFTER

    public Vector3 DEBUGGERDISTANCEVEC;

}
