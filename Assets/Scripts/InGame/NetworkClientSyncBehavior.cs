namespace Love.Core
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public abstract class NetworkClientSyncBehaviour : NetworkBehaviour
    {
        #region Static fields

        static private bool s_registeredHandler = false;

        #endregion

        #region Serialized fields

        [SerializeField]
        bool _forceSend = false;

        #endregion

        #region Private fields

        List<ClientSyncVarBase> _syncFields = new List<ClientSyncVarBase>();


        float _LastClientSyncTime; // last time client received a sync from server
        float _LastClientSendTime; // last time client send a sync to server

        int _componentIndex = -1;
        int componentIndex
        {
            get
            {
                if (_componentIndex == -1)
                {
                    var components = GetComponents<NetworkClientSyncBehaviour>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        if (this == components[i])
                        {
                            _componentIndex = i;
                            break;
                        }
                    }
                }
                return _componentIndex;
            }
        }

        #endregion

        #region Unity events

        public virtual void Awake()
        {
            Type t = this.GetType();
            var fields = t.GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var name = field.Name;
                if (field.FieldType.IsSubclassOf(typeof(ClientSyncVarBase)))
                {
                    var syncvar = (ClientSyncVarBase)field.GetValue(this);
                    if (syncvar == null)
                    {
                        Debug.LogError("SyncVar " + name + " should be initialized!");
                    }
                    _syncFields.Add(syncvar);
                }
            }
        }

        public virtual void Update()
        {
            if (isServer)
            {
                UpdateServer();
            }
            if (isClient)
            {
                UpdateClientLocalAuthority();
            }
        }

        void UpdateServer()
        {
            if (syncVarDirtyBits != 0)
                return;

            if (!NetworkServer.active)
                return;

            if (!isServer)
                return;

            if (GetNetworkSendInterval() == 0)
                return;

            for (int i = 0; i < _syncFields.Count; i++)
            {
                var syncVar = _syncFields[i];
                if (_forceSend || syncVar.dirty)
                {
                    SetDirtyBit(1U << i);
                    syncVar.dirty = false;
                }
            }
        }

        void UpdateClientLocalAuthority()
        {
            if (!hasAuthority)
                return;

            if (!localPlayerAuthority)
                return;

            if (NetworkServer.active)
                return;

            bool dirty = false;
            for (int i = 0; i < _syncFields.Count; i++)
            {
                var syncVar = _syncFields[i];
                if (_forceSend || syncVar.dirty)
                {
                    dirty = true;
                    break;
                }
            }

            if (dirty && Time.time - _LastClientSendTime > GetNetworkSendInterval())
            {
                SendMessage();
                _LastClientSendTime = Time.time;
            }
        }

        #endregion

        #region Network messages

        class Message : MessageBase
        {
            public uint netId;
            public uint componentIndex;
            public byte[] payload;

            public override void Deserialize(NetworkReader reader)
            {
                netId = reader.ReadPackedUInt32();
                componentIndex = reader.ReadPackedUInt32();
                payload = reader.ReadBytesAndSize();
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.WritePackedUInt32(netId);
                writer.WritePackedUInt32(componentIndex);
                writer.WriteBytesAndSize(payload, payload.Length);
            }
        }

        static void OnMessageReceive(NetworkMessage networkMessage)
        {
            var message = networkMessage.ReadMessage<Message>();

            var foundObj = NetworkServer.FindLocalObject(new NetworkInstanceId(message.netId));
            if (foundObj == null)
            {
                Debug.LogError("Received NetworkClientSync data for GameObject that doesn't exist");
                return;
            }
            var foundSyncs = foundObj.GetComponents<NetworkClientSyncBehaviour>();
            if (message.componentIndex >= foundSyncs.Length)
            {
                Debug.LogError("Cannot found NetworkClientSync component");
                return;
            }
            var foundSync = foundSyncs[message.componentIndex];
            if (!foundSync.localPlayerAuthority)
            {
                Debug.LogError("HandleTransform no localPlayerAuthority");
                return;
            }
            if (networkMessage.conn.clientOwnedObjects == null)
            {
                Debug.LogError("HandleTransform object not owned by connection");
                return;
            }

            NetworkReader reader = new NetworkReader(message.payload);
            foundSync.OnDeserialize(reader, false);
        }

        [Client]
        void SendMessage()
        {
            if (ClientScene.readyConnection == null)
            {
                return;
            }

            var writer = new NetworkWriter();
            for (int i = 0; i < _syncFields.Count; i++)
            {
                var syncVar = _syncFields[i];
                if (_forceSend || syncVar.dirty)
                {
                    SetDirtyBit(1U << i);
                }
            }
            OnSerialize(writer, false);

            var message = new Message();
            message.netId = netId.Value;
            message.componentIndex = (uint)componentIndex;
            message.payload = writer.ToArray();

            ClientScene.readyConnection.Send((short)RoomSharingMsgType.NetworkClientSyncLocal, message);
        }

        #endregion

        #region Network events

        public override void OnStartServer()
        {
            _LastClientSyncTime = 0;
            if (!s_registeredHandler)
            {
                NetworkServer.RegisterHandler((short)RoomSharingMsgType.NetworkClientSyncLocal, OnMessageReceive);
                s_registeredHandler = true;
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (!initialState)
            {
                writer.WritePackedUInt64(syncVarDirtyBits);
            }
            for (int i = 0; i < _syncFields.Count; i++)
            {
                var syncVar = _syncFields[i];
                if (initialState || (syncVarDirtyBits & (1UL << i)) != 0)
                {
                    syncVar.Serialize(writer);
                }
            }

            return
                base.OnSerialize(writer, initialState) ||
                initialState || (syncVarDirtyBits != 0);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            ulong dirtyBits = ~0UL;
            if (!initialState)
            {
                dirtyBits = reader.ReadPackedUInt64();
            }
            for (int i = 0; i < _syncFields.Count; i++)
            {
                var syncVar = _syncFields[i];
                if ((dirtyBits & (1UL << i)) != 0)
                {
                    syncVar.Deserialize(reader, localPlayerAuthority && hasAuthority);
                }
            }

            base.OnDeserialize(reader, initialState);
        }

        #endregion

        #region Private methods

        #endregion
    }
}
