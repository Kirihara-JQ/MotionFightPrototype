using System.Collections;
using System.Collections.Generic;
using Gamekit3D;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

namespace MotionFight.Arena
{
    /// <summary>
    /// Applies the minimum 1v1 scene contract for the CircularArea prototype.
    /// Combat input, attack animation, damage and match flow remain separate systems.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class ArenaModeController : MonoBehaviour
    {
        [Header("Combat Participants")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private ChomperBehavior activeMonster;
        [SerializeField] private List<GameObject> nonParticipatingEnemies = new List<GameObject>();

        [Header("CircularArea Spawns")]
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform monsterSpawn;
        [SerializeField] private Transform circularAreaReference;

        [Header("Third-Person Camera")]
        [SerializeField] private CameraSettings cameraSettings;

        [Header("Legacy Flow Isolation")]
        [SerializeField] private List<GameObject> disabledLegacyFlowRoots = new List<GameObject>();

        [Header("Runtime")]
        [SerializeField] private bool applySpawnsOnAwake = true;
        [SerializeField] private bool logConfiguration;

        public PlayerController Player => player;
        public ChomperBehavior ActiveMonster => activeMonster;
        public Transform PlayerSpawn => playerSpawn;
        public Transform MonsterSpawn => monsterSpawn;
        public Transform CircularAreaReference => circularAreaReference;

        private void Awake()
        {
            ApplyArenaMode();
        }

        private IEnumerator Start()
        {
            yield return null;
            FrameCombatants();
        }

        public void ApplyArenaMode()
        {
            DisableLegacyFlow();
            EnforceSingleMonster();

            if (applySpawnsOnAwake)
            {
                PlacePlayer();
                PlaceMonster();
            }

            ConfigureKeyboardAndMouseCamera();

            if (playerInput != null)
                playerInput.GainControl();

            if (logConfiguration)
                Debug.Log("CircularArea 1v1 mode active: one player, one Chomper, legacy flow isolated.", this);
        }

        private void DisableLegacyFlow()
        {
            foreach (var target in disabledLegacyFlowRoots)
            {
                if (target != null && target != gameObject)
                    target.SetActive(false);
            }
        }

        private void EnforceSingleMonster()
        {
            foreach (var enemy in nonParticipatingEnemies)
            {
                if (enemy != null && (activeMonster == null || enemy != activeMonster.gameObject))
                    enemy.SetActive(false);
            }

            if (activeMonster != null)
                activeMonster.gameObject.SetActive(true);
        }

        private void PlacePlayer()
        {
            if (player == null || playerSpawn == null)
                return;

            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
                characterController.enabled = false;

            player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

            if (characterController != null)
                characterController.enabled = true;
        }

        private void PlaceMonster()
        {
            if (activeMonster == null || monsterSpawn == null)
                return;

            var agent = activeMonster.GetComponent<NavMeshAgent>();
            if (Vector3.SqrMagnitude(activeMonster.transform.position - monsterSpawn.position) > 0.0001f ||
                Quaternion.Angle(activeMonster.transform.rotation, monsterSpawn.rotation) > 0.1f)
            {
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                    agent.Warp(monsterSpawn.position);
                else
                    activeMonster.transform.position = monsterSpawn.position;

                activeMonster.transform.rotation = monsterSpawn.rotation;
            }
        }

        private void ConfigureKeyboardAndMouseCamera()
        {
            if (cameraSettings == null)
                return;

            cameraSettings.inputChoice = CameraSettings.InputChoice.KeyboardAndMouse;
        }

        private void FrameCombatants()
        {
            if (player == null || activeMonster == null || cameraSettings == null)
                return;

            var camera = cameraSettings.keyboardAndMouseCamera;
            var orbitalFollow = camera != null ? camera.GetComponent<CinemachineOrbitalFollow>() : null;
            if (orbitalFollow == null)
                return;

            var toMonster = activeMonster.transform.position - player.transform.position;
            toMonster.y = 0f;
            if (toMonster.sqrMagnitude < 0.001f)
                toMonster = player.transform.forward;

            var direction = toMonster.normalized;
            var cameraPosition = player.transform.position - direction * 5f + Vector3.up * 2.7f;
            var playerHead = player.transform.position + Vector3.up * 1.45f;
            var monsterHead = activeMonster.transform.position + Vector3.up * 0.9f;
            var lookPoint = Vector3.Lerp(playerHead, monsterHead, 0.35f);
            var cameraRotation = Quaternion.LookRotation((lookPoint - cameraPosition).normalized, Vector3.up);

            orbitalFollow.ForceCameraPosition(cameraPosition, cameraRotation);
        }

        private void OnValidate()
        {
            if (activeMonster != null && nonParticipatingEnemies.Contains(activeMonster.gameObject))
                Debug.LogWarning("The active Chomper must not also appear in nonParticipatingEnemies.", this);
        }
    }
}
