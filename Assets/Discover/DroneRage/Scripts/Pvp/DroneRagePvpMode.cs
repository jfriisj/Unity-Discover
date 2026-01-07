// Copyright (c) Meta Platforms, Inc. and affiliates.

using Discover.DroneRage.Bootstrapper;

namespace Discover.DroneRage.Pvp
{
    /// <summary>
    /// Helper to detect if the current DroneRage session is in PvP mode.
    /// </summary>
    public static class DroneRagePvpMode
    {
        public const string PVP_APP_NAME = "dronerage-pvp";

        /// <summary>
        /// Returns true if the active application is dronerage-pvp.
        /// </summary>
        public static bool IsPvpMode()
        {
            var container = DroneRageAppContainerUtils.GetAppContainer();
            return container != null && container.AppName == PVP_APP_NAME;
        }
    }
}
