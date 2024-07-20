using Unity.Netcode;
using UnityEngine;

namespace GameFramework.Network.Movement
{
    public class NetworkMovementComponent : NetworkBehaviour
    {
        [SerializeField]
        private CharacterController cc;

        [SerializeField]
        private float speed;

        [SerializeField]
        private float turnSpeed;

        [SerializeField]
        private Transform camSocket;

        [SerializeField]
        private GameObject vcam;

        private Transform vcamTransform;

        public Vector2 minMaxRotationX;

        private int tick = 0;
        private float tickRate = 1f / 60f;
        private float tickDeltaTime = 0;

        private const int BUFFER_SIZE = 1024;
        private InputState[] inputStates = new InputState[BUFFER_SIZE];
        private TransformState[] transformStates = new TransformState[BUFFER_SIZE];

        public NetworkVariable<TransformState> ServerTransformState =
            new NetworkVariable<TransformState>();
        public TransformState previousTransformState;

        private void OnEnable()
        {
            ServerTransformState.OnValueChanged += OnServerStateChanged;
        }

        public override void OnNetworkSpawn()
        {
            vcamTransform = vcam.transform;
        }

        private void OnServerStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            previousTransformState = previousvalue;
        }

        public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector2 lookInput)
        {
            tickDeltaTime += Time.deltaTime;
            if (tickDeltaTime > tickRate)
            {
                int bufferIndex = tick % BUFFER_SIZE;

                if (!IsServer)
                {
                    MovePlayerServerRpc(tick, movementInput, lookInput);
                    MovePlayer(movementInput);
                    RotatePlayer(lookInput);
                }
                else
                {
                    MovePlayer(movementInput);
                    RotatePlayer(lookInput);

                    TransformState state = new TransformState()
                    {
                        Tick = tick,
                        Position = transform.position,
                        Rotation = transform.rotation,
                        HasStartedMoving = true
                    };

                    previousTransformState = ServerTransformState.Value;
                    ServerTransformState.Value = state;
                }

                InputState inputState = new InputState()
                {
                    Tick = tick,
                    MovementInput = movementInput,
                    LookInput = lookInput,
                };

                TransformState transformState = new TransformState()
                {
                    Tick = tick,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    HasStartedMoving = true
                };
                inputStates[bufferIndex] = inputState;
                transformStates[bufferIndex] = transformState;

                tickDeltaTime -= tickRate;
                tick++;
            }
        }

        public void ProcessSimulatedPlayerMovement()
        {
            tickDeltaTime += Time.deltaTime;
            if (tickDeltaTime > tickRate)
            {
                if (ServerTransformState.Value.HasStartedMoving)
                {
                    transform.position = ServerTransformState.Value.Position;
                    transform.rotation = ServerTransformState.Value.Rotation;
                }

                tickDeltaTime -= tickRate;
                tick++;
            }
        }

        private void MovePlayer(Vector2 movementInput)
        {
            Vector3 movement =
                movementInput.x * vcamTransform.right + movementInput.y * vcamTransform.forward;

            movement.y = 0;
            if (!cc.isGrounded)
            {
                movement.y = -9.61f;
            }
            cc.Move(movement * speed * tickRate);
        }

        private void RotatePlayer(Vector2 lookInput)
        {
            float cameraAngle = Vector3.SignedAngle(
                transform.forward,
                vcamTransform.forward,
                vcamTransform.right
            );
            float cameraRotationAmount = lookInput.y * turnSpeed * Time.deltaTime;
            float newCameraAngle = cameraAngle - cameraRotationAmount;
            if (newCameraAngle <= minMaxRotationX.x && newCameraAngle >= minMaxRotationX.y)
            {
                vcamTransform.RotateAround(
                    vcamTransform.position,
                    vcamTransform.right,
                    -lookInput.y * turnSpeed * tickRate
                );
            }
            transform.RotateAround(
                transform.position,
                transform.up,
                lookInput.x * turnSpeed * tickRate
            );
        }

        [ServerRpc]
        private void MovePlayerServerRpc(int tick, Vector2 movementInput, Vector2 lookInput)
        {
            MovePlayer(movementInput);
            RotatePlayer(lookInput);

            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = transform.position,
                Rotation = transform.rotation,
                HasStartedMoving = true
            };

            previousTransformState = ServerTransformState.Value;
            ServerTransformState.Value = state;
        }
    }
}
