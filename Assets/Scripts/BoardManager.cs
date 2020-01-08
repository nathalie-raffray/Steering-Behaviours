using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    public GameObject planter;
    public GameObject chair;
    public GameObject table;
    public GameObject shopper;
    public GameObject advertiser;
    private float passThroughWidth = 2.2f; //the minimum distance between two objects allowing a player to pass in between them
    private float maxDistBetweenChairs;

    public float k, p, r, s;

    private float planterRadius, shopperRadius;

    public static List<Chair> chairList;
  
    public class Chair
    {
        public GameObject obj;
        public bool occupied;

        public Vector3 offsetPos;

        public Chair(GameObject g, bool b)
        {
            obj = g;
            occupied = b;
            offsetPos = obj.transform.position + obj.transform.right.normalized * 0.65f;
        }
    }
    

    public float shopperSpawnRate; //time passed between the next spawn
    private float spawnTimer = 0;

    //bounding box for food court area
    private float minZ = -8f;
    private float maxZ = -1;
    private float minX = -11;
    private float maxX = 9;

    public float totalNumAdvertisers;

    //each cluster of a table and chairs has a bounding sphere around it
    private float boundingSphereRadius = 2f;
    List<Vector3> boundingSphereCenters = new List<Vector3>();

    private Vector3 vertical = new Vector3(0, 0, 0);
    private Vector3 horizontal = new Vector3(0, 90, 0);

    GameObject current;
    List<Collider> colliders = new List<Collider>();

    private int totalNumChairs; //will return between 7 and 10
    private int numTables;
    private int numPlanters;
    List<Vector3> possibleSphereCenters = new List<Vector3>();
    List<Vector3> possibleChairCenters = new List<Vector3>();
    List<Vector3> possiblePlanterCenters = new List<Vector3>();

    List<Vector3> boundingSphereCentersConst = new List<Vector3>();


    void Awake()
    {
        totalNumChairs = Random.Range(7, 10); //will return between 7 and 10
        numTables = Random.Range(3, 5); //will return between 3 and 4
        numPlanters = Random.Range(2, 6); //will return between 2 and 5

        Vector3 planterBounds = planter.GetComponent<BoxCollider>().bounds.size;
        planterRadius = Mathf.Max(planterBounds.x, planterBounds.z);

        Vector3 shopperBounds = shopper.GetComponent<Collider>().bounds.size;
        shopperRadius = Mathf.Max(planterBounds.x, planterBounds.z);

        float offset = (maxX - minX) / numTables;
        float currMinX, currMaxX;
        Vector3 temp;
        for (int i = 0; i<numTables; i++) //create the positions for where the tables will be situated
        {
            currMinX = minX + offset * i + boundingSphereRadius;
            currMaxX = minX + offset * (i+1) - boundingSphereRadius;

            temp = new Vector3(Random.Range(currMinX, currMaxX), 0, Random.Range(minZ + boundingSphereRadius, maxZ - boundingSphereRadius));
            boundingSphereCenters.Add(temp);
            boundingSphereCentersConst.Add(temp);
        }

        while(numTables != 0)
        {
            makeTableAndSurroundingChairs();
            numTables--;
        }
        makePlanters();

        GameObject[] tempChairs;
        tempChairs = GameObject.FindGameObjectsWithTag("chair");
        chairList = new List<Chair>();

        for (int i = 0; i<tempChairs.Length; i++)
        {
            chairList.Add(new Chair(tempChairs[i], false));
        }

        Instantiate(shopper, new Vector3(minX - 2, 0.32f, (minZ + maxZ)/2), Quaternion.identity);

        //Instantiate(shopper, new Vector3(minX - 2, 0.3f, Random.Range(minZ, maxZ)), Quaternion.identity);
        // Instantiate(shopper, new Vector3(minX - 2, 0.32f, Random.Range(minZ, maxZ)), Quaternion.identity);

        float advertisersMade = 0;
        createAdvertisers(advertisersMade);

        // Instantiate(advertiser, new Vector3(minX, 0.959f, Random.Range(minZ, maxZ)), Quaternion.identity);
        //Instantiate(advertiser, new Vector3(minX, 0.959f, Random.Range(minZ, maxZ)), Quaternion.identity);
        // Instantiate(advertiser, new Vector3(minX, 0.959f, Random.Range(minZ, maxZ)), Quaternion.identity);
        if (p > 1) p = 1;
        if (p < 0) p = 0;
        AdvertiserManager2.k = this.k;
        AdvertiserManager2.p = this.p;
        AdvertiserManager2.r = this.r;
        AdvertiserManager2.s = this.s;
        //create table with chairs around it and then make it
    }

    void createAdvertisers(float advertisersMade)
    {
        while (advertisersMade < totalNumAdvertisers)
        {
            Vector3 tryPos = new Vector3(Random.Range(-13, 14), 0, Random.Range(-8.5f, 1.3f));
            var checkResult = Physics.OverlapSphere(tryPos, 0.4f);
            while (checkResult.Length != 0)
            {
                tryPos = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minZ, maxZ));
                checkResult = Physics.OverlapSphere(tryPos, 0.4f);
            }
            GameObject g = Instantiate(advertiser, new Vector3(tryPos.x, 0.9f, tryPos.z), Quaternion.identity);
            g.transform.Find("pitches").gameObject.GetComponent<TextMesh>().text = "0";
            advertisersMade++;
        }
    }

    void Update()
    {
        GameObject[] currAdvertisers = GameObject.FindGameObjectsWithTag("advertiser");
        if(currAdvertisers.Length < totalNumAdvertisers)
        {
            createAdvertisers(currAdvertisers.Length);
        }else if(currAdvertisers.Length > totalNumAdvertisers)
        {
            for(int i = 0; i<currAdvertisers.Length && currAdvertisers.Length > totalNumAdvertisers; i++)
            {
                Destroy(currAdvertisers[i]);
            } 
        }
        AdvertiserManager2.k = this.k;
        AdvertiserManager2.p = this.p;
        AdvertiserManager2.r = this.r;
        AdvertiserManager2.s = this.s;

        spawnTimer += Time.deltaTime;
        if(spawnTimer > shopperSpawnRate)
        {
            Vector3 tryPos = new Vector3(minX - 2, 0.32f, Random.Range(minZ, maxZ));
            var checkResult = Physics.OverlapSphere(tryPos, shopperRadius);
            while (checkResult.Length != 0)
            {
                tryPos = new Vector3(minX - 2, 0.32f, Random.Range(minZ, maxZ));
                checkResult = Physics.OverlapSphere(tryPos, shopperRadius);
            }
            GameObject g = Instantiate(shopper, new Vector3(tryPos.x, 0.32f, tryPos.z), Quaternion.identity);
            g.GetComponent<ShopperManager>().speed = Random.Range(2, 3.5f);

           // Instantiate(shopper, new Vector3(minX - 2, 0.32f, Random.Range(minZ, maxZ)), Quaternion.identity);
            spawnTimer = 0;
        }
    }


    void makePlanters()
    {

        float currX = minX;
        int plantersMade = 0;
        int count = 0;
        int count2 = 0;
        while (numPlanters > plantersMade && count < 20) {
            count++;
            Vector3 tryPos = new Vector3(Random.Range(-11.35f, 11.15f), 0, Random.Range(-5.85f, 0.4f));
            var checkResult = Physics.OverlapSphere(tryPos, passThroughWidth+planterRadius);
            count2 = 0;
            while (checkResult.Length != 0 && count2 < 70)
            {
                tryPos = new Vector3(Random.Range(-11.35f, 11.15f), 0, Random.Range(-5.85f, 0.4f));
                checkResult = Physics.OverlapSphere(tryPos, passThroughWidth+planterRadius);
                count2++;
            }
            if(checkResult.Length == 0) Instantiate(planter, new Vector3(tryPos.x, 0, tryPos.z), Quaternion.identity);

            plantersMade++;
        }
    }

    

    void makeTableAndSurroundingChairs()
    {

        possibleChairCenters.Clear();

        if (boundingSphereCenters.Count == 0) return;

        Vector3 center = boundingSphereCenters[0];
        GameObject currTable;

        if ((int)Random.Range(0, 2) == 0) currTable = Instantiate(table, center, Quaternion.Euler(vertical));
        else currTable = Instantiate(table, center, Quaternion.Euler(horizontal));

        boundingSphereCenters.RemoveAt(0);

        int numChairs = Random.Range(1, 5);
        if (numTables == 1 || numChairs>totalNumChairs) numChairs = totalNumChairs;

        totalNumChairs -= numChairs;

        Vector2 scale = new Vector2((boundingSphereRadius), (boundingSphereRadius));

        Vector2 unit = Random.insideUnitCircle.normalized;
        Vector3 pointOnCirc = new Vector3(center.x + unit.x*boundingSphereRadius, 0, center.z + unit.y * boundingSphereRadius);

        Vector3 test = pointOnCirc - center;
        

        if (numChairs <= 0) return;
        float theta = 360 / numChairs;

        //In general, suppose that you are rotating about the origin clockwise through an angle θ. Then the point(s, t) ends up at(u, v) where
        //u = scosθ + tsinθandv =−ssinθ + tcosθ.

        Vector3 curr = pointOnCirc;

        int chairsDone = numChairs;

        while(chairsDone > 0)
        {
            //going clockwise around the circle
            Vector3 v = (pointOnCirc - center).normalized;
            float x = (v.x) * Mathf.Cos(theta * (Mathf.PI/180)) + (v.z) * Mathf.Sin(theta * (Mathf.PI / 180));
            float z = -(v.x) * Mathf.Sin(theta * (Mathf.PI / 180)) + (v.z) * Mathf.Cos(theta * (Mathf.PI / 180));

            possibleChairCenters.Add(new Vector3(x * boundingSphereRadius + center.x, 0, z * boundingSphereRadius + center.z));

            curr.x = x;
            curr.z = z;
            theta += 360 / numChairs;
            chairsDone--;
        }

        Vector3 radius;
        GameObject currChair;
        foreach(Vector3 chairCenter in possibleChairCenters)
        {
            radius = center - chairCenter;
            radius.Normalize();

            theta = Mathf.Acos(Vector3.Dot(chair.transform.right.normalized, radius.normalized)) * 180 / Mathf.PI; //to make the chairs face the table

            if (chairCenter.z < center.z) theta = 360 - theta;

            currChair = Instantiate(chair, chairCenter, Quaternion.Euler(0, theta, 0));     
        }
    }
}
