using UnityEngine;
using System.Collections;

namespace EzyVoxel {
    public sealed class BitUtil {
        public static string GetBitStringByte(int data) {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < 8; i++) {
                builder.Append(GetBit(data, i) ? 1 : 0);
            }

            return builder.ToString();
        }

        public static string GetBitStringShort(int data) {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < 16; i++) {
                builder.Append(GetBit(data, i) ? 1 : 0);
            }

            return builder.ToString();
        }

        public static string GetBitStringInt(int data) {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < 32; i++) {
                builder.Append(GetBit(data, i) ? 1 : 0);
            }

            return builder.ToString();
        }

        public static bool GetBit(int data, int pos) {
            return ((data >> pos) & 1) == 1;
        }
    }
}
