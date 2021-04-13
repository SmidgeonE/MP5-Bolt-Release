using System;
using BepInEx;
using FistVR;
using HarmonyLib;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem.Sample;

namespace MP5_Bolt_Release
{
    [BepInPlugin("MP5_Bolt_Release", "MP5_Bolt_Release", "1.1.0")]
    public class Mod : BaseUnityPlugin
    {
        private static ClosedBoltWeapon _thisClosedBoltWeapon;
        private static float? _newCurrentRotation;

        private static bool _closeBoltTrigger;
        private static bool _changePrivVariablesTrigger;

        private static HandInput _fvrHandInput;
        private static ControlOptions.CoreControlMode _controlMode;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Mod));
        }

        private void Update()
        {
            bool isPressed;

            // Gets the input from the player based on if it is streamlined or not.
            if (_controlMode == ControlOptions.CoreControlMode.Standard)
                isPressed = _fvrHandInput.TouchpadNorthPressed && _fvrHandInput.TouchpadPressed && _closeBoltTrigger;
            else
                isPressed = _fvrHandInput.BYButtonPressed && _closeBoltTrigger;
            
            if (isPressed)
            {
                CloseBolt();
                _closeBoltTrigger = false;
                _changePrivVariablesTrigger = true;
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
                if (closedBoltWeapon.Handle == null) return;

                var boltHandle = closedBoltWeapon.Handle;

                //Exits the method if the bolt is not in correct position 
                if (boltHandle.CurPos != ClosedBoltHandle.HandlePos.Locked) return;

                var number = boltHandle.Rot_Standard;
                
                // Sets the private m_currentRot field
                _thisClosedBoltWeapon = closedBoltWeapon;
                _newCurrentRotation = number;

                _closeBoltTrigger = true;
            }
        }

        [HarmonyPatch(typeof(FVRInteractiveObject), "EndInteraction")]
        [HarmonyPrefix]
        private static void EndInteractionPatch()
        {
            _closeBoltTrigger = false;
            _thisClosedBoltWeapon = null;
            _changePrivVariablesTrigger = false;
        }

        private static void CloseBolt()
        {
            var boltHandle = _thisClosedBoltWeapon.Handle;
            
            _thisClosedBoltWeapon.Bolt.ReleaseBolt();
            
            if (_newCurrentRotation != null)
                boltHandle.transform.localEulerAngles = new Vector3(0, 0, _newCurrentRotation.Value);
            
            boltHandle.CurPos = ClosedBoltHandle.HandlePos.Rear;
        }

        [HarmonyPatch(typeof(ClosedBoltHandle), "UpdateHandle")]
        [HarmonyPrefix]
        private static void ClosedBoltUpdatePatch(ClosedBolt __instance, ref float ___m_curSpeed, ref float ___m_currentRot, ref bool ___m_isAtLockAngle)
        {
            if (_thisClosedBoltWeapon == null || _newCurrentRotation == null || _changePrivVariablesTrigger == false) return;
            if (_thisClosedBoltWeapon != __instance.Weapon) return;
            
            ___m_currentRot = _newCurrentRotation.Value;
            _newCurrentRotation = null;
            ___m_isAtLockAngle = false;
            ___m_curSpeed = 0.1f;
            _changePrivVariablesTrigger = false;
        }

        
        /* These patches get the current control mode */
        [HarmonyPatch(typeof(GameOptions), "InitializeFromSaveFile")]
        [HarmonyPrefix]
        private static void InitialOptionsGrabPatch(GameOptions __instance)
        {
            _controlMode = __instance.ControlOptions.CCM;
        }
        
        [HarmonyPatch(typeof(GameOptions), "SaveToFile")]
        [HarmonyPrefix]
        private static void UpdateControlOptionsPatch(GameOptions __instance)
        {
            _controlMode = __instance.ControlOptions.CCM;
        }
    }
}
