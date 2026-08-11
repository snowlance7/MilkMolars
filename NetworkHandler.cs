//using GameNetcodeStuff;
//using HarmonyLib;
//using SnowyLib;
//using Unity.Netcode;
//using UnityEngine;
//using static MilkMolars.Plugin;

//namespace MilkMolars
//{
//    internal class NetworkHandler : NetworkBehaviour
//    {
//        public static NetworkHandler Instance { get; private set; } = null!;

//        public Sprite MilkMolarUIIcon = null!;
//        public Sprite MegaMilkMolarUIIcon = null!;


//        public override void OnNetworkSpawn()
//        {
//            if (IsServer && Instance != null)
//                Instance.gameObject.GetComponent<NetworkObject>().Despawn(destroy: true);
//            Instance = this;
//            logger.LogDebug("NetworkHandler spawned");
//            base.OnNetworkSpawn();
//        }
//    }

//    [HarmonyPatch]
//    public class NetworkHandlerPatches
//    {
//        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
//        static void AwakePostFix()
//        {
//            if (!IsServerOrHost) { return; }
//            var networkHandlerHost = UnityEngine.Object.Instantiate(ItemSCPsContentHandler.Instance.ItemSCPsAssets?.NetworkHandlerPrefab, Vector3.zero, Quaternion.identity);
//            networkHandlerHost?.GetComponent<NetworkObject>().Spawn();
//        }
//    }
//}