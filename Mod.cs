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
        private static ClosedBoltWeapon thisClosedBoltWeapon;
        private static float? newCurrentRotation;
        
        private void Awake()
        {
            Debug.Log("Patch started");
            Harmony.CreateAndPatchAll(typeof(Mod));
            Debug.Log("Patch worked");
            
            
            // When the player grabs tghe weapon in the proper place
            // Check for when the press the up key, if they do
            // If it is typeOf ClosedBolt, call ReleaseBolt();
        }

        private void Update()
        {
            if (checkForPlayerPress)
            {
                // Do input stuff
            }
        }

        [HarmonyPatch(typeof(FVRInteractiveObject), "BeginInteraction")]
        [HarmonyPrefix]
        private static void CloseBoltWeaponPatch(FVRInteractiveObject __instance)
        {
            Debug.Log("");
            Debug.Log("");
            Debug.Log("Player is hold the weapon");

            if (__instance.GetType() == typeof(FVRAlternateGrip))
            {
                Debug.Log("This is an alternate grip");

                var thisObject = __instance as FVRAlternateGrip;
                
                Debug.Log("This primary objects name :  " + thisObject.PrimaryObject.name);

                if (thisObject != null && thisObject.PrimaryObject is ClosedBoltWeapon closedBoltWeapon)
                {
                    Debug.Log("Parent is close bolt");

                    var boltHandle = closedBoltWeapon.Handle;

                    //Exits the method if the bolt is not in correct position 
                    if (boltHandle.CurPos != ClosedBoltHandle.HandlePos.Locked)
                    {
                        Debug.Log("Bolt is not locked back, so therefore we return");
                        return;
                    }
                    

                    var number = boltHandle.Rot_Standard;
                        
                    Debug.Log("number: " + number);
                    
                    closedBoltWeapon.Bolt.ReleaseBolt();
                    boltHandle.transform.localEulerAngles = new Vector3(0, 0, number);
                    boltHandle.CurPos = ClosedBoltHandle.HandlePos.Rear;
                        
                    // Sets the private m_currentRot field

                    thisClosedBoltWeapon = closedBoltWeapon;
                    newCurrentRotation = number;
                }
            }
        }

        [HarmonyPatch(typeof(ClosedBoltHandle), "UpdateHandle")]
        [HarmonyPrefix]
        private static void ClosedBoltUpdatePatch(ClosedBolt __instance, float ___m_curSpeed)
        {
            Debug.Log("Current speed: " + ___m_curSpeed);
            
            if (thisClosedBoltWeapon == null && newCurrentRotation == null)
            {
                Debug.Log("both are null");
                return;
            }
            
            
            
        }
    }
}
