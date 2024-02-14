using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CyberMultiplier
{
    internal class CyberGrindPatch
    {
        // Don't re-activate if you are already
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EndlessGrid), "OnTriggerEnter")]
        private static bool ActivateCheck(EndlessGrid __instance)
        {
            return __instance.GetComponent<Collider>().enabled;
        }

        // Activate others if you have the setting enabled
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EndlessGrid), "OnTriggerEnter")]
        private static void StartOthers(EndlessGrid __instance)
        {
            if (Plugin.syncIn.Value) CyberManager.StartGrids();
            CyberManager.setFinished(__instance, false);
            CyberManager.disable(__instance);
        }

        // Big PreFix to sync the wave changing
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EndlessGrid), "Update")]
        private static bool WaitOthers(EndlessGrid __instance, ref int ___enemyAmount, ref ActivateNextWave ___anw)
        {
            if (!Plugin.syncOut.Value) return true;
            if (__instance.GetComponent<Collider>().enabled) return true;
            if (___anw.deadEnemies >= ___enemyAmount) {
                CyberManager.setFinished(__instance, true);

                // if you finished and others have not finished wait
                if (!CyberManager.otherFinished()) return false;
            }
            return true;
        }

        // If the setting is enabled count all enemies
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EndlessGrid), "Update")]
        private static void Enemies(EndlessGrid __instance, ref Text ___enemiesLeftText) {
            if (!__instance.Equals(CyberLogic.GetGrid(0)) || !Plugin.screenCram.Value) return;
            var list = EnemyTracker.Instance.enemies;
            list.RemoveAll(e => e == null || e.dead);
            ___enemiesLeftText.text = list.Count.ToString();
        }
    }
}
