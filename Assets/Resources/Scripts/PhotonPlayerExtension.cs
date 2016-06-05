using UnityEngine;
using System.Collections;

namespace PhotonPlayerExtension {

    public static class PhotonPlayerExtension {

        public static bool IsInATeam (this PhotonPlayer player) {
            if (player.CurrentTeamId () == -1) {
                return false;
            }
            return true;
        }

        public static int CurrentTeamId (this PhotonPlayer player) {
            if (!player.customProperties.ContainsKey ("team")) {
                return -1;
            }
            return (byte) player.customProperties["team"];
        }

        public static void LeaveTeam (this PhotonPlayer player) {
            player.Unready ();

            ExitGames.Client.Photon.Hashtable teamHashtable = new ExitGames.Client.Photon.Hashtable ();
            teamHashtable["team"] = null;
            player.SetCustomProperties (teamHashtable);
        }

        public static void JoinTeam (this PhotonPlayer player, int teamId) {
            ExitGames.Client.Photon.Hashtable teamHashtable = new ExitGames.Client.Photon.Hashtable ();
            teamHashtable["team"] = (byte) teamId;
            player.SetCustomProperties (teamHashtable);
        }

        public static bool IsReady (this PhotonPlayer player) {
            if (!player.customProperties.ContainsKey ("ready")) {
                return false;
            }
            return (bool) player.customProperties["ready"];
        }

        public static void Ready (this PhotonPlayer player) {
            ExitGames.Client.Photon.Hashtable readyHashTable = new ExitGames.Client.Photon.Hashtable ();
            readyHashTable["ready"] = (bool) true;
            player.SetCustomProperties (readyHashTable);
        }

        public static void Unready (this PhotonPlayer player) {
            ExitGames.Client.Photon.Hashtable readyHashTable = new ExitGames.Client.Photon.Hashtable ();
            readyHashTable["ready"] = (bool) false;
            player.SetCustomProperties (readyHashTable);
        }

    }

}
