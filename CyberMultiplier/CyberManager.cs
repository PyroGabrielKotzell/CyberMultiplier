using BepInEx.Logging;
using System.Reflection;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyberMultiplier
{
    // Class for most calculations and world management
    internal class CyberManager : MonoSingleton<CyberManager>
    {
        private static bool gridsStarting = false;
        private static bool allowNextWaxe = false;

        private static bool logs;
        private static ManualLogSource mls;

        // Initialize the listener for the entrance to the GooberGrind
        public static void Listen()
        {
            logs = Plugin.logGrids.Value;
            mls = Plugin.mls;

            if (Plugin.multiplier.Value <= 1 && !Plugin.allInvValues.Value) return;
            if (Plugin.multiplier.Value == 1) return;

            // attach at sceneloaded a listener that acts only if the scene is "Endless"
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (SceneHelper.CurrentScene == "Endless")
                {

                    // reset timer
                    if (Plugin.syncOut.Value)
                    {
                        if (logs) mls.LogInfo("Starting timer");
                        CyberLogic.StartTimer();
                    }

                    // clear old arena pointers and start the process of creation
                    CyberLogic.InitContainers();
                    Initialize();
                }
                else if (SceneHelper.CurrentScene == "Main menu")
                {

                    // stop timer if Main menu is loaded 
                    if (logs) mls.LogInfo("Stopping timer");
                    CyberLogic.StopTimer();
                }
            };
        }

        // Initialize method to make the HooperGrind grids (arenas)
        private static void Initialize()
        {

            // add the arena GameObject and stop if you didn't find it (if it is null)
            CyberLogic.RegisterGrid(GameObject.Find("Everything"));
            if (CyberLogic.GetGrid(0) == null)
            {
                mls.LogError("The CyberGrind GameObject could not be found!");
                return;
            }

            // start the process of creation getting the value given
            int n = Plugin.multiplier.Value;

            for (int i = -n / 2; i < n / 2f; i++)
            {
                for (int j = -n / 2; j < n / 2f; j++)
                {

                    // make a clone of the first GoombaGrind
                    GameObject clone = Instantiate(CyberLogic.GetGrid(0));

                    // Make platform blocks disappear
                    clone.transform.GetChild(0).gameObject.SetActive(false);
                    clone.transform.GetChild(1).gameObject.SetActive(false);

                    // move it, save it and print a log
                    clone.transform.position = new Vector3(80 * i, 0, 80 * j);
                    CyberLogic.RegisterGrid(clone);
                    if (logs) mls.LogInfo("New grid created at: " + clone.transform.position.x + " 0 " + clone.transform.position.z);
                }
            }

            // delete the copy at 0 0 0
            if (logs) mls.LogInfo("Deleting clone grid at 0 0 0");
            int index = n * n / 2 + (n % 2 == 0 ? 1 + n / 2 : 1);
            SceneHelper.Destroy(CyberLogic.GetGrid(index));
            CyberLogic.RemoveGrid(index);
        }

        // Start all FumoGrind grids (big lag expected)
        public static void StartGrids()
        {
            if (gridsStarting) return;
            gridsStarting = true;
            if (logs) mls.LogInfo("Starting grids...");

            // call starting grid OnTriggerEnter
            EndlessGrid eg = CyberLogic.GetEG(0);

            // run music for original grid
            CyberLogic.GetGrid(0).transform.GetChild(3).gameObject.SetActive(true);

            // collider with player tag
            GameObject c = new GameObject { tag = "Player" };
            c.AddComponent<BoxCollider>();

            // invoke grid collide
            MethodInfo mt = eg.GetType().GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            mt.Invoke(eg, new Object[] { c.GetComponent<BoxCollider>() });
            if (logs) mls.LogInfo("Grid started: 0");

            // for each cloned grid call the OnTriggerEnter
            for (int i = CyberLogic.GridsCount() - 1; i > 0; i--)
            {
                eg = CyberLogic.GetEG(i);
                mt = eg.GetType().GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);
                mt.Invoke(eg, new Object[] { c.GetComponent<BoxCollider>() });

                if (logs) mls.LogInfo("Grid started: " + i);
            }
            gridsStarting = false;
        }

        // Set EndlessGrid as finished
        public static void setFinished(EndlessGrid eg, bool val)
        {
            CyberLogic.AddEGStatus(eg, val);
            CyberLogic.SetEGStatus(eg, val);
        }
        
        // Did other EndlessGrid(s) finish the enemies
        public static bool otherFinished()
        {
            if (gridsStarting) return true;
            return allowNextWaxe;
        }

        // Every once in a while calculate if everyone can start the next wave
        internal static void CalculateAllow(object sender, ElapsedEventArgs e)
        {
            bool allow = true;
            EndlessGrid[] egs = CyberLogic.GetEGStatusesKeys();
            for (int i = CyberLogic.EGStatusesCount() - 1; i >= 0; i--)
            {
                allow = allow && CyberLogic.GetEGStatus(egs[i]);
                CyberLogic.SetEGStatus(egs[i], false);
            }
            allowNextWaxe = allow;
        }

        public static void disable(EndlessGrid eg)
        {
            
            // un-activate music and display except for the original one
            GameObject gameobj = CyberLogic.GetGrid(eg);
            if (gameobj.Equals(CyberLogic.GetGrid(0))) return;
            gameobj.transform.GetChild(3).GetChild(0).gameObject.SetActive(false);
            gameobj.transform.GetChild(4).gameObject.SetActive(false);
        }
    }
}
