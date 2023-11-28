﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드 가져오기

// 좀비 AI 구현
public class Zombie : LivingEntity
{
    public LayerMask whatIsTarget; // 추적 대상 레이어

    private LivingEntity targetEntity; // 추적 대상
    private NavMeshAgent navMeshAgent; // 경로 계산 AI 에이전트

    public ParticleSystem hitEffect; // 피격 시 재생할 파티클 효과
    public AudioClip deathSound; // 사망 시 재생할 소리
    public AudioClip hitSound; // 피격 시 재생할 소리

    private Animator zombieAnimator; // 애니메이터 컴포넌트
    private AudioSource zombieAudioPlayer; // 오디오 소스 컴포넌트
    private Renderer zombieRenderer; // 렌더러 컴포넌트

    public float damage = 20f; // 공격력
    public float timeBetAttack = 0.5f; // 공격 간격
    private float lastAttackTime; // 마지막 공격 시점

    // 추적할 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget {
        get
        {
            // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            // 그렇지 않다면 false
            return false;
        }
    }

    private void Awake() {
        // 초기화
        navMeshAgent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        zombieAudioPlayer = GetComponent<AudioSource>();

        zombieRenderer = GetComponentInChildren<Renderer>();

    }

    // 좀비 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(ZombieData zombieData) {

        startingHealth = zombieData.health;
        health = zombieData.health;

        damage = zombieData.damage;

        navMeshAgent.speed = zombieData.speed;

        zombieRenderer.material.color = zombieData.skinColor;

    }

    private void Start() {
        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    private void Update() {
        // 추적 대상의 존재 여부에 따라 다른 애니메이션 재생
        zombieAnimator.SetBool("HasTarget", hasTarget);
    }

    // 주기적으로 추적할 대상의 위치를 찾아 경로 갱신
    private IEnumerator UpdatePath() {
        // 살아 있는 동안 무한 루프
        while (!dead)
        {
            if(hasTarget)
            {

                //추적할것이 존재할때
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(targetEntity.transform.position);

            }
            else
            {
                //추적할것 없음
                navMeshAgent.isStopped = true;

                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget); // Physics.OverlapSphere 가상구의 중심에서 20f반지름 겹치는 해당콜레이더 레이어들만 찾기

                for (int i =0; i<colliders.Length; i++)
                {
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    if(livingEntity != null && !livingEntity.dead)//living엔터티 컴포넌트 가지고 있으면서 죽지 않은 개채있으면
                    {
                        targetEntity = livingEntity;

                        break; //감지했으면 바로 for 탈출
                    }
                }


            }
            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal) {
        //데미지 적용전에 이펙트 방향,위치 조정과 사운드 부분을 하고 
        if(!dead)
        {
            hitEffect.transform.position = hitPoint;//파티클을 맞은 곳에 나오도록 파티클 위치조정
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal); //피격 방향으로조정 즉 날라온방향을 바라보도록

            hitEffect.Play();//이펙실행

            zombieAudioPlayer.PlayOneShot(hitSound); //처맞소리
        }
        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // 사망 처리
    public override void Die() {
        // LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();
        //추가 다른 AI방해않도록 비활성화
        //콜라이더 비활성
        Collider[] zombieColliders = GetComponents<Collider>();
        for (int i = 0; i < zombieColliders.Length; i++)
        {
            zombieColliders[i].enabled = false;
        }
        //네비메시 비활성
        navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;
        //죽는 애니활성
        zombieAnimator.SetTrigger("Die");
        //죽은 효과음
        zombieAudioPlayer.PlayOneShot(deathSound);
    }

    private void OnTriggerStay(Collider other) {
        // 트리거 충돌한 상대방 게임 오브젝트가 추적 대상이라면 공격 실행
        if(!dead && Time.time >=lastAttackTime + timeBetAttack)
        {
            LivingEntity attckTarget = other.GetComponent<LivingEntity>();

            if(attckTarget != null && attckTarget == targetEntity)
            {
                lastAttackTime = Time.time;

                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;

                attckTarget.OnDamage(damage, hitPoint, hitNormal);
            }
        }

    }
}