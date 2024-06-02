//#define SUPPORT_LEVEL_BUNDLE // Managed via build mod tool.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using Harmony;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("First person camera (Caps Lock)")] // ENTER MOD TITLE


public class ModEntryPoint : MonoBehaviour // ModEntryPoint - RESERVED LOOKUP NAME
{
    static bool fpsView = false;

    void ScriptsPatch()
    {
        //Debug.Log("!!!Patch begin");
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;

        var harmony = HarmonyInstance.Create("com.atomrpg.mod." + modName);
        harmony.PatchAll();
        //Debug.Log("!!!Patch end");
    }

    [HarmonyPatch(typeof(CameraControl))]
    [HarmonyPatch("PositionUpdate")]
    class Patch_CameraControl_PositionUpdate
    {
        static bool Prefix(CameraControl __instance)
        {
            return !fpsView;
        }
    }

    [HarmonyPatch(typeof(PlayerControl))]
    [HarmonyPatch("OnGUI")]
    class Patch_PlayerControl_OnGUI
    {
        static bool Prefix(PlayerControl __instance)
        {
            if (fpsView)
            {
                Event e = Event.current;

                if(e.type == EventType.MouseDown)
                {
                    AccessTools.Method(typeof(PlayerControl), "ProcessInput").Invoke(__instance, new object[] { true });
                }
                else if (e.type == EventType.MouseUp)
                {
                    AccessTools.Method(typeof(PlayerControl), "ProcessInput").Invoke(__instance, new object[] { false });
                }
                else
                {

                    if (__instance.HasManualTarget || (Game.World.battle.IsOwnTurn(__instance.CharacterComponent) && Game.World.battle.InBattle))
                    {
                        if (!Game.World.HUD.HasModal())
                            AccessTools.Method(typeof(PlayerControl), "ProcessConsoleMalualTarget").Invoke(__instance, null);

                        AccessTools.Method(typeof(PlayerControl), "ProcessSelection").Invoke(__instance, null);
                        AccessTools.Method(typeof(PlayerControl), "UpdateConsoleCursor").Invoke(__instance, null);
                    }
                    else
                    {
                        AccessTools.Method(typeof(PlayerControl), "ProcessSelection").Invoke(__instance, null);

                        __instance.ResetManualTarget();

                        if (!Game.World.battle.InBattle)
                        {
                            //_battleTarget = null;
                        }

                        if (Game.World.HUD.m_consoleCursor != null)
                        {
                            Game.World.HUD.m_consoleCursor.Hide();
                        }
                    }
                }
            }

            return !fpsView;
        }
    }

    [HarmonyPatch(typeof(CameraControl))]
    [HarmonyPatch("GetInput")]
    class Patch_CameraControl_GetInput
    {
        private static float _freecamYaw = 0.0f;
        private static float _freecamPitch = 0.0f;

        static bool Prefix(CameraControl __instance)
        {
            if(!fpsView)
            {
                return true;
            }

            float dx = 0;
            float dy = 0;

            if (Game.World.HUD.HasModal())
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;

                float dt = Time.smoothDeltaTime;
                var mX = Input.GetAxis("Mouse X");
                var mY = -Input.GetAxis("Mouse Y");

                _freecamYaw += mX * dt * 100;
                _freecamPitch += mY * dt * 100;

                _freecamPitch = Mathf.Clamp(_freecamPitch, -90, 90);

                Quaternion freecamDesiredRot = Quaternion.Euler(_freecamPitch, _freecamYaw, 0);

                var transform = Game.World.cameraControl.transform;
                transform.rotation = Quaternion.Slerp(transform.rotation, freecamDesiredRot, dt * 10);
                transform.position = Game.World.Player.CharacterComponent.CameraTarget.transform.position;

                if (InputManager.GetKey(InputManager.Action.Camera_W))
                {
                    dy += 1;
                }

                if (InputManager.GetKey(InputManager.Action.Camera_S))
                {
                    dy -= 1;
                }

                if (InputManager.GetKey(InputManager.Action.Camera_A))
                {
                    dx -= 1;
                }

                if (InputManager.GetKey(InputManager.Action.Camera_D))
                {
                    dx += 1;
                }
            }

  
            Game.World.Player.CharacterComponent.ForceRun = Input.GetKey(KeyCode.LeftShift);

            Game.World.Player.CharacterComponent.SetDirectionalMove(dx, dy);

            return false;
        } 
    }

    static void SetShadowOnly(GameObject go, bool value)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.shadowCastingMode = value ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }

    void Start()
    {
        var assembly = GetType().Assembly;
        Debug.Log("Mod Init: " + assembly.GetName().Name + "(" + System.IO.Path.GetDirectoryName(assembly.Location) + ")");
        ScriptsPatch();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.CapsLock))
        {
            fpsView = !fpsView;
            UpdateView();
        }
    }

    private void UpdateView()
    {
        Cursor.lockState = fpsView ? CursorLockMode.Locked : CursorLockMode.Confined;
        SetShadowOnly(Game.World.Player.CharacterComponent.gameObject, fpsView);
    }
}