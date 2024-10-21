public static class ArrayExtensions
{
    public static int Width<T>(this T[,] array)
    {
        return array.GetLength(0);
    }

    public static int Height<T>(this T[,] array)
    {
        return array.GetLength(1);
    }
}
