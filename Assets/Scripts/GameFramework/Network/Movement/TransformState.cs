using System;
using Unity.Netcode;
using UnityEngine;

namespace GameFramework.Network.Movement
{
    public class TransformState : INetworkSerializable, IEquatable<TransformState>
    {
        public int Tick;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool HasStartedMoving;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Tick);
                reader.ReadValueSafe(out Position);
                reader.ReadValueSafe(out Rotation);
                reader.ReadValueSafe(out HasStartedMoving);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Tick);
                writer.WriteValueSafe(Position);
                writer.WriteValueSafe(Rotation);
                writer.WriteValueSafe(HasStartedMoving);
            }
        }

        public bool Equals(TransformState other)
        {
            // Implement your equality comparison logic here
            // Compare Tick, Position, Rotation, and HasStartedMoving properties
            // Return true if they are equal, otherwise return false
            // Example:
            return Tick == other.Tick
                && Position == other.Position
                && Rotation == other.Rotation
                && HasStartedMoving == other.HasStartedMoving;
        }
    }
}
