using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C };
    public Type enemyType;

    public int maxHealth;
    public int curHealth;
    public Transform target;
    public bool isChase;
    public BoxCollider meleeArea;
    public bool isAttack;

    public GameObject bullet;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    Vector3 reactVec;
    NavMeshAgent nav;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2f);


        meleeArea.enabled = false;
    }

    void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);

            nav.isStopped = !isChase;
        }

    }

    void Targeting()
    {
        float targetRadius = 0;
        float targetRange = 0;

        switch (enemyType)
        {
            case Type.A:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;

            case Type.B:
                targetRadius = 1;
                targetRange = 12f;
                break;

            case Type.C:
                targetRadius = 0.5f;
                targetRange = 20f;
                break;
        }

        RaycastHit[] rayHits =
         Physics.SphereCastAll(transform.position,
          targetRadius,
           transform.forward,
            targetRange,
             LayerMask.GetMask("Player"));

        if (rayHits.Length > 0 && !isAttack)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {

        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;

            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 30, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;

            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 40;

                yield return new WaitForSeconds(2f);
                break;

        }




        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);



    }

    private void FixedUpdate()
    {
        FreezeVelocity();
        Targeting();
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.angularVelocity = Vector3.zero;
            rigid.velocity = Vector3.zero;
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            reactVec = transform.position - other.transform.position;


            Debug.Log("근접공격에 맞았습니다." + curHealth);
            StartCoroutine(OnDamage(reactVec, false));
        }

        if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);


            Debug.Log("원거리공격에 맞았습니다. " + curHealth);
            StartCoroutine(OnDamage(reactVec, false));

        }
    }

    public void HitGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        mat.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            mat.color = Color.white;

        }
        else
        {
            isChase = false;
            nav.enabled = false;

            mat.color = Color.gray;
            gameObject.layer = 12;

            anim.SetTrigger("doDie");

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }
            Destroy(gameObject, 4);
        }

    }
}
