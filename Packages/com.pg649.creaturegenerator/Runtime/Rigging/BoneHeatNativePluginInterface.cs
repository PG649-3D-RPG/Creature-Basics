using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class BoneHeatNativePluginInterface
{

    public struct Triplet {
        int i;
        int j;
        float value;

        public Triplet(int i, int j, float value) {
            this.i = i;
            this.j = j;
            this.value = value;
        }
    }

    public static void Example() {
        List<Triplet> triplets = new List<Triplet>();
        triplets.Add(new Triplet(0, 0, 1));
        triplets.Add(new Triplet(1, 1, 1));
        Triplet[] tripletsArray = triplets.ToArray();
        
        float[] rhsArray = new float[] {69.2f, 42.9f};
        float[] resultArray = new float[rhsArray.Length];

        GCHandle tripletsArrayHandle = GCHandle.Alloc(tripletsArray, GCHandleType.Pinned);
        GCHandle rhsArrayHandle = GCHandle.Alloc(rhsArray, GCHandleType.Pinned);
        GCHandle resultArrayHandle = GCHandle.Alloc(resultArray, GCHandleType.Pinned);

        int code = solveSPDMatrix(
                2, 
                2, 
                tripletsArrayHandle.AddrOfPinnedObject(), 
                tripletsArray.Length, 
                rhsArrayHandle.AddrOfPinnedObject(), 
                rhsArray.Length, 
                resultArrayHandle.AddrOfPinnedObject());

        if (code == 0) {
            Debug.Log(resultArray[0]); // should be 69.2
        }

        tripletsArrayHandle.Free();
        rhsArrayHandle.Free();
        resultArrayHandle.Free();
    }

    [DllImport("BoneHeat")]
    private static extern int solveSPDMatrix(int rows, int cols, System.IntPtr triplets, int tripletsLength, System.IntPtr rhs, int rhsLength, System.IntPtr result);

}
