/*  TargetMove is sets to player pos if { distance to player is over  70 and is enemy doesnt hunt) && (player is hunted) && (is coroutine lookingForPlayerCoroutine())}
 *  TargetMove is sets to random pos if { Distance to current target is less than 3 && is not hunting && enemy not looking for player}
 * 
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.VisualScripting;

public class AI_Manager : MonoBehaviour
{
    #region Player
    Player _player;
    Vector3 playerPos;
    #endregion

    #region Enemy
    Enemy _enemy;
    Vector3 enemyPos;
    NavMeshAgent _navMesh;
    #endregion

    #region AI  

    GameObject HideObject;
    Vector3 hideObjectPos;
    Vector3 closeHideObjectPos;

    Ray raycast;
    RaycastHit hit;
    //*****************************************************
    [HideInInspector] public float maxMove = 60f;    //Max target distance 
    [HideInInspector] public float speed = 5f;
    //*****************************************************
    Vector3 lastPlayerPos = Vector3.zero;
    public Vector3 targetMove = Vector3.zero;
    public GameObject Target;
    //*****************************************************
    public bool isInFieldOfView;
    public bool isHunt;
    public bool isRay;
    public bool sameRoom;
    public bool canMove;
    public bool canChangeTarget;
    //***********************
    public bool isAfterHunt;
    public bool isAfterHuntCoroutine;

    public bool isLookingForHiddenPlayer;
    public bool isLookingForHiddenPlayerCoroutine;

    public bool isLookingForPlayerAfterHunt;
    public bool isLookingForPlayerAfterHuntCoroutine;
    //*****************************************************

    #endregion

    #region temp
    public LayerMask playerLM;
    GameManager _gameManager;

    //public List<GameObject> interactiveOBJ = new List<GameObject>();

    #endregion

    void Start()
    {
        Target = GameObject.FindGameObjectWithTag("target");
        _navMesh = GameObject.FindGameObjectWithTag("Enemy").GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        _enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Enemy>();

        #region temp
        targetMove = transform.position;    //set target to zero
        #endregion
    }


    void Raycast()
    {
        Vector3 currPos = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
        Vector3 direct = new Vector3(_player.gameObject.transform.position.x, _player.gameObject.transform.position.y - 2, _player.gameObject.transform.position.z);
        Vector3 raycastDir = (direct - transform.position).normalized;
        Physics.Raycast(currPos, raycastDir, out hit, Mathf.Infinity);
        Debug.DrawRay(currPos, raycastDir * hit.distance, Color.blue);
    }
    void randomMove()
    {
        if (!isHunt && !isAfterHuntCoroutine)
            canChangeTarget = true;
        else
            canChangeTarget = false;

        if (Vector3.Distance(transform.position, targetMove) < 3f && canChangeTarget)
        {
            targetMove = new Vector3(Random.Range(_player.transform.position.x - maxMove, maxMove + _player.transform.position.x)
                , transform.position.y, Random.Range(_player.transform.position.z - maxMove, maxMove + _player.transform.position.z));
        }

        if (targetMove != Vector3.zero)    //if IS target
        {
            Target.transform.position = targetMove;
            _enemy.cameraObject.transform.LookAt(Target.transform.position);
        }

    }
    void AI_seekPlayer()
    {
        if (Vector3.Distance(enemyPos, playerPos) > 70 && !isHunt)
        {
            targetMove = playerPos;
        }

        //************************************************************

        if (isAfterHunt && !isHunt && !isAfterHuntCoroutine)
        {
            StartCoroutine(afterHuntCoroutine());
        }
        else
        {
            StopCoroutine(afterHuntCoroutine());
        }


    }

    void hunt()
    {
        #region FieldOfView
        Vector3 screenPoint = _enemy.enemyView.WorldToViewportPoint(_player.transform.position);
        if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            isInFieldOfView = true;
        else
            isInFieldOfView = false;

        #endregion

        if (isInFieldOfView && Vector3.Distance(_player.transform.position, _enemy.transform.position) < 30f)  //Player is in field of view the enemy AND is closest than 30
        {
            if (isRay)                          //Check the player is this same room
            {
                sameRoom = true;
            }
            else
            {
                sameRoom = false;
            }


            if (isRay && !_player.isHided)      //if player is not hidden *isHunt, *isAfterHunt
            {
                isHunt = true;
                isAfterHunt = true;
            }
            else if (isRay && _player.isHided)  //if player is hidden !*isAfterHunt
            {
                isAfterHunt = false;
            }
            else                                //if player is behind wall, !*isHunt
            {
                isHunt = false;
            }
        }
        else                                    //if player is over 30 distance or he is not in fieldOfView !*isHunt
        {
            isHunt = false;
        }

    }
    void isHunt_v()
    {
        Vector3 huntedPlayerPos = new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z);
        if (isHunt)
        {
            targetMove = huntedPlayerPos;
            speed = 9f;
            _enemy._anim.speed = 1f;
            _enemy._anim.SetBool("Walking", false);
            _enemy._anim.SetBool("Run", true);


        }
        else
        {
            if (isAfterHunt)
            {
                speed = 9f;
                _enemy._anim.SetBool("Walking", false);
                _enemy._anim.SetBool("Run", true);
            }
            else //if (isLookingForPlayerAfterHunt)
            {
                speed = 3f;
                _enemy._anim.SetBool("Run", false);
                _enemy._anim.SetBool("Walking", true);
            }
        }
    }
    void NavAgent()
    {
        if (canMove)
        {
            _navMesh.speed = speed;
            _navMesh.destination = targetMove;
            _navMesh.updateRotation = true;
        }
    }
    void Update()
    {
        Raycast();
        #region bool Manager
        //*********************************************

        if (targetMove != null && !isLookingForHiddenPlayer && (_enemy._anim.GetBool("Run") || _enemy._anim.GetBool("Walking")))        canMove = true;
        else                                                                                                                            canMove = false;

        //*********************************************

        if(hit.collider.gameObject.tag == "Player") isRay = true;
        else                                        isRay = false;

        //*********************************************
        #endregion

        targetMove = new Vector3(targetMove.x, transform.position.y, targetMove.z);
        enemyPos = _enemy.transform.position;
        playerPos = new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z);
        

        if (canMove)
            NavAgent();

        randomMove();
        AI_seekPlayer();
        hunt();
        isHunt_v();

        if (_player.hideObject != null && _player.isHided)
        {
            float distanceToTable = 3f;
            hideObjectPos = _player.hideObject.transform.position;
            closeHideObjectPos = new Vector3(hideObjectPos.x + distanceToTable, transform.position.y, hideObjectPos.z + distanceToTable);

        }
    }
    IEnumerator afterHuntCoroutine()                    //If AI doesnt see a player, he still run to him for 8 seconds
    {
        isAfterHuntCoroutine = true;                   //is coroutine
        targetMove = playerPos;                     //Go to player position
        if (isHunt)
        {
            isAfterHunt = false;                    //If enemy found the player, stop this coroutine
            isAfterHuntCoroutine = false;
            StopCoroutine(afterHuntCoroutine());
        }
        if (sameRoom)                  
        {
            isLookingForPlayerAfterHunt = true;

            isAfterHunt = false;                //If enemy is in this same room, stop this coroutine and run lookingForPlayerCoroutine
            isAfterHuntCoroutine = false;
            StartCoroutine(lookingForPlayerCoroutine());
            StopCoroutine(afterHuntCoroutine());
        }
        
        yield return new WaitForSeconds(30);

        isAfterHunt = false;
        isAfterHuntCoroutine = false;
        
    }
    IEnumerator lookingForPlayerCoroutine()
    {

        isLookingForPlayerAfterHuntCoroutine = true;

        
        yield return new WaitForSeconds(10);

        isLookingForPlayerAfterHuntCoroutine = false;
        isLookingForPlayerAfterHunt = false;
    }



    //IEnumerator lookingForPlayerAfterHuntCoroutine()    //The AI looking for player if, player is not hidden
    //{
    //    isLookingForPlayerAfterHuntCoroutine = true;   //is coroutine
    //    isLookingForPlayerAfterHunt = true;

    //    if (_player.isHided && sameRoom)
    //    {
    //        isLookingForPlayerAfterHunt = false;
    //        isLookingForPlayerAfterHuntCoroutine = false;
    //        StartCoroutine(lookingForHiddenPlayerCoroutine());
    //    }
    //    else if (isHunt)
    //    {
    //        StopAllCoroutines();
    //    }


    //    yield return new WaitForSeconds(Random.Range(10, 15));

    //    if (_player.isHided && sameRoom)
    //    {
    //        isLookingForPlayerAfterHunt = false;
    //        isLookingForPlayerAfterHuntCoroutine = false;
    //        StartCoroutine(lookingForHiddenPlayerCoroutine());
    //    }
    //    isLookingForPlayerAfterHunt = false;
    //    isLookingForPlayerAfterHuntCoroutine = false;


    //}
    //IEnumerator lookingForHiddenPlayerCoroutine()       //The AI looking for player if, player is hidden
    //{
    //    isLookingForHiddenPlayerCoroutine = true;            //is coroutine
    //    isLookingForHiddenPlayer = true;


    //    _enemy._anim.SetBool("Run", false);
    //    _enemy._anim.SetBool("Walking", false);
    //    _enemy._anim.SetBool("Looking", true);
    //    if (isHunt)
    //    {
    //        StopCoroutine(lookingForHiddenPlayerCoroutine());
    //    }

    //    if (!_player.isHided)
    //    {
    //        if (isLookingForHiddenPlayerCoroutine)
    //        {
    //            StopCoroutine(lookingForHiddenPlayerCoroutine());
    //        }
    //    }

    //    yield return new WaitForSeconds(Random.Range(15, 30));


    //    _enemy._anim.SetBool("Looking", false);
    //    isLookingForHiddenPlayer = false;
    //    isLookingForHiddenPlayerCoroutine = false;
    //}
}
