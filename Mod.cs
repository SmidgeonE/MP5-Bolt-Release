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

        [HarmonyPatch(typeof(FVRInteractiveObject), "BeginInteraction")]
        [HarmonyPrefix]
        private static void CloseBoltWeaponPatch(FVRInteractiveObject __instance)
        {
            Debug.Log("");
            Debug.Log("");
            Debug.Log("Player is hold the weapon");
            
            
            Debug.Log("objecst name : " + __instance.name);
            Debug.Log("object type : " + __instance.GetType());
            Debug.Log("parents name : " + __instance.transform.parent.gameObject.name);
            Debug.Log("parents type : " + __instance.transform.parent.gameObject.GetType());

            if (__instance.GetType() == typeof(FVRAlternateGrip))
            {
                Debug.Log("This is an alternate grip");

                var thisObject = __instance as FVRAlternateGrip;
                
                Debug.Log("This primary objects name :  " + thisObject.PrimaryObject.name);
                
                

                if (thisObject != null && thisObject.PrimaryObject is ClosedBoltWeapon closedBoltWeapon)
                {
                    Debug.Log("Parent is close bolt");

                    var bolt = closedBoltWeapon.Bolt;
                    Debug.Log("Updating Bolt");
                    bolt.UpdateBolt();
                }
            }
        }
    }
}