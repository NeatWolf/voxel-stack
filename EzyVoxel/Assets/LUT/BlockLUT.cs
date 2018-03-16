using System;
using System.Reflection;
using UnityEngine;

namespace VoxelLUT {

    /**
     * The static class which manages all the LUT variations for
     * the blocks. This is a self managed class, when accessed for the first
     * time, the LUT table will be built and ready for access.
     */
    public sealed class BlockLUT {
        // the maximum number of bits in use for the LUT
        public const int MAX_BITS = 6;
        // the number of bits we will use, which is 6 bits
        // making a possible unsigned combination of 2 ^ 6 (64) values
        public const int MAX_LUT = 1 << MAX_BITS;

        // represents 111111 bit sequence for masking purposes
        public const int BIT_MASK = ~(~0 << MAX_BITS);

        // the primary LUT table which identifies how a certain
        // block will be rendered according to how it's neighbour
        // blocks are outlined
        private static readonly BlockVisual[] _LUT;

        static BlockLUT() {
            _LUT = new BlockVisual[MAX_LUT];

            for (int i = 0; i < MAX_LUT; i++) {
                // this uses reflection to map a class name so we can generate it
                string clazz = "VoxelLUT." + GetRefClassName(i);

                // use reflection to invoke our anonymous class matching the name
                // the Create() functionality once invoked will automatically add
                // the block into the LUT table of this class. This is so we don't
                // manually invoke and create classes who'se names are a pain to deal
                // with already.
                try {
                    Type.GetType(clazz).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                }
                catch {
                    Debug.LogError("BlockLUT::Failed to bind Class = " + clazz);
                }
            }
        }

        /**
         * This is called statically via the individual blocks to register
         * themselves into the system's lookup. Generally the blocks will register
         * automatically via invoking Create() when this class is accessed for the
         * first time.
         */
        public static void Put(int hex, BlockVisual visual) {
            _LUT[hex & BIT_MASK] = visual;
        }

        /**
         * The provided index will be mapped in the range of 0 to 64. Returns
         * the block which has rendering information. The index is calculated
         * depending on the neighbours of the chunk and varies.
         */ 
        public static BlockVisual Get(int index) {
            return _LUT[index & BIT_MASK];
        }

        /**
         * Used to generate the pre-defined name of the class which we will be using
         * to dynamically invoke when this class is loaded first time. 
         */
        public static string GetRefClassName(int index) {
            // maximum string is "Block_".length + MAX_BITS
            System.Text.StringBuilder builder = new System.Text.StringBuilder(6 + MAX_BITS);

            builder.Append("Block_");

            // write o or x depending if the requested bit
            // is 1 or a 0
            for (int i = 0; i < MAX_BITS; i++) {
                builder.Append((index & (1 << i)) == 0 ? "o" : "x");
            }

            return builder.ToString();
        }
    }
}
