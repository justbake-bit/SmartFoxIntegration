using System;
using System.Security.Cryptography;
using UnityEngine;

namespace justbake.sfi {
    public static class Utils {

        public static uint GetTrueRandomUInt() {
            // use Crypto RNG to avoid having time based duplicates
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        // simplified IsSceneObject check from Mirror II
        public static bool IsSceneObject(NetworkIdentity identity) {
            // original UNET / Mirror still had the IsPersistent check.
            // it never fires though. even for Prefabs dragged to the Scene.
            // (see Scene Objects example scene.)
            // #if UNITY_EDITOR
            //             if (UnityEditor.EditorUtility.IsPersistent(identity.gameObject))
            //                 return false;
            // #endif

            return identity.gameObject.hideFlags != HideFlags.NotEditable &&
                identity.gameObject.hideFlags != HideFlags.HideAndDontSave &&
                identity.SceneId != 0;
        }

    }
}