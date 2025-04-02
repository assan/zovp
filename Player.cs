using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    public float speed = 5f;
    bool IsDead = false;
    Animator anim;
    AudioSource audioSource;
    public AudioClip run;
    public bool IsSoundPlay = false;
    public float currentTime = 0;
    // Характеристики игрока
    public float baseDefense = 10f;
    public float baseAttack = 5f;
    public float totalDefense;
    public float totalAttack;
    public static Player instance;

    public Inventory inventory; // Ссылка на инвентарь



    void Start()
    {
        instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        inventory = Inventory.instance;
        UpdateStats();

    }
    public void UpdateStats()
    {
        totalDefense = baseDefense;
        totalAttack = baseAttack;

        // Проверяем все слоты экипировки
        if (inventory.headSlot != null && inventory.headSlot.equippedItem != null)
        {
            totalDefense += inventory.headSlot.equippedItem.defenseBonus;
            totalAttack += inventory.headSlot.equippedItem.attackBonus;
        }
        if (inventory.ArmorSlot != null && inventory.ArmorSlot.equippedItem != null)
        {
            totalDefense += inventory.ArmorSlot.equippedItem.defenseBonus;
            totalAttack += inventory.ArmorSlot.equippedItem.attackBonus;
        }

        if (inventory.ClothesSlot != null && inventory.ClothesSlot.equippedItem != null)
        {
            totalDefense += inventory.ClothesSlot.equippedItem.defenseBonus;
            totalAttack += inventory.ClothesSlot.equippedItem.attackBonus;
        }
        if (inventory.weapon1Slot != null && inventory.weapon1Slot.equippedItem != null)
        {
            totalDefense += inventory.weapon1Slot.equippedItem.defenseBonus;
            totalAttack += inventory.weapon1Slot.equippedItem.attackBonus;
        }
        if (inventory.weapon2Slot != null && inventory.weapon2Slot.equippedItem != null)
        {
            totalDefense += inventory.weapon2Slot.equippedItem.defenseBonus;
            totalAttack += inventory.weapon2Slot.equippedItem.attackBonus;
        }

        Debug.Log($"Player stats updated: Defense = {totalDefense}, Attack = {totalAttack}");


        // Аналогично для остальных слотов
    }




    void Update()
    {       
            if (IsDead) return;
            if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
            {
                anim.SetBool("IsRun", true);
                rb.velocity = new Vector2(speed, rb.velocity.y);
                spriteRenderer.flipX = false;
                currentTime = 0;
                PlaySteps(true);
            }
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
            {
                anim.SetBool("IsRun", true);
                rb.velocity = new Vector2(-speed, rb.velocity.y);
                spriteRenderer.flipX = true;
                currentTime = 0;
                PlaySteps(true);
            }
            if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow) || (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow)))
            {
                anim.SetBool("IsRun", false);
                rb.velocity = new Vector2(0, rb.velocity.y);
                currentTime = 0;
                PlaySteps(false);
            }
            currentTime += Time.deltaTime;

    }
    void PlaySteps(bool IsRun)
    {
        if (IsRun && !IsSoundPlay)
        {
            IsSoundPlay = true;
            audioSource.clip = run;
            audioSource.Play();

        }
        else if (!IsRun)
        {
            IsSoundPlay = false;
            audioSource.clip = null;
        }
    }
}
