
namespace VoxelLUT {

    /**
     * The static class which manages all the LUT variations for
     * the blocks. This is a self managed class, when accessed for the first
     * time, the LUT table will be built and ready for access.
     */
    public sealed class BlockLUT {
        // the number of bits we will use, which is 6 bits
        // making a possible unsigned combination of 2 ^ 6 (64) values
        public const int MAX_LUT = 1 << 6;

        // represents 111111 bit sequence
        public const int BIT_MASK = 0x3F;

        // the primary LUT table which identifies how a certain
        // block will be rendered according to how it's neighbour
        // blocks are outlined
        private static readonly BlockVisual[] _LUT;

        static BlockLUT() {
            _LUT = new BlockVisual[MAX_LUT];

            for (int i = 0; i < MAX_LUT; i++) {
                // this uses reflection to map a class name so we can generate it
                string clazz = GetRefClassName(i);

                // use reflection to invoke our anonymous class matching the name
                // the Create() functionality once invoked will automatically add
                // the block into the LUT table of this class. This is so we don't
                // manually invoke and create classes who'se names are a pain to deal
                // with already.
                System.Type.GetType(clazz).GetMethod("Create").Invoke(null, null);
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
        public static BlockVisual LUT(int index) {
            return _LUT[index & BIT_MASK];
        }

        /**
         * Used to generate the pre-defined name of the class which we will be using
         * to dynamically invoke when this class is loaded first time. 
         */
        private static string GetRefClassName(int index) {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(12);

            builder.Append("Block_");

            // write o or x depending if the requested bit
            // is 1 or a 0
            builder.Append((index & (1 << 0)) == 0 ? "o" : "x");
            builder.Append((index & (1 << 1)) == 0 ? "o" : "x");
            builder.Append((index & (1 << 2)) == 0 ? "o" : "x");
            builder.Append((index & (1 << 3)) == 0 ? "o" : "x");
            builder.Append((index & (1 << 4)) == 0 ? "o" : "x");
            builder.Append((index & (1 << 5)) == 0 ? "o" : "x");

            return builder.ToString();
        }
    }
}
