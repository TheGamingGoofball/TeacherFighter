﻿using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

//This is the script that Mr Taylor Uses for movement

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof (PlatformerCharacter2D))]
    public class TaylorFightControl : MonoBehaviour
    {
        private PlatformerCharacter2D m_Character;

        private bool m_Jump;
        private bool m_Dodge;
        private bool m_FacingRight = true;
        private bool lightActive, mediumActive, heavyActive, jumpActive;
        private int lightCount;
        private Cooldown lightSpamCooldown;

        private float lightCooldownAmount;

        private Cooldown fireCooldown;
        private Cooldown lightCooldown;
        private Cooldown mediumCooldown;
        private Cooldown heavyCooldown;
        private Cooldown moveActive; // Used because there is a slight delay between anim.trigger and the actual animation returning active
        private Cooldown damageWait;

        public Transform firePoint;
        public Transform respawnPoint;
        public Transform basicAttackPoint;
        
        public GameObject fireBallPrefab;
        private GameObject cooldownUI;

        public float speed = 20f;
        public float basicAttackRange = 0.5f;
        
        private SimpleHealthBar playerHealthBar;
        private SimpleHealthBar staminaBar;
        private Stamina stamina;

        private Vector3 startPosition;
        private Animator anim;
        public LayerMask enemyLayers;
        private Component[] audioArray;
        private AudioSource[] audioData;


        private void Start()
        {
           anim = gameObject.GetComponent<Animator>();
           stamina = gameObject.GetComponent<Stamina>();
        }
      
     
        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();
            startPosition = transform.position;
            anim = gameObject.GetComponent<Animator>();
            playerHealthBar = gameObject.GetComponent<PlatformerCharacter2D>().healthBarObject.GetComponent<SimpleHealthBar>();
            staminaBar = gameObject.GetComponent<PlatformerCharacter2D>().staminaBarObject.GetComponent<SimpleHealthBar>();
            
            audioArray = gameObject.GetComponents(typeof(AudioSource));
            audioData = new AudioSource[audioArray.Length];
            
            for(int i = 0; i < audioArray.Length; i++) 
            {
                audioData[i] = (AudioSource) audioArray[i];
            }

            heavyCooldown = gameObject.AddComponent<Cooldown>();
            mediumCooldown = gameObject.AddComponent<Cooldown>();
            lightCooldown = gameObject.AddComponent<Cooldown>();
            fireCooldown = gameObject.AddComponent<Cooldown>();
            damageWait = gameObject.AddComponent<Cooldown>();
            moveActive = gameObject.AddComponent<Cooldown>();
            lightSpamCooldown = gameObject.AddComponent<Cooldown>();

            lightCount = 0;
            
        }


        private void Update()
        {
            

            if (!m_Jump)
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            if (!m_Dodge)
            
                m_Dodge = CrossPlatformInputManager.GetButtonDown("Taylor_Dodge");
            if (m_Character.m_Grounded) 
                jumpActive = false;
            else 
                jumpActive = true;
            // Detect current active attack
            
            heavyActive = this.anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy");
            mediumActive = this.anim.GetCurrentAnimatorStateInfo(0).IsName("Medium");
            lightActive = this.anim.GetCurrentAnimatorStateInfo(0).IsName("Light");

            if (!lightSpamCooldown.active())
                lightCount = 0;
                
            // Handle Inputs

            if (!this.anim.GetCurrentAnimatorStateInfo(0).IsName("Stun") && !moveActive.active() && !this.anim.GetCurrentAnimatorStateInfo(0).IsName("Block")) 
            {

                if(Input.GetButtonDown("Taylor_Fire") || Input.GetAxis("Axis 10") != 0 && !heavyActive)
                {
                    if (stamina.getStamina() >= 20f) 
                    {
                        if (!fireCooldown.active()) 
                        {
                        Shoot();
                        stamina.startCountdown(1f);
                        fireCooldown.startCooldown(0.5f);
                        }
                    }
                }
                else if (Input.GetButtonDown("Taylor_Light") && !heavyActive) 
                {
                    if (!lightCooldown.active()) 
                    {
                        Light();
                        lightCount++;
                        if (lightCount < 3) {
                            lightCooldown.startCooldown(0.2f);
                            lightCooldownAmount = 0.2f;
                            lightSpamCooldown.startCooldown(0.5f);
                        }
                        else {
                            lightCount = 0;
                            lightCooldown.startCooldown(0.5f);
                            lightCooldownAmount = 0.5f;
                        }
                    }
                }
                else if (Input.GetButtonDown("Taylor_Medium") && !heavyActive) 
                {
                    if (!mediumCooldown.active()) 
                    {
                        Medium();
                        mediumCooldown.startCooldown(0.5f);
                        moveActive.startCooldown(0.2f);
                    }
                }
                else if (Input.GetButtonDown("Taylor_Heavy")) 
                {
                    if (!heavyCooldown.active()) 
                    {
                        Heavy();
                        heavyCooldown.startCooldown(0.8f);
                        moveActive.startCooldown(0.5f);
                    }
                }
            }

            // Updates Cooldown UI
            cooldownUI = m_Character.cooldownUI;

            // Light
            cooldownUI.transform.GetChild(0).gameObject.transform.GetChild(0)
                .gameObject.transform.GetChild(0).GetComponent<SimpleHealthBar>().UpdateBar(lightCooldown.getCurrentTime(), lightCooldownAmount);
            // Medium
            cooldownUI.transform.GetChild(1).gameObject.transform.GetChild(0)
                .gameObject.transform.GetChild(0).GetComponent<SimpleHealthBar>().UpdateBar(mediumCooldown.getCurrentTime(), 0.5f);
            // Heavy
            cooldownUI.transform.GetChild(2).gameObject.transform.GetChild(0)
                .gameObject.transform.GetChild(0).GetComponent<SimpleHealthBar>().UpdateBar(heavyCooldown.getCurrentTime(), 0.8f);
            // Special
            cooldownUI.transform.GetChild(3).gameObject.transform.GetChild(0)
                .gameObject.transform.GetChild(0).GetComponent<SimpleHealthBar>().UpdateBar(fireCooldown.getCurrentTime(), 0.2f);

            // Freeze constraints after doing basic moves

            if(this.anim.GetCurrentAnimatorStateInfo(0).IsName("Light") || this.anim.GetCurrentAnimatorStateInfo(0).IsName("Medium") 
            || this.anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy") || this.anim.GetCurrentAnimatorStateInfo(0).IsName("Fire")) {
                gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }

        }

        private void FixedUpdate()
        {
            // Read the inputs.
            bool crouch = Input.GetKey(KeyCode.LeftControl);
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
        
            if(m_Dodge) 
            {
                if(h > 0) 
                    h = 5;
                else if (h < 0) 
                    h = (-5);
            }

            // Pass all parameters to the character control script.
            m_Character.Move(h, crouch, m_Jump);

            if(m_Jump && !this.anim.GetCurrentAnimatorStateInfo(0).IsName("Jumping") && !audioData[3].isPlaying && !jumpActive)
                audioData[3].Play();

            m_Jump = false;
            m_Dodge = false;

            
        }

        private void Flip()
        {
            m_FacingRight = !m_FacingRight;
            transform.Rotate(0f, 180f, 0f);
        }

 
        void Shoot()
        {

            if(damageWait.isInitial()) 
            {
                anim.SetTrigger("Fire");
                damageWait.startCooldown(Shoot, 0.2f);
            }

            if(!damageWait.isInitial()) 
            {

            GameObject ballClone = Instantiate(fireBallPrefab, firePoint.position, firePoint.rotation);
            ballClone.transform.localScale = transform.localScale;
            stamina.staminaDecrease(20f);
            }
        }

        void Light() 
        {
            anim.SetTrigger("Light");
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(basicAttackPoint.position, basicAttackRange, enemyLayers);

            foreach(Collider2D enemy in hitEnemies)
            {
                AudioSource.PlayClipAtPoint(audioData[0].clip, gameObject.transform.position);
                enemy.GetComponent<Damage>().doDamage(1.5f, 0.5f);
            }
        }
    

        void Medium() 
        {
            if(damageWait.isInitial()) 
            {
                anim.SetTrigger("Medium");
                damageWait.startCooldown(Medium, 0.2f);
            }

            if(!damageWait.isInitial()) 
            {
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(basicAttackPoint.position, basicAttackRange, enemyLayers);

                foreach(Collider2D enemy in hitEnemies)
                {
                    AudioSource.PlayClipAtPoint(audioData[2].clip, gameObject.transform.position);
                    enemy.GetComponent<Damage>().doDamage(4f, 0.5f);

                }
            }
        }

        void Heavy() 
        {
            if(damageWait.isInitial())
            {
                anim.SetTrigger("Heavy");
                damageWait.startCooldown(Heavy, 0.2f);
            }
      
            if(!damageWait.isInitial()) 
            {
            
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(basicAttackPoint.position, basicAttackRange, enemyLayers);
                foreach(Collider2D enemy in hitEnemies)
                {   
                    AudioSource.PlayClipAtPoint(audioData[1].clip, gameObject.transform.position);
                    enemy.GetComponent<Damage>().doDamage(8f, 0.5f);

                }
            }
        
        }

        void OnDrawGizmosSelected()
        {
            if(basicAttackPoint == null)
            return;
            Gizmos.DrawWireSphere(basicAttackPoint.position, basicAttackRange);
        }
    }        
}
