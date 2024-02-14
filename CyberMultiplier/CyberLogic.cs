using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using UnityEngine;

namespace CyberMultiplier
{
    // Class containing the grids and EndlessGrids in the world
    internal abstract class CyberLogic
    {
        private static List<GameObject> grids;
        private static Dictionary<EndlessGrid, GameObject> egToGrid;
        private static Dictionary<EndlessGrid, bool> finishedGrids;
        private static Timer allowTimer = new Timer(5000);

        public static void InitContainers()
        {
            grids = new List<GameObject>();
            egToGrid = new Dictionary<EndlessGrid, GameObject>();
            finishedGrids = new Dictionary<EndlessGrid, bool>();
        }

        public static void StartTimer()
        {
            if (allowTimer.Enabled)
            {
                allowTimer.Stop();
                allowTimer.Dispose();
            }
            allowTimer = new Timer(5000);
            allowTimer.Elapsed += CyberManager.CalculateAllow;
            allowTimer.Start();
        }

        public static void StopTimer()
        {
            if (allowTimer.Enabled)
            {
                allowTimer.Stop();
                allowTimer.Dispose();
            }
        }

        public static void AddGrid(GameObject grid) { grids.Add(grid); }

        public static GameObject GetGrid(int index) => grids[index];

        public static GameObject GetGrid(EndlessGrid eg) => egToGrid[eg];

        public static int GridsCount() => grids.Count;

        public static EndlessGrid GetEG(int index) => grids[index].transform.GetChild(2).GetComponent<EndlessGrid>();

        public static EndlessGrid GetEG(GameObject grid) => grid.transform.GetChild(2).GetComponent<EndlessGrid>();

        public static void AddEGMap(EndlessGrid eg, GameObject grid)
        {
            if (!egToGrid.ContainsKey(eg)) egToGrid.Add(eg, grid);
        }

        public static int EGMapCount() => egToGrid.Count;

        public static void RegisterGrid(GameObject grid)
        {
            AddGrid(grid);
            AddEGMap(GetEG(grid), grid);
        }

        public static void RemoveGrid(int index)
        {
            egToGrid.Remove(GetEG(index));
            grids.RemoveAt(index);
        }

        public static void RemoveGrid(GameObject grid)
        {
            egToGrid.Remove(GetEG(grid));
            grids.Remove(grid);
        }

        public static void AddEGStatus(EndlessGrid eg, bool status)
        {
            if (!egToGrid.ContainsKey(eg)) finishedGrids.Add(eg, status);
        }

        public static void SetEGStatus(EndlessGrid eg, bool status) { finishedGrids[eg] = status; }

        public static bool GetEGStatus(EndlessGrid eg) => finishedGrids[eg];

        public static EndlessGrid[] GetEGStatusesKeys()
        {
            EndlessGrid[] egs = new EndlessGrid[finishedGrids.Count];
            finishedGrids.Keys.CopyTo(egs, 0);
            return egs;
        }

        public static int EGStatusesCount() => finishedGrids.Count;
    }
}
