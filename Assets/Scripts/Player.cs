using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject[] weapon;
    public bool[] hasWeapon;
    public GameObject[] grenade;
    public int hasGrenade;
    public GameObject grenadeObj;

    public int ammo;
    public int coin;
    public int health;


    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenade;

    public Camera followCamera;


    public float speed;
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    bool isJump;
    bool isDodge;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool isSwap;
    bool fDown;
    bool gDown;
    bool isFireReady = true;
    bool rDown;
    bool isReload = false;
    bool isBorder;
    bool isDamage;


    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;
    MeshRenderer[] meshes;

    Rigidbody rigid;

    GameObject nearObj;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshes = GetComponentsInChildren<MeshRenderer>();
    }


    void Update()
    {

        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Grenade();
        Reload();
        Dodge();
        Interaction();
        Swap();

    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");

    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        if (isSwap)
        {
            moveVec = Vector3.zero;
        }
        if (!isBorder)
        {
            if (wDown)
            {
                transform.position += moveVec * speed * 0.3f * Time.deltaTime;
            }
            else
            {
                transform.position += moveVec * speed * Time.deltaTime;
            }
        }



        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);

    }

    void Turn()
    {
        //player turn soft

        //if (moveVec != Vector3.zero)
        //{
        //    Vector3 relativePos = (transform.position + moveVec) - transform.position;
        //    Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        //    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 10);
        //}
        //else
        //{
        //    transform.LookAt(transform.position + moveVec);
        //}

        //player turn hard

        transform.LookAt(transform.position + moveVec);

        //player turn wiht mouse
        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }

        }

    }

    void Jump()
    {

        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * 10, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }

    }

    void Grenade()
    {
        if (hasGrenade == 0)
        {
            return;
        }

        if (gDown && !isReload && !isSwap)
        {


            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 15;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenade--;
                grenade[hasGrenade].SetActive(false);
            }


        }
    }

    void Attack()
    {
        if (equipWeapon == null)
        {
            return;
        }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap && !isReload)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
        {
            return;
        }

        if (equipWeapon.type == Weapon.Type.Melee)
        {
            return;
        }

        if (ammo == 0)
        {
            return;
        }

        if (rDown && !isJump && !isDodge && !isSwap && !isReload)
        {
            Debug.Log("재장전");
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        reAmmo -= equipWeapon.curAmmo;
        equipWeapon.curAmmo += reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Dodge()
    {

        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            speed *= 2;
            anim.SetTrigger("doDodge");
            dodgeVec = moveVec;
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }

    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }


    void Swap()
    {

        if (sDown1 && (!hasWeapon[0] || equipWeaponIndex == 0))
        {
            return;
        }
        if (sDown2 && (!hasWeapon[1] || equipWeaponIndex == 1))
        {
            return;
        }
        if (sDown3 && (!hasWeapon[2] || equipWeaponIndex == 2))
        {
            return;
        }


        int weaponIndex = -1;

        if (sDown1)
        {
            weaponIndex = 0;
        }
        if (sDown2)
        {
            weaponIndex = 1;
        }
        if (sDown3)
        {
            weaponIndex = 2;
        }

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {

            if (equipWeapon != null)
            {
                equipWeapon.gameObject.SetActive(false);

            }
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapon[weaponIndex].GetComponent<Weapon>(); ;
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;
            Invoke("SwapOut", 0.3f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interaction()
    {
        if (iDown && nearObj != null && !isJump && !isDodge)
        {
            if (nearObj.tag == "Weapon")
            {
                Item item = nearObj.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapon[weaponIndex] = true;

                Destroy(nearObj);
            }
        }
    }


    void OnCollisionEnter(Collision other)
    {
        //jump once
        if (other.gameObject.tag == "Ground")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    break;

                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                    {
                        ammo = maxCoin;
                    }
                    break;

                case Item.Type.Grenade:
                    grenade[hasGrenade].SetActive(true);
                    hasGrenade += item.value;
                    if (hasGrenade > maxHasGrenade)
                    {
                        hasGrenade = maxHasGrenade;
                    }
                    break;

                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                    {
                        health = maxHealth;
                    }
                    break;
            }

            Destroy(other.gameObject);

        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enmyBullet = other.GetComponent<Bullet>();
                health -= enmyBullet.damage;
                if(other.GetComponent<Rigidbody>() != null) {
                    Destroy(other.gameObject);
                }
                StartCoroutine(OnDamage());
            }

        }

    }

    IEnumerator OnDamage()
    {

        isDamage = true;
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.color = Color.yellow;
        }

        yield return new WaitForSeconds(1f);

        isDamage = false;
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.color = Color.white;
        }

    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Weapon")
        {
            nearObj = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObj = null;
        }
    }
}
