using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvertiserManager2 : SteeringAgent
{
    public bool following = false;
    public float neighbourhoodRadius; //radius for collision detection for objects other than advertiser
    public float repulsionRadius; //if other advertisers are within this radius, then this gameobject will have a repulsion velocity

    public GameObject advertisement;

    public Vector3 wanderVelocity = new Vector3();
    public Vector3 followVelocity = new Vector3();
    private Vector3 pasttotalVelocity = new Vector3();
    public Vector3 separationVelocity = new Vector3();
    public Vector3 repulsionVelocity = new Vector3();
    public Vector3 circleCenter = new Vector3();
    public Vector3 displacement = new Vector3(1, 0, 0);

    //Wandering around non-foodcourt, non-shops area
    public float wanderAngle;
    private float pastWanderAngle = 0;
    public float circleDistance;
    public float circleRadius;

    public float wanderTimer = 2;

    public float behindDistance = 1;
    public float aheadDistance = 1;

    private bool shouldDropAd = false;
    private int numSalePitches = 0;

    private Vector3 behind = new Vector3(); //this keeps track of the position behind the shopper to follow
    private Vector3 ahead = new Vector3(); //this keeps track of the position ahead of the shopper to avoid

    [HideInInspector] public GameObject leader;

    private float adDropTimer;
    public float followTimer;
    public float withinRadiusTimer;
    public float detectShopperTimer;

    public static float k, p, r, s;

    public float transitionSpeed;

    void Start()
    {
        speed = 12;
        circleDistance = 2f;
        circleRadius = 1f;
        wanderAngle = Random.Range(-180, 180);
        rayDistance = 3f;
        transitionSpeed = 25;
        repulsionRadius = 4;
        neighbourhoodRadius = 2;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        separationVelocity.Set(0, 0, 0);
        emergencySeparationVelocity.Set(0, 0, 0);
        repulsionVelocity.Set(0, 0, 0);
        followVelocity.Set(0, 0, 0);

        wanderTimer += Time.deltaTime;

        checkForShoppers();

        if(numSalePitches >= 3)
        {
            //despawn
           // Debug.Log("destroying~");
            Vector3 tryPos = new Vector3(Random.Range(-13, 14), 0, Random.Range(-8.5f, 1.3f));
            var checkResult = Physics.OverlapSphere(tryPos, 0.4f);
            while (checkResult.Length != 0)
            {
                tryPos = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minZ, maxZ));
                checkResult = Physics.OverlapSphere(tryPos, 0.4f);
            }
            GameObject g = Instantiate(this.gameObject, new Vector3(tryPos.x, 0.9f, tryPos.z), Quaternion.identity);
            g.transform.Find("pitches").gameObject.GetComponent<TextMesh>().text = "0";
            Destroy(this.gameObject);
        }

        if (following)
        {
            if(leader == null) //if leader despawned
            {
                following = false;
            }
            else
            {
                withinRadiusTimer += Time.deltaTime;
                followTimer += Time.deltaTime;
                if (Vector3.Distance(transform.position, leader.transform.position) > r)
                {
                    withinRadiusTimer = 0;
                }
                else if (withinRadiusTimer > 4)
                {
                    numSalePitches++;
                    Debug.Log("hello?");
                    transform.Find("pitches").gameObject.GetComponent<TextMesh>().text = numSalePitches.ToString();
                    //other stuff
                    following = false;
                }

                if (followTimer > 5)
                {
                    following = false;
                }
                else if(following)
                {
                    Separation();
                    FollowTheLeader();
                }
            }

           
        }
        if(!following)
        {
            adDropTimer += Time.deltaTime;
            if (adDropTimer > k && !shouldDropAd)
            {
                if (Random.Range(0, 1) < p)
                {
                    //drop advertisement
                    shouldDropAd = true;
                }
            }
            Separation();
            IHateMyself();
            Wander();
        }
    
        updatePos();
    }

    void updatePos()
    {
        normalizeV();

        totalVelocity = separationVelocity * 0.25f + repulsionVelocity * 2 + wanderVelocity + followVelocity;

        if(following) totalVelocity = separationVelocity * 0.25f + repulsionVelocity + followVelocity;

        if (checkEmergencySeparation()) totalVelocity = separationVelocity + emergencySeparationVelocity + followVelocity;

       // pasttotalVelocity = totalVelocity;
        totalVelocity.Normalize();
        //pasttotalVelocity.Normalize();

        Vector3 newPos = transform.position + new Vector3(totalVelocity.x * Time.deltaTime * speed, 0, totalVelocity.z * Time.deltaTime * speed);
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * transitionSpeed);
    }

   
    void Wander()
    {
      
       // circleCenter.Set(pasttotalVelocity.x, pasttotalVelocity.y, pasttotalVelocity.z);
        circleCenter.Set(totalVelocity.x, totalVelocity.y, totalVelocity.z);

        circleCenter.Normalize();
        circleCenter *= circleDistance;

        if(wanderTimer > 0.5f)
        {
            wanderTimer = 0;
            wanderAngle = Random.Range(-180, 180);
        }
        
        displacement.Set(1, 0, 0);
        
        displacement.Normalize();
        displacement *= circleRadius;

        pastWanderAngle = wanderAngle; //wait a second..

        displacement = Quaternion.Euler(0, Mathf.Lerp(pastWanderAngle, wanderAngle, Time.deltaTime/5), 0) * displacement;
       
        wanderVelocity.Set(displacement.x + circleCenter.x, 0, displacement.z + circleCenter.z);
        wanderVelocity.Normalize();

    }

    void OnDrawGizmos() //to help debug
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(circleCenter+transform.position, circleRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, neighbourhoodRadius);
        // Debug.Log("hey");
        // Draws a 5 unit long red line in front of the object
        Gizmos.color = Color.red;
      //  Vector3 direction = transform.TransformDirection(wanderVelocity) * 5;

        Vector3 direction = wanderVelocity;
        Gizmos.DrawRay(transform.position, direction);

        Gizmos.color = Color.blue;
        //direction = transform.TransformDirection(circleCenter+transform.position);
        direction = circleCenter;
        Gizmos.DrawRay(transform.position, direction);


        Gizmos.color = Color.green;
        //rection = transform.TransformDirection(displacement + transform.position + circleCenter);
        direction = displacement + circleCenter;
        Gizmos.DrawRay(transform.position, direction);

        if (following)
        {
            Gizmos.DrawSphere(leader.transform.position, 0.35f);

            Gizmos.DrawRay(leader.transform.position, leader.GetComponent<ShopperManager>().totalVelocity);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(behind, 0.15f);
            Gizmos.DrawSphere(ahead, 0.15f);
        }
    }

    bool gotLeft, gotRight, gotTop, gotBottom;

    public Vector3 emergencySeparationVelocity;

    void Separation()
    {
        float scale = 1;
        gotLeft = gotRight = gotTop = gotBottom = false;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, neighbourhoodRadius);

        string s = "";
        for (int i = 0; i<hitColliders.Length; i++)
        {
            s += hitColliders[i].gameObject.name + ", ";
        }//Debug.Log(s);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            scale = 1;
            if (hitColliders[i].gameObject.name.Equals("advertiserhead") || hitColliders[i].gameObject.CompareTag("advertisement") || hitColliders[i].gameObject.GetInstanceID() == gameObject.GetInstanceID()
                || (hitColliders[i].gameObject.name.Equals("shopperhead") || (hitColliders[i].gameObject.CompareTag("shopper")))) continue;
            if (hitColliders[i].gameObject.CompareTag("advertiser"))
            {
                continue;
            }
            if (hitColliders[i].gameObject.CompareTag("shop") || hitColliders[i].gameObject.name.Equals("leftshop") || hitColliders[i].gameObject.name.Equals("rightshop"))
            {
                if (hitColliders[i].gameObject.name.Equals("leftshop") && !gotLeft)
                {
                    if (hitColliders[i].gameObject.transform.position.x + hitColliders[i].bounds.size.x < transform.position.x)
                    {
                        separationVelocity += new Vector3(hitColliders[i].gameObject.transform.position.x + hitColliders[i].bounds.size.x
                            - transform.position.x, 0, 0); //DO I NEED THIS?
                    }
                    else separationVelocity += new Vector3(-1, 0, 0);
                    separationVelocity += new Vector3(-1, 0, 0);
                    emergencySeparationVelocity += new Vector3(-1, 0, 0);
                    gotLeft = true;
                }
                else if (hitColliders[i].gameObject.name.Equals("rightshop") && !gotRight)
                {
                    if (hitColliders[i].gameObject.transform.position.x - hitColliders[i].bounds.size.x > transform.position.x)
                    {
                        separationVelocity += new Vector3(hitColliders[i].gameObject.transform.position.x - hitColliders[i].bounds.size.x
                        - transform.position.x, 0, 0);
                    }
                    else separationVelocity += new Vector3(1, 0, 0);

                    separationVelocity += new Vector3(1, 0, 0); 
                    emergencySeparationVelocity += new Vector3(1, 0, 0);
                    gotRight = true;

                }
                else if (hitColliders[i].gameObject.transform.parent.gameObject.name.Equals("topshop") && !gotTop)
                {

                    if (hitColliders[i].gameObject.transform.position.z - hitColliders[i].bounds.size.z > transform.position.z)
                    {
                        separationVelocity += new Vector3(0, 0, hitColliders[i].gameObject.transform.position.z - hitColliders[i].bounds.size.z
                                             - transform.position.z);
                    }
                    else separationVelocity += new Vector3(0, 0, 1);
                    emergencySeparationVelocity += new Vector3(0, 0, 1);
                    separationVelocity += new Vector3(0, 0, 1);
                    gotTop = true;
                    if (shouldDropAd)
                    {
                        shouldDropAd = false;
                        adDropTimer = 0;
                        Instantiate(advertisement, new Vector3(transform.position.x, 0, transform.position.z), Quaternion.Euler(0, 180, 0));
                    }
                }
                else if (hitColliders[i].gameObject.transform.parent.gameObject.name.Equals("bottomshop") && !gotBottom)
                {

                    if (hitColliders[i].gameObject.transform.position.z + hitColliders[i].bounds.size.z < transform.position.z)
                    {
                        separationVelocity += new Vector3(0, 0, hitColliders[i].gameObject.transform.position.z + hitColliders[i].bounds.size.z
                        - transform.position.z);
                    }
                    else separationVelocity += new Vector3(0, 0, -1);
                    separationVelocity += new Vector3(0, 0, -1); 
                    emergencySeparationVelocity += new Vector3(0, 0, -1);
                    gotBottom = true;
                    if (shouldDropAd)
                    {
                        shouldDropAd = false;
                        adDropTimer = 0;
                        Instantiate(advertisement, new Vector3(transform.position.x, 0, transform.position.z), Quaternion.Euler(0, 180, 0));
                    }
                    
                }
                continue;
            }
            separationVelocity += (hitColliders[i].gameObject.transform.position - transform.position)*scale;
        }
        separationVelocity.y = 0;
        separationVelocity.Normalize();
        separationVelocity *= -1;

        emergencySeparationVelocity.y = 0;
        emergencySeparationVelocity.Normalize();
        emergencySeparationVelocity *= -1;
    }

    bool checkEmergencySeparation()
    {
        if (transform.position.z >= 1.5 || transform.position.z <= -8.7 || transform.position.x <= -14.7 || transform.position.x >= 15.4) return true;
        return false;
    }

    void IHateMyself()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, repulsionRadius);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.CompareTag("advertiser"))
            {
                repulsionVelocity += (hitColliders[i].gameObject.transform.position - transform.position);
            }
            if (hitColliders[i].gameObject.name.Equals("topshop")&& transform.position.z > (hitColliders[i].gameObject.transform.position.z- hitColliders[i].bounds.size.z/2f))
            {
                repulsionVelocity += new Vector3(0, 0, -1);
            }
            if (hitColliders[i].gameObject.name.Equals("bottomshop") && transform.position.z < (hitColliders[i].gameObject.transform.position.z + hitColliders[i].bounds.size.z/2f))
            {
                repulsionVelocity += new Vector3(0, 0, 1);
            }
        }
        repulsionVelocity.y = 0;
        repulsionVelocity.Normalize();
        repulsionVelocity *= -1;

    }

    void FollowTheLeader()
    {
        Vector3 leaderVelocity = (leader.GetComponent<ShopperManager>().totalVelocity).normalized;
        if (r < behindDistance) behindDistance = r;
        behind = leader.transform.position + leaderVelocity *-1*behindDistance;
        followVelocity = (behind - transform.position) - (totalVelocity);

        ahead = leader.transform.position + leaderVelocity * aheadDistance;
        if(Vector3.Distance(ahead, transform.position) < 1.5f) //if the advertiser is in the leaders way, then move
        {
            Vector3 avoidVelocity = transform.position - ahead;
            followVelocity += avoidVelocity;
        }

        followVelocity.Normalize();
    }

    void checkForShoppers()
    {

        if (following) return;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, s);
        for(int i = 0; i<hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.CompareTag("shopper"))
            {
                GameObject shopper = hitColliders[i].gameObject;
                if (shopper.GetComponent<ShopperManager>().flyered)
                {
                    if (Vector3.Distance(transform.position, shopper.transform.position) <= s)
                    {
                        following = true;
                        leader = shopper;
                        followTimer = 0;
                        withinRadiusTimer = 0;
                    }
                }
            }
        }
    }



    public void normalizeV()
    {
        followVelocity.Normalize();
        repulsionVelocity.Normalize();
        wanderVelocity.Normalize();
        separationVelocity.Normalize();
        emergencySeparationVelocity.Normalize();
    }


}
