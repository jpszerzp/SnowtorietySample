using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectileController : MonoBehaviour {

    bool charged;
    private float lifeTime = 10.0f;
    private Color renderColor;
    Rigidbody2D rb2d;

    public GameObject snowballDeath;
    public GameObject iceballDeath;
    public GameObject iceballTrail;
    public GameObject snowballTrail;
    GameObject newIceballTrail;
    GameObject newSnowballTrail;

    public bool Charged
    {
        get { return charged; }
        set { charged = value; }
    }
    public Color RenderColor
    {
        get { return renderColor; }
        set { renderColor = value; }
    }

    // Use this for initialization
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (charged)
        {
            if(iceballTrail != null)
            {
                newIceballTrail = GameObject.Instantiate(iceballTrail, gameObject.transform.position, Quaternion.identity);
                newIceballTrail.name = "Spawned_" + iceballTrail.name;
            }
        }
        else
        {
            if(snowballTrail != null)
            {
                newSnowballTrail = GameObject.Instantiate(snowballTrail, gameObject.transform.position, Quaternion.identity);
                newSnowballTrail.name = "Spawned_" + snowballTrail.name;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0.0f)
        {
            Destroy(gameObject);
        }
        if (charged)
        {
            if (iceballTrail != null)
            {
                newIceballTrail.transform.position = gameObject.transform.position;
                if (lifeTime <= 0.0f)
                {
                    Destroy(newIceballTrail);
                }
            }
        }
        else
        {
            if (snowballTrail != null)
            {
                newSnowballTrail.transform.position = gameObject.transform.position;
                if (lifeTime <= 0.0f)
                {
                    Destroy(newSnowballTrail);
                }
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Wall" || 
            collision.gameObject.tag == "Elf" || /*collision.gameObject.tag == "Breakable" ||*/ 
            collision.gameObject.tag == "Present" || collision.gameObject.tag == "SnowPile")
        {
            PlayHitSnowOrIce(collision);
            UnitDestroyIceOrSnow();
        }
        else if (collision.gameObject.tag == "Breakable")
        {
            if (!charged)
            {
                PlayHitSnowOrIce(collision);
                UnitDestroyIceOrSnow();
            }
        }
    }

  
    private void UnitDestroyIceOrSnow()
    {
        Destroy(gameObject);
        if (iceballTrail != null)
        {
            Destroy(newIceballTrail);
        }
        if (snowballTrail != null)
        {
            Destroy(newSnowballTrail);
        }
    }


    public void ReflectProjectile()
    {
        // get current velocity
        Vector2 vel = rb2d.velocity;
        //Vector2 normal = new Vector2(0, 1);
        //float normalScale = Vector2.Dot(vel, normal);
        //Vector2 vertical = normalScale * normal;
        //Vector2 reflectedVel = vel - (2 * vertical);
    }

    
    void PlayHitSnowOrIce(Collision2D collision)
    {
        if (charged)
        {
            if(iceballDeath == null)
            {
                return;
            }
            GameObject newIceballDeathObject = GameObject.Instantiate(iceballDeath, collision.contacts[0].point, Quaternion.identity);
            newIceballDeathObject.name = "Spawned_" + iceballDeath.name;
            GameObject.Destroy(newIceballDeathObject, 0.75f);
        }
        else
        {
            if(snowballDeath == null)
            {
                return;
            }
            GameObject newSnowballDeathObject = GameObject.Instantiate(snowballDeath, collision.contacts[0].point, Quaternion.identity);
            newSnowballDeathObject.name = "Spawned_" + snowballDeath.name;
            GameObject.Destroy(newSnowballDeathObject, 0.75f);
        }
    }
}
