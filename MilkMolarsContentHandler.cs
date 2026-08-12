using Dusk;
using UnityEngine;

namespace MilkMolars
{
    public class MilkMolarsContentHandler : ContentHandler<MilkMolarsContentHandler>
    {
        public class MilkMolarsAssetsAssets(DuskMod mod, string filePath) : AssetBundleLoader<MilkMolarsAssetsAssets>(mod, filePath)
        {
            [LoadFromBundle("NotificationOverlay.prefab")]
            public GameObject NotificationOverlayPrefab { get; private set; } = null!;
        }
        public MilkMolarsAssetsAssets? MilkMolarsAssets;

        public class MilkMolarAssets(DuskMod mod, string filePath) : AssetBundleLoader<MilkMolarAssets>(mod, filePath) { }
        public MilkMolarAssets? MilkMolar;

        public class MegaMilkMolarAssets(DuskMod mod, string filePath) : AssetBundleLoader<MegaMilkMolarAssets>(mod, filePath) { }
        public MegaMilkMolarAssets? MegaMilkMolar;

        public MilkMolarsContentHandler(DuskMod mod) : base(mod)
        {
            RegisterContent("milkmolars_assets", out MilkMolarsAssets);
            RegisterContent("milkmolar", out MilkMolar);
            RegisterContent("megamilkmolar", out MegaMilkMolar);
        }
    }

}