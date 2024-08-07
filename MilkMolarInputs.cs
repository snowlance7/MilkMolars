using System;
using System.Collections.Generic;
using System.Text;
using LethalCompanyInputUtils;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace MilkMolars
{
    public class MilkMolarInputs : LcInputActions
    {
        public static MilkMolarInputs Instance;

        public static void Init()
        {
            Instance = new MilkMolarInputs();
        }

        [InputAction(KeyboardControl.M, Name = "OpenSCPInventoryUI")]
        public InputAction OpenUIKey { get; set; }
    }
}
