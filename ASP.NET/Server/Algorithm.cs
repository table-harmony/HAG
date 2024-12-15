using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server {
    public class Algorithm {

        public static byte[] Encode(byte[] data) {

            //TODO: read header 
            //TODO: get width, height, is there alpha, 

            //TODO: Decide which process to store the byte
            // COPY - A copy of the last pixel
            // SET - An array storing 64 unique pixels checking if current pixel exists
            // DELTA - A difference of last pixel in one byte
            // BIG DELTA - A difference which can be represented in two bytes
            // RGBA/RGB - Full 3 byte values

            return [];
        }
        
        public static byte[] Decode(byte[] data) {
            return [];
        }
    }
}
