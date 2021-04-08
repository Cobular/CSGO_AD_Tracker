using System;
using System.Windows.Forms;

namespace CSGO_AD_Tracker_Forms_net5
{
    public enum CsgoKeys
    {
        W = Keys.W,
        A = Keys.A,
        S = Keys.S,
        D = Keys.D,
        LCtrl = Keys.LControlKey,
        Space = Keys.Space,
    }

    public sealed class KeyboardData
    {
        private static readonly Lazy<KeyboardData>
            Lazy = new(() => new KeyboardData());

        public static KeyboardData Instance => Lazy.Value;

        // The status of various keys. True means pressed.
        // 0: w
        // 1: a
        // 2: s
        // 3: d
        // 4: ctrl
        // 5: space
        private readonly bool[] _keyboardStatus = new bool[6];

        /// <summary>
        /// Updates the keyboardStatus with the given key information.
        /// </summary>
        /// <param name="key">A Forms.Keys, must be w, a, s, d, ctrl, or space.</param>
        /// <param name="pressed">A boolean representing if the key is pressed or not</param>
        /// <returns>True if key is one of those, false otherwise</returns>
        public bool UpdateKeyStatus(CsgoKeys key, bool pressed)
        {
            switch (key)
            {
                case CsgoKeys.W:
                    _keyboardStatus[0] = pressed;
                    break;
                case CsgoKeys.A:
                    _keyboardStatus[1] = pressed;
                    break;
                case CsgoKeys.S:
                    _keyboardStatus[2] = pressed;
                    break;
                case CsgoKeys.D:
                    _keyboardStatus[3] = pressed;
                    break;
                case CsgoKeys.LCtrl:
                    _keyboardStatus[4] = pressed;
                    break;
                case CsgoKeys.Space:
                    _keyboardStatus[5] = pressed;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public bool CheckKeyStatus(CsgoKeys key)
        {
            switch (key)
            {
                case CsgoKeys.W:
                    return _keyboardStatus[0];
                case CsgoKeys.A:
                    return _keyboardStatus[0];
                case CsgoKeys.S:
                    return _keyboardStatus[0];
                case CsgoKeys.D:
                    return _keyboardStatus[0];
                case CsgoKeys.LCtrl:
                    return _keyboardStatus[0];
                case CsgoKeys.Space:
                    return _keyboardStatus[0];
                default:
                    return false;
            }
        }

        public bool[] GetKeyStatuses => _keyboardStatus;

        private KeyboardData()
        {
        }
    }
}