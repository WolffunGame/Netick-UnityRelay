using System;
using System.Collections.Generic;
using UnityEngine;
using Netick.Unity;
using UnityEngine.Serialization;

namespace Netick.Samples.Bomberman
{
    public class BombermanController : NetworkBehaviour
    {
        public List<Bomb> SpawnedBombs = new ();
        [HideInInspector] public Vector3 SpawnPos;
        [SerializeField] private float _speed = 6.0f;
        [SerializeField] private float _speedBoostMultiplayer = 2f;
        [SerializeField] private NetworkObject _bombPrefab;
        private CharacterController _CC;

        // Networked properties
        [Networked] public int Score { get; set; } = 0;
        [Networked] public int PlayerNumber { get; set; }
        [Networked] public bool Alive { get; set; } = true;

        [Networked(relevancy: Relevancy.InputSource)]
        public int MaxBombs { get; set; } = 1;

        [Networked(relevancy: Relevancy.InputSource)]
        public float SpeedPowerUpTimer { get; set; } = 0;

        [Networked(relevancy: Relevancy.InputSource)]
        public float BombPowerUpTimer { get; set; } = 0;

        void Awake()
        {
            // We store the spawn pos so that we use it later during respawn
            SpawnPos = transform.position;
            _CC = GetComponent<CharacterController>();
        }

        public override void NetworkStart()
        {
            base.NetworkStart();
            Respawn();
        }

        public override void OnInputSourceLeft()
        {
            Sandbox.GetComponent<BombermanEventsHandler>().KillPlayer(this);
            // destroy the player object when its input source (controller player) leaves the game
            Sandbox.Destroy(Object);
        }

        public override void NetworkFixedUpdate()
        {
            if (!Alive || !FetchInput(out BombermanInput input))
                return;
            
            if (BombPowerUpTimer > 0)
                BombPowerUpTimer -= Sandbox.FixedDeltaTime;
            else
                MaxBombs = 1;

            if (SpeedPowerUpTimer > 0)
                SpeedPowerUpTimer -= Sandbox.FixedDeltaTime;

            var hasSpeedBoost = SpeedPowerUpTimer > 0;
            var speed = hasSpeedBoost ? _speed * _speedBoostMultiplayer : _speed;

            _CC.Move(input.Movement * speed * Sandbox.FixedDeltaTime);

            // we make sure the z coord of the pos of the player is always zero
            var tran = transform;
            var position = tran.position;
            position = new Vector3(position.x, position.y, 0f);
            tran.position = position;

            if (!IsServer || !input.PlantBomb || SpawnedBombs.Count >= MaxBombs) return;
            // * round the bomb pos so that it snaps to the nearest square.
            var bomb = Sandbox.NetworkInstantiate(_bombPrefab.gameObject, Round(transform.position), Quaternion.identity)
                .GetComponent<Bomb>();
            bomb.Bomber = this;
        }

        public void ReceivePowerUp(PowerUpType type, float boostTime)
        {
            switch (type)
            {
                case PowerUpType.IncreaseBombs:
                    SpeedPowerUpTimer += boostTime;
                    break;
                case PowerUpType.Speed:
                    BombPowerUpTimer += boostTime;
                    MaxBombs += 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void Die()
        {
            Alive = false;
            Sandbox.GetComponent<BombermanEventsHandler>().KillPlayer(this);
        }

        public void Respawn()
        {
            Alive = true;
            Sandbox.GetComponent<BombermanEventsHandler>().RespawnPlayer(this);

            transform.position = SpawnPos;

            SpeedPowerUpTimer = 0;
            BombPowerUpTimer = 0;
            MaxBombs = 1;
        }

        [OnChanged(nameof(Alive))]
        private void OnAliveChanged(OnChangedData onChangedData)
        {
            // Based on state of Alive:

            // * Hide/show player object
            GetComponentInChildren<Renderer>().SetEnabled(Sandbox, Alive);

            // * Enable/disable the CharacterController
            _CC.enabled = Alive;
        }

        public Vector3 Round(Vector3 vec)
        {
            return new Vector3(Mathf.Round(vec.x), Mathf.Round(vec.y), Mathf.Round(vec.z));
        }
    }
}