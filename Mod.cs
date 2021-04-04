using System;
using BepInEx;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace MP5_Bolt_Release
{
    [BepInPlugin("smack", "smeep", "1.0.0")]
    public class Mod : BaseUnityPlugin
    {
        private static bool checkForPlayerPress;
        
        private void Awake()
        {
            Debug.Log("Patch started");
            Harmony.CreateAndPatchAll(typeof(Mod));
            Debug.Log("Patch worked");
            
            
            // When the player grabs tghe weapon in the proper place
            // Check for when the press the up key, if they do
            // If it is typeOf ClosedBolt, call ReleaseBolt();
            
            
        }
    }
}