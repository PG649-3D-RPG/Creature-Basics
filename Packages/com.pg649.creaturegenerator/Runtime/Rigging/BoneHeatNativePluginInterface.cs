using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class BoneHeatNativePluginInterface
{

    public static void Example() {
        SparseMatrix matrix = new SparseMatrix(2, 2);
        matrix.triplets.Add(new SparseMatrix.Triplet(0, 0, 1));
        matrix.triplets.Add(new SparseMatrix.Triplet(1, 1, 1));

        SparseMatrix.Triplet[] tripletsArray = matrix.triplets.ToArray();
        float[] rhsArray = new float[] {69.2f, 42.9f};
        float[] resultArray = new float[rhsArray.Length];

        GCHandle tripletsArrayHandle = GCHandle.Alloc(tripletsArray, GCHandleType.Pinned);
        GCHandle rhsArrayHandle = GCHandle.Alloc(rhsArray, GCHandleType.Pinned);
        GCHandle resultArrayHandle = GCHandle.Alloc(resultArray, GCHandleType.Pinned);

        int code = solveSPDMatrix(
                matrix.rows, 
                matrix.cols, 
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
