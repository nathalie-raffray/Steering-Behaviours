using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopperManager : SteeringAgent
{

    private bool setChair = false;
    private bool setShop = false;
    public bool waitInShop = false;
    public bool waitOnChair = false;

    private float shopOffset = 0;

    private Rigidbody rb;

    private bool freeze = false;
    public string goal;

    private float timer=0;
    private float freezeTimer = 0;

    private float shopperY;

    public Material flyeredMaterial;
    public Material normalMaterial;

    public bool flyered = false;
    private float flyeredTimer = 0;

    public Vector3 rbVelocity; //TO DEBUG DELETE AFTER

    private static float neighbourhoodRadius = 3f;

    public static GameObject[] shops;
    private static List<BoardManager.Chair> chairs;

    private GameObject shopGoal;
    private BoardManager.Chair chairGoal;



    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        shopperY = transform.position.y;

        if (Random.Range(0, 100) < 50) goal = "right";
        else goal = "shop";

        shops = GameObject.FindGameObjectsWithTag("shop");
        chairs = BoardManager.chairList;
    }

    void FixedUpdate()
    {

        if (flyered)
        {
            flyeredTimer += Time.deltaTime;
            if(flyeredTimer > 2)
            {
                flyered = false;
                this.gameObject.GetComponent<MeshRenderer>().material = normalMaterial;
            }
        }

        if (freeze)
        {
            freezeTimer += Time.deltaTime;
            this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3();
            if(freezeTimer > 2)
            {
                freeze = false;
            }
        }

        else if (waitInShop)
        {
            timer += Time.deltaTime;
            if(timer >= 1)
            {
                timer = 0;
                waitInShop = false;
                gameObject.GetComponent<Renderer>().enabled = true;
               gameObject.GetComponent<Rigidbody>().isKinematic = false;
                goal = "chair";
            }
        }else if (waitOnChair)
        {
            transform.position = new Vector3(chairGoal.obj.transform.position.x, 0.82f, chairGoal.obj.transform.position.z);
            timer += Time.deltaTime;
            if(timer >= 2.5f) //wait 2 or 3 seconds
            {
                timer = 0;
                waitOnChair = false;
                goal = "right";
                Vector3 offChairPos = chairGoal.obj.transform.position - chairGoal.obj.transform.right.normalized * 0.65f;
                transform.position = new Vector3(offChairPos.x, shopperY, offChairPos.z);
               this.GetComponent<Rigidbody>().isKinematic = false;
                chairGoal.occupied = false;
            }
        }
        else
        {
            Seek(goal);
            if(!waitInShop && !waitOnChair)
            {
                ObstacleAvoidance();
                updatePos();
            }
           
        }
    }

    void updatePos()
    {

        float cancelling = Vector3.Dot(fleeVelocity, seekVelocity);
        if (cancelling < -1.2f)
        {
            totalVelocity = fleeVelocity;
            Debug.Log("cancelling");
        }
        else
        {
            totalVelocity = seekVelocity + fleeVelocity*fleeClamp;
            totalVelocity.Normalize();
        }
        transform.position += new Vector3(totalVelocity.x * Time.deltaTime * speed, 0, totalVelocity.z * Time.deltaTime * speed);
        velocity.Set(totalVelocity.x * Time.deltaTime, 0, totalVelocity.z * Time.deltaTime);
    }

    //public Vector3 debugNextPos;

    void Seek(string goal)
    {
        if (goal.Equals("right"))
        {
            if (transform.position.z < maxZ && transform.position.z > minZ) desiredVelocity = new Vector3((maxX + 2) - transform.position.x, velocity.y, velocity.z);
            else
            {
                desiredVelocity = new Vector3((maxX + 2) - transform.position.x, velocity.y, Random.Range(minZ, maxZ)-transform.position.z); //in case the player is in the shop
            }

            seekVelocity = desiredVelocity - velocity;
        }
        else if (goal.Equals("shop"))
        {
            if (setShop)
            {
                if (arrivedAtShop())
                {
                    gameObject.GetComponent<Renderer>().enabled = false;
                    gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    GameObject[] advertisers = GameObject.FindGameObjectsWithTag("advertiser");
                    for (int i = 0; i < advertisers.Length; i++)
                    {
                        if (advertisers[i].GetComponent<AdvertiserManager2>().leader == this.gameObject)
                        {
                            advertisers[i].GetComponent<AdvertiserManager2>().following = false; //make sure all advertisers who were following this shopper arent following anymore
                        }
                    }
                    waitInShop = true;
                    timer = 0;
                    setShop = false;
                }
                else
                {
                    setShopVelocity();
                }
            }
            else
            {
                shopGoal = shops[(int)Random.Range(0, shops.Length)];
                if (shopGoal.transform.position.z < -5f) shopOffset = shopGoal.GetComponent<Collider>().bounds.size.z-0.5f;
                else shopOffset = -shopGoal.GetComponent<Collider>().bounds.size.z+0.5f;
                setShopVelocity();
                setShop = true;
            }
        }
        else if (goal.Equals("chair"))
        {
            if (setChair)
            {
                if (arrivedAtChair() && !chairGoal.occupied)
                {
                    chairGoal.occupied = true;
                    this.GetComponent<Rigidbody>().isKinematic = true;
                    transform.position.Set(chairGoal.obj.transform.position.x, 0.82f, chairGoal.obj.transform.position.z);
                    waitOnChair = true;
                    timer = 0;
                    setChair = false;
                }
                else
                {
                    setChairVelocity();
                }
            }
            else
            {
               // Shuffle(chairs);
                float tempDist = -1;
                float currDist;
                setChair = false;
                bool freeChairs = false;
                foreach(BoardManager.Chair chair in chairs)
                {
                    currDist = (chair.obj.transform.position - transform.position).magnitude;
                    if (!chair.occupied && (tempDist == -1 || currDist < tempDist)) //try to go to closest chair
                    {
                        chairGoal = chair;
                        setChair = true;
                        tempDist = currDist;
                        freeChairs = true;
                    }
                    else if (!chair.occupied && !setChair)
                    {
                        chairGoal = chair; 
                        freeChairs = true;
                    }
                }
                if (!freeChairs) //if all chairs are occupied, make the goal to be leave the mall
                {
                    goal = "right";

                    if (transform.position.z < maxZ && transform.position.z > minZ) desiredVelocity = new Vector3((maxX + 2) - transform.position.x, velocity.y, velocity.z);
                    else
                    {
                        desiredVelocity = new Vector3((maxX + 2) - transform.position.x, velocity.y, Random.Range(minZ, maxZ) - transform.position.z); //in case the player is in the shop(not necessarily) 
                    }

                    seekVelocity = desiredVelocity - velocity;
                    setChair = true;
                }
                else if (!setChair)
                {
                    //wait till one is free?
                    Debug.Log("HEY PROBLEM OVER HERE");
                }
                setChairVelocity();

                string s = "";
                foreach(BoardManager.Chair chair in chairs)
                {
                    s += chair.occupied;
                }
                Debug.Log(s);

            }
        }
        else
        {
            Debug.Log("goal?: "+goal);
        }

        normalizeVelocities();
    }

    void setChairVelocity()
    {
        //calculate seek velocity
        desiredVelocity = chairGoal.offsetPos - transform.position; //offset seek velocity
        desiredVelocity.y = velocity.y;
        seekVelocity = desiredVelocity - velocity;
    }

    void setShopVelocity()
    {
        //calculate seek velocity
        desiredVelocity = shopGoal.transform.position - transform.position;
        desiredVelocity.y = velocity.y; //make sure velocity.y = 0 always
        desiredVelocity.z += shopOffset;
        seekVelocity = desiredVelocity - velocity;
    }
   

    public static void Shuffle<Chair>(List<Chair> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n);
            Chair value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    bool arrivedAtDestination(Vector3 destination)
    {
        Vector3 roundedPosition = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        Vector3 roundedDestination = new Vector3(Mathf.Round(destination.x), Mathf.Round(destination.y), Mathf.Round(destination.z));
       // Debug.Log(Vector3.Distance(roundedPosition, roundedDestination));
       // Debug.Log("roundedDestination: " + roundedDestination);
        //Debug.Log("roundedPosition: " + roundedPosition);
        //Debug.Log("shopGoalPosition: " + shopGoal.transform.position);
        //Debug.Log("shopGoalOffset: " + shopOffset);
      // Debug.Log("shopGoal bounds: " + shopGoal.GetComponent<Collider>().bounds.size.z);
        if (Vector3.Distance(roundedPosition, roundedDestination) <= 1) return true;
        return false;
    }

    bool arrivedAtShop()
    {
        Vector3 shopDestination = new Vector3(shopGoal.transform.position.x, transform.position.y, shopGoal.transform.position.z + shopOffset);
        return arrivedAtDestination(shopDestination);
    }

    bool arrivedAtChair()
    {
        //did it hit the chair collider?
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1);
        for(int i = 0; i<hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.GetInstanceID() == chairGoal.obj.GetInstanceID()) return true;
        }
        return false;
    }

    public bool ignoreObstacle(GameObject obstacle)
    {
        if (goal == "shop")
        {
            if (obstacle.GetInstanceID() == shopGoal.GetInstanceID()) return true;
            return false;
        }
        if (goal == "chair")
        {
            if (obstacle == chairGoal.obj && arrivedAtChair()) return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(goal == "chair")
        {
            if (collision.gameObject.GetInstanceID() == chairGoal.obj.GetInstanceID())
            {
                Physics.IgnoreCollision(chairGoal.obj.GetComponent<Collider>(), this.gameObject.GetComponent<Collider>());
            }
        }
        if (goal == "shop")
        {
            if (collision.gameObject.GetInstanceID() == shopGoal.GetInstanceID())
            {
                Physics.IgnoreCollision(shopGoal.GetComponent<Collider>(), this.gameObject.GetComponent<Collider>());
            }
        }
        if(collision.gameObject.name == "rightshop"){
            Destroy(this.gameObject);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("advertisement"))
        {
            freeze = true;
            this.gameObject.GetComponent<MeshRenderer>().material = flyeredMaterial;
            flyered = true;
            flyeredTimer = 0;
            freezeTimer = 0;
            
            Destroy(other.gameObject);
            
        }
        if (other.gameObject.name == "rightshopTrigger")
        {
            Destroy(this.gameObject);
        }
    }

    public void ObstacleAvoidance()
    {
        fleeVelocity.Set(0, 0, 0);
        int layerMask = ~(11 << 8);
        RaycastHit hit;
        Vector3 hitPoint = new Vector3();

        Vector3 leftBound = transform.position + transform.forward.normalized * (agentBounds.z / 2);
        Vector3 rightBound = transform.position - transform.forward.normalized * (agentBounds.z / 2);

        GameObject obstacle = null;
        GameObject currObstacle;
        float minDistanceToCollider = Mathf.Infinity;


        Debug.DrawRay(transform.position, seekVelocity, Color.green, 3f);
        Debug.DrawRay(leftBound, seekVelocity, Color.blue, 3f);
        Debug.DrawRay(rightBound, seekVelocity, Color.red, 3f);

        Vector3 agentToCollision;

        if (Physics.Raycast(leftBound, seekVelocity, out hit, rayDistance, layerMask))
        {
            agentToCollision = hit.point - transform.position;
            agentToCollision.y = 0;

            currObstacle = hit.collider.gameObject;

            if (!ignoreObstacle(currObstacle) && agentToCollision.magnitude < minDistanceToCollider)
            {
                minDistanceToCollider = agentToCollision.magnitude;
                obstacle = currObstacle;
                hitPoint = hit.point;
                currentlyAvoiding = "left" + currObstacle.name;
            }

        }
        if (Physics.Raycast(rightBound, seekVelocity, out hit, rayDistance, layerMask))
        {

            agentToCollision = hit.point - transform.position;
            agentToCollision.y = 0;

            currObstacle = hit.collider.gameObject;

            if (!ignoreObstacle(currObstacle) && agentToCollision.magnitude < minDistanceToCollider)
            {
                minDistanceToCollider = agentToCollision.magnitude;
                obstacle = currObstacle;
                hitPoint = hit.point;
                currentlyAvoiding = "right" + currObstacle.name;
            }

        }
        if (Physics.Raycast(transform.position, seekVelocity, out hit, rayDistance, layerMask))
        {

            agentToCollision = hit.point - transform.position;
            agentToCollision.y = 0;

            currObstacle = hit.collider.gameObject;

            if (!ignoreObstacle(currObstacle) && (agentToCollision.magnitude < minDistanceToCollider || currObstacle.GetInstanceID() == obstacle.GetInstanceID()))
            {
                minDistanceToCollider = agentToCollision.magnitude;
                obstacle = currObstacle;
                hitPoint = hit.point;
                currentlyAvoiding = "middle" + currObstacle.name;
            }

        }
        if (obstacle == null) return; //no collision was found

        Vector3 difference = (obstacle.transform.position - transform.position).normalized - (seekVelocity).normalized;
        differenceMag = difference.magnitude;
        if (difference.magnitude < 0.5f)
        {
            //shopper heading towards the center of the obstacle, turn right or left                                                                                                                                                
            Vector3 vv = new Vector3(seekVelocity.x, 0, seekVelocity.z);
            Vector3 newV = Vector3.Cross(vv, new Vector3(0, 1, 0));
            fleeVelocity.Set(newV.x, 0, newV.z);

        }
        else //shopper must head away from obstacle center
        {
            //Vector Projection
            Vector3 v = obstacle.transform.position - transform.position;
            v.y = 0;
            v.Normalize();
            Vector3 u = seekVelocity;
            u.Normalize();
            Vector3 uu = Vector3.Dot(u, v) * u;
            Vector3 w = -uu + v;
            fleeVelocity.Set(-w.x, 0, -w.z);
            this.u = u;
            this.v = v;
            this.uu = uu;
            this.w = -w;

        }

        float m = minDistanceToCollider; //so that it's always above 1
        m /= 3;
        fleeClamp = Mathf.Lerp(1, 0, m);
        fleeClamp *= fleeClampScalar;

        if (Physics.Raycast(transform.position, fleeVelocity, out hit, 1, layerMask)) //in case my flee velocity will make me collide with an obstacle that is closer than
                                                                                      //the obstacle i would collide with using my seek velocity
        {
            agentToCollision = hit.point - transform.position;
            agentToCollision.y = 0;
            obstacle = hit.collider.gameObject;

            if (agentToCollision.magnitude < minDistanceToCollider && !ignoreObstacle(obstacle))
            {
                fleeClamp /= 2;
            }
        }
        normalizeVelocities();
        Debug.DrawRay(transform.position, seekVelocity, Color.green, 3f);
        Debug.DrawRay(leftBound, seekVelocity, Color.blue, 3f);
        Debug.DrawRay(rightBound, seekVelocity, Color.red, 3f);

        currentlyAvoiding += obstacle.name + obstacle.GetInstanceID();

    }

    public void normalizeVelocities()
    {
        desiredVelocity.Normalize();
        seekVelocity.Normalize();
        fleeVelocity.Normalize();
    }
}


