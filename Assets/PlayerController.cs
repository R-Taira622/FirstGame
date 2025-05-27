using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // 歩くスピード
    public float jumpForce = 5f; // ジャンプの高度

    private Rigidbody2D rb;
    private Animator animator;
    private float moveInput;
    public GameObject attackHitBox;
    public int playerHP = 3;
    public float knockbackForce = 1f; // この値はInspectorから直接変更できる＋そっちが優先！
    private bool isKnockback = false;
    private bool isInvincible = false;
    private int lastDirection = 1;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        attackHitBox.SetActive(false); // 最初は非表示
    }

    // Update is called once per frame
    void Update()
    {
        // 左右キー入力をチェック（-1〜1の範囲）
        moveInput = Input.GetAxisRaw("Horizontal");
        Debug.Log("moveInput: " + moveInput);

        // 向きを反転する（左向きのとき）
        if (moveInput != 0)
        {
            // GetComponent<SpriteRenderer>().flipX = moveInput < 0;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (moveInput < 0 ? -1 : 1);
            transform.localScale = scale;
            // アニメーション切り替え
            animator.SetBool("isWalking", true);

            // 向きが変わった時だけ処理
            if (moveInput != lastDirection)
            {
                // 攻撃判定の位置を左右で切り替え
                if (attackHitBox != null)
                {
                    Vector3 pos = attackHitBox.transform.localPosition;
                    Debug.Log("HitBox X: " + pos.x);
                    pos.x = Mathf.Abs(pos.x) * (moveInput < 0 ? -1 : 1);
                    attackHitBox.transform.localPosition = pos;
                }

                lastDirection = (int)moveInput;
            }
        }

        else
        {
            animator.SetBool("isWalking", false);
        }

        // ジャンプ入力（スペースキー）
        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // 攻撃の入力チェック（Zキー）
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetTrigger("Attack");
            StartCoroutine(EnableHitBox());
        }

    }

    IEnumerator EnableHitBox()
    {
        attackHitBox.SetActive(true);
        yield return new WaitForSeconds(0.2f); // 攻撃あたり判定がある時間
        attackHitBox.SetActive(false);
    }

    void FixedUpdate()
    {
        if (isKnockback) return; // ノックバック中は何もしない

        // 左右の移動を物理エンジンで適用
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy") && !isInvincible)
        {
            //ダメージを受ける
            playerHP--;
            Debug.Log("ダメージ！残りHP： " + playerHP);

            //ノックバック処理（敵の位置に応じて逆方向に飛ばす）
            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            // knockbackDir += Vector2.up * 0.5f;
            // knockbackDir.Normalize();
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            isKnockback = true; // 移動を一時停止

            //無敵時間スタート
            StartCoroutine(InvincibleCoroutine());
        }
    }

    IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        //点滅を繰り返す
        for (int i = 0; i < 5; i++)
        {
            sr.color = new Color(1, 1, 1, 0.3f); //透明度を下げる
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(1, 1, 1, 1f); //元に戻す
            yield return new WaitForSeconds(0.1f);
        }

        isInvincible = false;
        isKnockback = false; // のけぞり解除
    }
    
}
