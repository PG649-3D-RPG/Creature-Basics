using System.Runtime.InteropServices;

public class BoneHeatNativePluginInterface
{

    [DllImport("BoneHeat")]
    public static extern int get_life();

}
