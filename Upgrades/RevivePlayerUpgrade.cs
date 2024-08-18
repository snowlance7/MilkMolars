using System;
using System.Collections.Generic;
using System.Text;

namespace MilkMolars.Upgrades
{
    internal class RevivePlayerUpgrade : MilkMolarUpgrade
    {
        public override void ActivateRepeatableUpgrade()
        {
            base.ActivateRepeatableUpgrade();
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            
        }
    }
}
