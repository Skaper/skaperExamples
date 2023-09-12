using System;
using System.Collections;
using Photon.Pun;
using ExampleApp.PUN;

namespace ExampleApp.Core.Behaviours
{
    public abstract class ExampleAppBehaviour : MonoBehaviourPunCallbacks, IPunObservable
    {
        protected bool IsMultiplayerAndMasterClient => ProjectData.IsMultiplayerAndMasterClient;

        protected RpcTarget _rpcTarget = RpcTarget.Others;

        private void Awake()
        {
            StartCoroutine(SetupMultiplayer());
            AwakeOverride();
        }

        private void OnDestroy()
        {
            if (!Settings.Instance.Multiplayer)
            {
                return;
            }

            if (photonView)
            {
                photonView.ObservedComponents.Remove(this);
            }
            
            MasterLoading.SpectatorJoined -= ResendRpcCalls;
        }

        protected virtual void AwakeOverride() { }

        private IEnumerator SetupMultiplayer()
        {
            if (!Settings.Instance.Multiplayer)
            {
                yield break;
            }

            while (!photonView)
            {
                yield return null;
            }
            
            photonView.ObservedComponents.Add(this);

            MasterLoading.SpectatorJoined += ResendRpcCalls;
        }

        protected virtual void ResendRpcCalls()
        {
            if (!IsMultiplayerAndMasterClient)
            {
                return;
            }
        }

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
        }
    }
}
