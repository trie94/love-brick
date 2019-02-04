namespace Love.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public abstract class ClientSyncVarBase
    {
        bool _dirty;
        public bool dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

        public abstract void Serialize(NetworkWriter writer);
        public abstract void Deserialize(NetworkReader reader, bool ignore = false);
    }

    public abstract class ClientSyncVar<T> : ClientSyncVarBase
    {
        [SerializeField]
        T _value;
        Action<T, T> _onChange;
        public Action<T, T> OnChange
        {
            get
            {
                if (_onChange == null)
                {
                    _onChange = (t, o) => { };
                }
                return _onChange;
            }
        }

        public T value
        {
            get { return _value; }
            set
            {
                T temp = _value;
                _value = value;
                dirty = true;
                if (_onChange != null)
                {
                    try
                    {
                        _onChange(value, temp);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }
                }
            }
        }

        public ClientSyncVar(T value, Action<T, T> onChange = null)
        {
            this._value = value;
            this._onChange = onChange;
        }

        public abstract void Serialize(NetworkWriter writer, T value);
        public abstract void Deserialize(NetworkReader reader, out T value);

        public sealed override void Serialize(NetworkWriter writer)
        {
            Serialize(writer, _value);
        }

        public sealed override void Deserialize(NetworkReader reader, bool ignore = false)
        {
            T temp;
            Deserialize(reader, out temp);
            if (!ignore)
            {
                value = temp;
            }
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarChar : ClientSyncVar<char>
    {
        public ClientSyncVarChar(char value, Action<char, char> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, char value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out char value)
        {
            value = reader.ReadChar();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarByte : ClientSyncVar<byte>
    {
        public ClientSyncVarByte(byte value, Action<byte, byte> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, byte value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out byte value)
        {
            value = reader.ReadByte();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarSByte : ClientSyncVar<sbyte>
    {
        public ClientSyncVarSByte(sbyte value, Action<sbyte, sbyte> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, sbyte value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out sbyte value)
        {
            value = reader.ReadSByte();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarString : ClientSyncVar<string>
    {
        public ClientSyncVarString(string value, Action<string, string> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, string value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out string value)
        {
            value = reader.ReadString();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarDecimal : ClientSyncVar<decimal>
    {
        public ClientSyncVarDecimal(decimal value, Action<decimal, decimal> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, decimal value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out decimal value)
        {
            value = reader.ReadDecimal();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarFloat : ClientSyncVar<float>
    {
        public ClientSyncVarFloat(float value, Action<float, float> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, float value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out float value)
        {
            value = reader.ReadSingle();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarDouble : ClientSyncVar<double>
    {
        public ClientSyncVarDouble(double value, Action<double, double> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, double value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out double value)
        {
            value = reader.ReadDouble();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarInt : ClientSyncVar<int>
    {
        public ClientSyncVarInt(int value, Action<int, int> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, int value)
        {
            writer.WritePackedUInt32((uint)value);
        }

        public override void Deserialize(NetworkReader reader, out int value)
        {
            value = (int)reader.ReadPackedUInt32();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarUInt : ClientSyncVar<uint>
    {
        public ClientSyncVarUInt(uint value, Action<uint, uint> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, uint value)
        {
            writer.WritePackedUInt32(value);
        }

        public override void Deserialize(NetworkReader reader, out uint value)
        {
            value = reader.ReadPackedUInt32();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarULong : ClientSyncVar<ulong>
    {
        public ClientSyncVarULong(ulong value, Action<ulong, ulong> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, ulong value)
        {
            writer.WritePackedUInt64(value);
        }

        public override void Deserialize(NetworkReader reader, out ulong value)
        {
            value = reader.ReadPackedUInt64();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarLong : ClientSyncVar<long>
    {
        public ClientSyncVarLong(long value, Action<long, long> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, long value)
        {
            writer.WritePackedUInt64((ulong)value);
        }

        public override void Deserialize(NetworkReader reader, out long value)
        {
            value = (long)reader.ReadPackedUInt64();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarShort : ClientSyncVar<short>
    {
        public ClientSyncVarShort(short value, Action<short, short> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, short value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out short value)
        {
            value = reader.ReadInt16();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarUShort : ClientSyncVar<ushort>
    {
        public ClientSyncVarUShort(ushort value, Action<ushort, ushort> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, ushort value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out ushort value)
        {
            value = reader.ReadUInt16();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarBool : ClientSyncVar<bool>
    {
        public ClientSyncVarBool(bool value, Action<bool, bool> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, bool value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out bool value)
        {
            value = reader.ReadBoolean();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarVector2 : ClientSyncVar<Vector2>
    {
        public ClientSyncVarVector2(Vector2 value, Action<Vector2, Vector2> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Vector2 value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Vector2 value)
        {
            value = reader.ReadVector2();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarVector3 : ClientSyncVar<Vector3>
    {
        public ClientSyncVarVector3(Vector3 value, Action<Vector3, Vector3> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Vector3 value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Vector3 value)
        {
            value = reader.ReadVector3();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarVector4 : ClientSyncVar<Vector4>
    {
        public ClientSyncVarVector4(Vector4 value, Action<Vector4, Vector4> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Vector4 value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Vector4 value)
        {
            value = reader.ReadVector4();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarColor : ClientSyncVar<Color>
    {
        public ClientSyncVarColor(Color value, Action<Color, Color> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Color value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Color value)
        {
            value = reader.ReadColor();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarColor32 : ClientSyncVar<Color32>
    {
        public ClientSyncVarColor32(Color32 value, Action<Color32, Color32> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Color32 value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Color32 value)
        {
            value = reader.ReadColor32();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarQuaternion : ClientSyncVar<Quaternion>
    {
        public ClientSyncVarQuaternion(Quaternion value, Action<Quaternion, Quaternion> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Quaternion value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Quaternion value)
        {
            value = reader.ReadQuaternion();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarRect : ClientSyncVar<Rect>
    {
        public ClientSyncVarRect(Rect value, Action<Rect, Rect> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Rect value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Rect value)
        {
            value = reader.ReadRect();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarPlane : ClientSyncVar<Plane>
    {
        public ClientSyncVarPlane(Plane value, Action<Plane, Plane> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Plane value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Plane value)
        {
            value = reader.ReadPlane();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarRay : ClientSyncVar<Ray>
    {
        public ClientSyncVarRay(Ray value, Action<Ray, Ray> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Ray value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Ray value)
        {
            value = reader.ReadRay();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarMatrix4x4 : ClientSyncVar<Matrix4x4>
    {
        public ClientSyncVarMatrix4x4(Matrix4x4 value, Action<Matrix4x4, Matrix4x4> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Matrix4x4 value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Matrix4x4 value)
        {
            value = reader.ReadMatrix4x4();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarNetworkIdentity : ClientSyncVar<NetworkIdentity>
    {
        public ClientSyncVarNetworkIdentity(NetworkIdentity value, Action<NetworkIdentity, NetworkIdentity> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, NetworkIdentity value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out NetworkIdentity value)
        {
            value = reader.ReadNetworkIdentity();
        }
    }

    [System.Serializable]
    public sealed class ClientSyncVarTransform : ClientSyncVar<Transform>
    {
        public ClientSyncVarTransform(Transform value, Action<Transform, Transform> onChange = null)
            : base(value, onChange)
        {
        }

        public override void Serialize(NetworkWriter writer, Transform value)
        {
            writer.Write(value);
        }

        public override void Deserialize(NetworkReader reader, out Transform value)
        {
            value = reader.ReadTransform();
        }
    }
}
