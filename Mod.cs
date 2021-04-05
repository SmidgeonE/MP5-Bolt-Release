using System;
using BepInEx;
using FistVR;
using HarmonyLib;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem.Sample;

namespace MP5_Bolt_Release
{
    [BepInPlugin("smack", "smeep", "1.0.0")]
    public class Mod : BaseUnityPlugin
    {
        private static ClosedBoltWeapon thisClosedBoltWeapon;
        private static float? newCurrentRotation;

        private static bool closeBoltTrigger;
        private static bool changePrivVariablesTrigger;

        private static HandInput _fvrHandInput;

        private void Awake()
        {
            Debug.Log("Patch started");
            Harmony.CreateAndPatchAll(typeof(Mod));
            Debug.Log("Patch worked");
        }

        private void Update()
        {
            if (_fvrHandInput.TouchpadNorthPressed && _fvrHandInput.TouchpadPressed)
            {
                Debug.Log("PRessing");
            }
            
            

            if (_fvrHandInput.TouchpadNorthPressed && _fvrHandInput.TouchpadPressed && closeBoltTrigger)
            {
                Debug.Log("Closing Bolt");

                CloseBolt();
                closeBoltTrigger = false;
                changePrivVariablesTrigger = true;
            }
        }

        // Gets the Input values
        [HarmonyPatch(typeof(FVRViveHand), "Update")]
        [HarmonyPostfix]
        private static void FVRViveHandUpdatePatch(FVRViveHand __instance)
        {
            if (__instance.IsThisTheRightHand) return;
            _fvrHandInput = __instance.Input;
        }
        
        [HarmonyPatch(typeof(FVRInteractiveObject), "BeginInteraction")]
        [HarmonyPrefix]
        private static void CloseBoltWeaponPatch(FVRInteractiveObject __instance)
        {
            if (__instance.GetType() != typeof(FVRAlternateGrip)) return;

            var thisObject = __instance as FVRAlternateGrip;
            if (thisObject == null) return;

            if (thisObject != null && thisObject.PrimaryObject is ClosedBoltWeapon closedBoltWeapon)
            {
                var boltHandle = closedBoltWeapon.Handle;

                //Exits the method if the bolt is not in correct position 
                if (boltHandle.CurPos != ClosedBoltHandle.HandlePos.Locked)
                {
                    Debug.Log("Bolt is not locked back, so therefore we return");
                    return;
                }
                
                var number = boltHandle.Rot_Standard;
                
                // Sets the private m_currentRot field
                thisClosedBoltWeapon = closedBoltWeapon;
                newCurrentRotation = number;

                closeBoltTrigger = true;
            }
        }

        private static void CloseBolt()
        {
            var boltHandle = thisClosedBoltWeapon.Handle;
            
            thisClosedBoltWeapon.Bolt.ReleaseBolt();
            
            if (newCurrentRotation != null)
                boltHandle.transform.localEulerAngles = new Vector3(0, 0, newCurrentRotation.Value);
            boltHandle.CurPos = ClosedBoltHandle.HandlePos.Rear;
        }

        [HarmonyPatch(typeof(ClosedBoltHandle), "UpdateHandle")]
        [HarmonyPrefix]
        private static void ClosedBoltUpdatePatch(ClosedBolt __instance, ref float ___m_curSpeed, ref float ___m_currentRot, ref bool ___m_isAtLockAngle)
        {
            if (thisClosedBoltWeapon == null || newCurrentRotation == null || changePrivVariablesTrigger == false) return;
            if (thisClosedBoltWeapon != __instance.Weapon) return;

            Debug.Log("Changing priv variables");
            ___m_currentRot = newCurrentRotation.Value;
            newCurrentRotation = null;
            ___m_isAtLockAngle = false;
            ___m_curSpeed = 0.1f;
            changePrivVariablesTrigger = false;
        }
    }
}
