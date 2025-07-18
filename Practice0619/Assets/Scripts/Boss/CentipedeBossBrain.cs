using UnityEngine;
using UnityEngine.AI;
using RPG.BehaviourTree;
using static RPG.BehaviourTree.Node;
using RPG.BehaviourTree.Centipede;
using RPG.Boss.Centipede;

public class CentipedeBossBrain : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CentipedeRigController rig;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject poisonProjectilePrefab;
    [SerializeField] private NavMeshAgent agent;   // ★추가

    [Header("Params")]
    [SerializeField] private float spitRange = 10f;
    [SerializeField, Min(0.1f)] private float spitCooldown = 7f;
    [SerializeField] private float chargeSpeed = 6f;
    [SerializeField] private float chargeStopDistance = 1.5f;
    [SerializeField] private float chargeMaxDuration = 3f;
    [SerializeField, Min(0.1f)] private float chargeCooldown = 4f;

    [Header("Wave Params")]
    [SerializeField] private GameObject waveProjectilePrefab;
    [SerializeField] private float waveRange = 12f;
    [SerializeField, Min(0.1f)] private float waveCooldown = 10f;
    [SerializeField] private float waveSpeed = 8f;

    private Node root;

    private void Awake()
    {
        if (waveCooldown <= 0f) waveCooldown = 10f;

        var spit = new SpitAction(rig, player, poisonProjectilePrefab, 12f, spitRange);
        var spitCd = new Cooldown(spit, spitCooldown);

        var wave = new WaveAction(rig, player, waveProjectilePrefab, waveSpeed, waveRange, true);
        var waveCd = new Cooldown(wave, waveCooldown);

        var charge = new ChargeAction(transform, agent, player, rig,
            chargeSpeed, chargeStopDistance, chargeMaxDuration);
        var chargeCd = new Cooldown(charge, chargeCooldown);

        // 현재는 우선순위형 Selector: Spit -> Wave -> Charge
        root = new Selector(spitCd, waveCd, chargeCd);

    }

    private void Update()
    {
        root.Update(Time.deltaTime);
    }
}
