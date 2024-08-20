using UnityEngine;

namespace _Game.Scripts {
    public static class SaveManager {
        private const string TOKEN = "SAVE_TOKEN_";
        private const int FALSE = 0;
        private const int TRUE = 100;
        
        public static int GetInt(IntData data, int? defaultValue) {
            return PlayerPrefs.GetInt(TOKEN + data, defaultValue ?? 0);
        }
        
        public static void SetInt(IntData data, int value) {
            PlayerPrefs.SetInt(TOKEN + data, value);
        }
        
        public static bool GetBool(BoolData data) {
            return PlayerPrefs.GetInt(TOKEN + data, FALSE) == TRUE;
        }
        
        public static void SetBool(BoolData data, bool value) {
            PlayerPrefs.SetInt(TOKEN + data, value ? TRUE : FALSE);
        }

        public enum IntData {
            LastCompletedLevel
        }

        public enum BoolData { }
    }
}