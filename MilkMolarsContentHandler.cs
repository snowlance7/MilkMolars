using Dusk;
using UnityEngine;

namespace MilkMolars
{
    public class MilkMolarsContentHandler : ContentHandler<MilkMolarsContentHandler>
    {
        public class MilkMolarsAssetsAssets(DuskMod mod, string filePath) : AssetBundleLoader<MilkMolarsAssetsAssets>(mod, filePath)
        {
            [LoadFromBundle("NetworkHandler.prefab")]
            public GameObject NetworkHandlerPrefab { get; private set; } = null!;

            [LoadFromBundle("NotificationOverlay.prefab")]
            public GameObject NotificationOverlayPrefab { get; private set; } = null!;
        }
        public MilkMolarsAssetsAssets? MilkMolarsAssets;

        public MilkMolarsContentHandler(DuskMod mod) : base(mod)
        {
            RegisterContent("milkmolars_assets", out MilkMolarsAssets);
        }
    }

}