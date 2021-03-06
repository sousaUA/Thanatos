using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public ParticleSystem dust; 
    private Rigidbody2D rigidbody;
    private Animator anim;

    //TODO: set SerializeFields For HP Systems
    [Header("For Basic Movement")]
    [SerializeField] public float MovementSpeed = 5;
    [SerializeField] public float JumpForce = 1;
    [SerializeField] Vector3 characterScale;

    [Header("For Jumping")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Vector2 groundCheckSize;

    [Header("For Wall Sliding")]
    [SerializeField] float wallSlidingSpeed;
    [SerializeField] bool isWallSliding;
    [SerializeField] Transform wallCheck;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] Vector2 wallCheckSize;

    [Header("For Wall Jumping")]
    [SerializeField] float wallJumpForce = 7f;
    [SerializeField] float wallJumpDirection = -1f;
    [SerializeField] Vector2 wallJumpAngle;

    [Header("For Dash")]
    [SerializeField] public float dashSpeed;
    [SerializeField] public float startDashTime;
    [SerializeField] private float dashTime;
    [SerializeField] public float dashCooldown;
    [SerializeField] private int dashDirection;
    [SerializeField] private bool used;
    
    [Header("For Combat")]
    [SerializeField] private float maxHP;
    [SerializeField] public float HP;
    [SerializeField] public Transform attackPoint;
    [SerializeField] public float attackRange =  0.7f;
    [SerializeField] public LayerMask enemyLayers;
    [SerializeField] public int attackDamage = 40;

    [Header("For Skills")]
    [SerializeField] public GameObject wallForSkill;
    [SerializeField] public GameObject shuriken;
    [SerializeField] public float throwForce;
    [SerializeField] Transform StoneWallGroundCheck;
    [SerializeField] private float wallCooldown;
    [SerializeField] public float startWallTime;
    [SerializeField] private bool wallUsed;
    [SerializeField] private bool canUseShuriken = true;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        characterScale = transform.localScale;
        dashTime = startDashTime;
        wallCooldown = startWallTime;
        dashDirection = 1;
        used = false;
        wallUsed = false;
        maxHP = 100;
        HP = maxHP;
        wallJumpAngle.Normalize();
    }

    private void Update()
    {
        if (HP > 0)
        {
            var movement = Input.GetAxis("Horizontal");

            #region BASICMOVEMENT
            if((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)) && isTouchingGround() && Mathf.Abs(rigidbody.velocity.y) < 0.001f){
                jump();
            }
            if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)){
                if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)){
                    characterScale.x = -1.7636f;
                    dashDirection = -1;
                    wallJumpDirection = 1f;
                }
                if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)){
                    characterScale.x = 1.7636f;
                    dashDirection = 1;
                    wallJumpDirection = -1f;
                }
                anim.SetBool("isRunning",true);
                transform.position += new Vector3(movement, 0, 0) * Time.deltaTime * MovementSpeed;
                transform.localScale = characterScale;
            } else{
                anim.SetBool("isRunning",false);
            }
            #endregion

            #region DASH
            if (used){
                if (dashCooldown > 0)
                    dashCooldown -= Time.deltaTime;
                else
                {
                    dashCooldown = 3;
                    used = false;
                    if (characterScale.x > 0)
                        dashDirection = 1;
                    else
                        dashDirection = -1;
                }
            }
            if (Input.GetKey(KeyCode.E)){
                if (!used){
                    if (dashTime <= 0){
                        dashTime = startDashTime;
                        dashDirection = 0;
                        rigidbody.velocity = Vector2.zero;
                        used = true; 
                    }
                    else
                    {       
                        dashTime -= Time.deltaTime;
                        anim.SetTrigger("dash");   
                        if (dashDirection > 0)
                            rigidbody.velocity = Vector2.right * dashSpeed;
                        else if (dashDirection < 0)  
                            rigidbody.velocity = Vector2.left * dashSpeed;  
                    }              
                }
            }
            if(!used){
                if (Input.GetKeyUp(KeyCode.E)){
                    dashTime = startDashTime;
                    dashDirection = 0;
                    rigidbody.velocity = Vector2.zero;
                    used = true; 
                }   
            }
            #endregion

            #region COMBAT
            if(Input.GetKeyDown(KeyCode.F)){
                if (isTouchingGround())
                    anim.SetTrigger("attack");
                else
                    anim.SetTrigger("jumpAttack");
                attack();
            }
            if(Input.GetKey(KeyCode.C)){
                anim.SetTrigger("die");
            }
            if(Input.GetKey(KeyCode.H)){
                anim.SetTrigger("hurt");
            }
            #endregion

            #region SKILLS
            //Stone
            if(Input.GetKeyDown(KeyCode.Q) && isTouchingGround() && !wallUsed){
                float offsetX;
                if (transform.localScale.x < 0) offsetX = -1.5f;
                else offsetX = 1.5f;
                StoneWallGroundCheck.transform.position = transform.position + new Vector3(offsetX, -1.8f ,0);
                if (StoneWallIsTouchingGround()){
                    GameObject newStone = Instantiate(wallForSkill);
                    anim.SetTrigger("wallSkill");
                    newStone.transform.position = transform.position + new Vector3(offsetX, 1.5f ,0);
                    newStone.SetActive(true);
                    wallUsed = true;
                    Destroy(newStone, 5);
                }
            }
            if (wallUsed){
                wallCooldown -= Time.deltaTime;
                if (wallCooldown <= 0){
                    wallUsed = false;
                    wallCooldown = startWallTime;
                } 
            }
            //Shuriken
            if(Input.GetKeyDown(KeyCode.R)){
                if (canUseShuriken){
                    anim.SetTrigger("shuriken");
                    GameObject newShuriken = Instantiate(shuriken);
                    float offsetX;
                    if (transform.localScale.x < 0) offsetX = -1f;
                    else offsetX = 1f;
                    newShuriken.transform.position = transform.position + new Vector3(offsetX, 0 , 0);
                    newShuriken.SetActive(true);
                    newShuriken.GetComponent<Rigidbody2D>().AddForce(new Vector2(offsetX * throwForce, 0), ForceMode2D.Impulse);
                    canUseShuriken = false;
                }
            }
            #endregion

            #region WALLSLIDE
            if (isTouchingWall() && !isTouchingGround())
            {
                isWallSliding = true;
                anim.SetBool("isWallSliding", true);
            }
            else
            {
                isWallSliding = false;
                anim.SetBool("isWallSliding", false);
            }
            if (isWallSliding && !isTouchingGround())
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, wallSlidingSpeed);
            if (isWallSliding && (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)))
                jump();
            #endregion
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawCube(wallCheck.position, wallCheckSize);

        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void jump(){
        createDust();
        rigidbody.AddForce(new Vector2(wallJumpForce * wallJumpDirection, JumpForce), ForceMode2D.Impulse);
        anim.SetTrigger("jump");
    }

    void attack(){
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        //Damage Enemies
        foreach(Collider2D enemy in hitEnemies)
        {
            if (enemy.GetComponent<EnemyScript>().currentHealth > 0)
                enemy.GetComponent<EnemyScript>().TakeDamage(attackDamage);
        }
    }

    void createDust(){
        dust.Play();
    }

    bool isTouchingGround(){
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
    }

    bool StoneWallIsTouchingGround(){
        return Physics2D.OverlapBox(StoneWallGroundCheck.position, groundCheckSize, 0, groundLayer);
    }

    bool isTouchingWall(){
        return Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0, wallLayer);
    }
    public void CanUseShuriken(bool canHe){
        canUseShuriken = canHe;
    }

    public void takeDamage(float damage){
        HP -= damage;
        Debug.Log("HP: " + HP);
        if (HP <= 0){
            anim.SetTrigger("die");
        }
    }
}
