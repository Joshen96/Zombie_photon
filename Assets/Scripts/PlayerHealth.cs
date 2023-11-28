using UnityEngine;
using UnityEngine.UI; // UI 관련 코드

// 플레이어 캐릭터의 생명체로서의 동작을 담당
public class PlayerHealth : LivingEntity {
    public Slider healthSlider; // 체력을 표시할 UI 슬라이더

    public AudioClip deathClip; // 사망 소리
    public AudioClip hitClip; // 피격 소리
    public AudioClip itemPickupClip; // 아이템 습득 소리

    private AudioSource playerAudioPlayer; // 플레이어 소리 재생기
    private Animator playerAnimator; // 플레이어의 애니메이터

    private PlayerMovement playerMovement; // 플레이어 움직임 컴포넌트
    private PlayerShooter playerShooter; // 플레이어 슈터 컴포넌트

    private void Awake() {
        // 사용할 컴포넌트를 가져오기
        playerAnimator = GetComponent<Animator>();
        playerAudioPlayer = GetComponent<AudioSource>();

        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();


    }

    protected override void OnEnable() { //부활기능으로 활용하기위함
        // LivingEntity의 OnEnable() 실행 (상태 초기화)
        base.OnEnable(); //생명체가 활성화될때 상태를 리셋하는것


        healthSlider.gameObject.SetActive(true);
        healthSlider.maxValue = startingHealth; //슬라이더 최대값을 시작 체력으로 세팅
        healthSlider.value = health;            //현재 체력값을 슬라이더 값을 세팅

        //살아있어서 조작 컴포넌트 활성화 //die()시 비활성화 할것이기에 여기서 다시 활성위함
        playerMovement.enabled = true;
        playerShooter.enabled = true;


    }

    // 체력 회복
    public override void RestoreHealth(float newHealth) {
        // LivingEntity의 RestoreHealth() 실행 (체력 증가)
        base.RestoreHealth(newHealth);

        healthSlider.value = health; // 체력 ui 슬라이더에도 적용
    }

    // 데미지 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitDirection) {

        if (!dead) //죽지 않을때만 히트 사운드 출력
        {
            playerAudioPlayer.PlayOneShot(hitClip);
        }
        
        
        // LivingEntity의 OnDamage() 실행(데미지 적용)
        base.OnDamage(damage, hitPoint, hitDirection);

        healthSlider.value = health;
    }

    // 사망 처리
    public override void Die() {
        // LivingEntity의 Die() 실행(사망 적용)
        base.Die();
        //죽으면 ui창 비활성화
        healthSlider.gameObject.SetActive(false);
        //죽은 사운드, 죽음 애니메이션 처리
        playerAudioPlayer.PlayOneShot(deathClip);
        playerAnimator.SetTrigger("Die");
        //죽으면 이동 공격 컴포넌트 비활성화
        playerMovement.enabled = false;
        playerShooter.enabled = false;
    }

    private void OnTriggerEnter(Collider other) {
        // 아이템과 충돌한 경우 해당 아이템을 사용하는 처리
        if (!dead)
        {
            IItem item = other.GetComponent<IItem>();

            if(item !=null)
            {
                item.Use(gameObject);

                playerAudioPlayer.PlayOneShot(itemPickupClip);
            }
        }

    }
}