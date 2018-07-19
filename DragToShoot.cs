using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragToShoot : MonoBehaviour {

    bool charged;

    float limitRadii;
    float velocityScale;
    //float normalBallVelocityScale;
    float animRadii;
    int lineSegNum;
    bool onHit;

    Vector3 velocity;
    float updatedSpd;
    float updatedAng;

    float g = 9.81f;
    float maxX;
    float maxY;

    Vector3[] points;
    LineRenderer lr;
    public GameObject groundRef;
    public GameObject snowBall;
    public GameObject iceBall;

    public Color snowGradient1_Color = new Vector4(1, 1,1, 1);
    public Color snowGradient2_Color = new Vector4(1, 1,1, 0.25f);
    public Color iceGradient1_Color = new Vector4(0.25f, 0.8f, 1, 1);
    public Color iceGradient2_Color = new Vector4(0.25f, 0.8f, 1, 0.25f);

    public LayerMask lineBlock;

    public bool Charged
    {
        get { return charged; }
        set { charged = value; }
    }

    // Use this for initialization
    void Start () {
        charged = false;
        limitRadii = 3.5f;
        animRadii = 0.5f;
        velocityScale = 8.57f;
        lineSegNum = 50;
        onHit = false;
        maxY = CalculateHeightToGround();

        lr = GetComponent<LineRenderer>();
        points = new Vector3[lineSegNum+ 1];
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 adjustedClick = Camera.main.ScreenToWorldPoint(AdjustClickToCam());
            float distance = Vector3.Distance(adjustedClick, transform.position);

            // if click is out of range, return
            if (distance > limitRadii)
            {
                return;
            }

            onHit = true;
        }

        // update velocity based current drag point and start drag point (vector subtraction)
        else if (Input.GetMouseButton(0) && onHit)
        {
            Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(AdjustClickToCam());
            velocity = (transform.position - currentMousePos) * velocityScale;

            Flip();

            float distance = Vector3.Distance(transform.position, currentMousePos);
            if (distance > limitRadii)
            {
                velocity = velocity * ((limitRadii * velocityScale) / velocity.magnitude);
            }

            if (!charged)
            {
                updatedSpd = velocity.magnitude;
                updatedAng = Mathf.Rad2Deg * Mathf.Atan2(velocity.y, velocity.x);
                CalculateTrajectory();
                DrawNormalTrajectory(points);
            }
            else
            {
                // charged shot points update
                Vector3[] chargedTrajec = new Vector3[2];
                chargedTrajec[0] = transform.position;
                chargedTrajec[1] = transform.position + (velocity / velocity.magnitude) * 38.0f;

                RaycastHit2D hit = Physics2D.Raycast(chargedTrajec[0], -velocity, lineBlock);
                //print(hit.fraction);
                if (hit.fraction != 0)
                {
                    chargedTrajec[1] = hit.point;
                }

                DrawChargedTrajectory(chargedTrajec);
            }

            // update animation
            if (distance > animRadii)
            {
                Animator snowmanAnim = transform.gameObject.GetComponent<Animator>();
                snowmanAnim.SetTrigger("startLoad");
            }
        }

        // spawn projectile with given velocity when mouse released
        else if (Input.GetMouseButtonUp(0) && onHit)
        {
            GameObject projectileClone;
            if (!charged)
            {
                projectileClone = GameObject.Instantiate(snowBall, transform.position, Quaternion.identity) as GameObject;
                projectileClone.GetComponent<ProjectileController>().Charged = false;
            }
            else
            {
                projectileClone = GameObject.Instantiate(iceBall, transform.position, Quaternion.identity) as GameObject;
                projectileClone.GetComponent<ProjectileController>().Charged = true;
                projectileClone.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
                velocity = 43.0f * (velocity / velocity.magnitude);
            }
            ProjectileController proj = projectileClone.GetComponent<ProjectileController>();
            Rigidbody2D rb2d = projectileClone.GetComponent<Rigidbody2D>();

            rb2d.velocity = velocity;
            proj.GetComponent<AudioSource>().Play();

            Animator snowmanAnim = transform.gameObject.GetComponent<Animator>();
            snowmanAnim.SetTrigger("startThrow");

            ClearTrajectory(points);
        }
    }

    Vector3 AdjustClickToCam()
    {
        Vector3 rawPos = Input.mousePosition;
        rawPos.z = -Camera.main.transform.position.z;

        return rawPos;
    }

    Vector3 AdjustTouchToCam(Touch touch)
    {
        Vector3 rawPos = touch.position;
        rawPos.z = -Camera.main.transform.position.z;
        return rawPos;
    }

    void DrawNormalTrajectory(Vector3[] points)
    {
        lr.positionCount = lineSegNum + 1;
        lr.SetPositions(points);

        lr.startColor = snowGradient1_Color;//Color.green;
        lr.endColor = snowGradient2_Color;//Color.green;
    }

    void DrawChargedTrajectory(Vector3[] points)
    {
        lr.positionCount = 2;
        lr.SetPositions(points);

        lr.startColor = Color.cyan;//iceGradient1_Color;//Color.red;
        lr.endColor = Color.white;//iceGradient2_Color;//Color.red;
    }

    void ClearTrajectory(Vector3[] points)
    {
        lr.positionCount = 0;
        onHit = false;
    }

    public void ClearTrajecGeneral()
    {
        ClearTrajectory(points);
    }

    void CalculateTrajectory()
    {
        bool blocked = false;

        // confirm time to reach verticalDisplacement (verticalDisplacement < 0)
        float time = GetVertTime(maxY);

        // confirm horizontal displacement during that period
        maxX = time * updatedSpd * Mathf.Cos(Mathf.Deg2Rad * updatedAng);

        // max dis confirmed; use linesegments to plot the route
        for (int i = 0; i < lineSegNum + 1; ++i)
        {
            if (!blocked)
            {
                float percent = (float)i / (float)lineSegNum;
                float xDisplacement = percent * maxX;
                float yDisplacement = SetVerticalDisplacement(xDisplacement, updatedSpd, updatedAng);
                points[i] = new Vector3(xDisplacement + transform.position.x, yDisplacement + transform.position.y, 0.0f);
            }
            else
            {
                points[i] = points[i - 1];
            }

            if (i != 0)
            {
                Vector3 dir = points[i] - points[i - 1];
                float dist = dir.magnitude;
                RaycastHit2D hit = Physics2D.Raycast(points[i - 1], dir, dist, lineBlock);

                if (hit.fraction != 0)              // hit
                {
                    blocked = true;
                    points[i] = points[i - 1];
                }
            }
        }
    }

    float GetVertTime(float vertDisplacement)
    {
        float radianAngle = Mathf.Deg2Rad * updatedAng;
        float delta = updatedSpd * updatedSpd * Mathf.Sin(radianAngle) * Mathf.Sin(radianAngle) - 2 * g * vertDisplacement;
        float time = (updatedSpd * Mathf.Sin(radianAngle) + Mathf.Sqrt(delta)) / g;
        return time;
    }

    float CalculateHeightToGround()
    {
        return groundRef.transform.position.y - transform.position.y;
    }

    float SetVerticalDisplacement(float x, float speed, float angle)
    {
        float radianAngle = Mathf.Deg2Rad * angle;

        float nonZero = 2.0f * speed * speed * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle);

        if (Mathf.Abs(nonZero) < 0.0001f)
        {
            return 0.0f;
        }
        else
        {
            return (Mathf.Tan(radianAngle) * x - (g / nonZero) * x * x);
        }
    }

    void Flip()
    {
        if (velocity.x < 0.0f)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = -1;
            transform.localScale = newScale;
        }
        else
        {
            Vector3 newScale = transform.localScale;
            newScale.x = 1;
            transform.localScale = newScale;
        }
    }
}
