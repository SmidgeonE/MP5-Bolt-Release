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

        private static bool _openBoltTrigger;
        private static bool _closeBoltTrigger;
        private static bool _changePrivVariablesTrigger;

        private static FVRViveHand interactingHand;
        private static HandInput _fvrHandInput;
        private static ControlOptions.CoreControlMode _controlMode;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Mod));
        }

        private void Update()
        {
            bool isPressingReleaseButton;
            bool isPressingLockButton;

            // Gets the input from the player based on if it is streamlined or not.
            if (_controlMode == ControlOptions.CoreControlMode.Standard)
            {
                isPressingReleaseButton = _fvrHandInput.TouchpadNorthPressed && _fvrHandInput.TouchpadPressed &&
                                          _openBoltTrigger;
                isPressingLockButton = _fvrHandInput.TouchpadSouthPressed && _fvrHandInput.TouchpadPressed &&
                                       _closeBoltTrigger;
            }
            else
            {
                isPressingReleaseButton = _fvrHandInput.BYButtonPressed && _openBoltTrigger;
                isPressingLockButton = _fvrHandInput.TriggerPressed && _closeBoltTrigger;
            }

            if (isPressingReleaseButton)
            {
                Console.WriteLine("opening bolt");
                OpenBolt();
                _openBoltTrigger = false;
                _changePrivVariablesTrigger = true;
            }
            else if (isPressingLockButton)
            {
                Console.WriteLine("closing bolt");
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
            interactingHand = __instance;
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
                if (boltHandle.CurPos == ClosedBoltHandle.HandlePos.Locked)
                {
                    // Sets the private m_currentRot field
                    _thisClosedBoltWeapon = closedBoltWeapon;
                    _newCurrentRotation = boltHandle.Rot_Standard;
                    _openBoltTrigger = true;
                }
                else
                {
                    _thisClosedBoltWeapon = closedBoltWeapon;
                    _newCurrentRotation = boltHandle.Rot_Safe;
                    _closeBoltTrigger = true;
                }
            }
        }

        [HarmonyPatch(typeof(FVRInteractiveObject), "EndInteraction")]
        [HarmonyPrefix]
        private static void EndInteractionPatch()
        {
            _openBoltTrigger = false;
            _closeBoltTrigger = false;
            _thisClosedBoltWeapon = null;
            _changePrivVariablesTrigger = false;
        }

        private static void OpenBolt()
        {
            var boltHandle = _thisClosedBoltWeapon.Handle;
            
            _thisClosedBoltWeapon.Bolt.ReleaseBolt();
            
            if (_newCurrentRotation != null)
                boltHandle.transform.localEulerAngles = new Vector3(0, 0, _newCurrentRotation.Value);
            
            boltHandle.CurPos = ClosedBoltHandle.HandlePos.Rear;
        }

        private static void CloseBolt()
        {
            var boltHandle = _thisClosedBoltWeapon.Handle;
            
            _thisClosedBoltWeapon.Bolt.LockBolt();

            if (_newCurrentRotation != null)
                boltHandle.transform.localEulerAngles = new Vector3(0, 0, _newCurrentRotation.Value);

            boltHandle.CurPos = ClosedBoltHandle.HandlePos.LockedToRear;
        }

        [HarmonyPatch(typeof(ClosedBoltHandle), "UpdateHandle")]
        [HarmonyPrefix]
        private static void ClosedBoltUpdatePatch(ClosedBolt __instance, ref float ___m_curSpeed, ref float ___m_currentRot, ref bool ___m_isAtLockAngle)
        {
            if (_thisClosedBoltWeapon == null || _newCurrentRotation == null || _changePrivVariablesTrigger == false) return;
            if (_thisClosedBoltWeapon != __instance.Weapon) return;
            
            ___m_currentRot = _newCurrentRotation.Value;
            _newCurrentRotation = null;
            
            if (_closeBoltTrigger)
            {
                Console.WriteLine("closing bolt");
                ___m_isAtLockAngle = true;
            }
            else if (_openBoltTrigger)
            {
                Console.WriteLine("opening bolt");
                ___m_isAtLockAngle = false;
                ___m_curSpeed = 0.1f;
            }
            
            _changePrivVariablesTrigger = false;
            
            /* this resets this entire mod */
            if (interactingHand == null)
            {
                Console.WriteLine("inteactong hand is null");
                return;
            }

            var alternateGrip = interactingHand.CurrentInteractable;
            interactingHand.CurrentInteractable.ForceBreakInteraction();
            Console.WriteLine("interaction broken");
            alternateGrip.BeginInteraction(interactingHand);
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
