namespace BisleriumBlog.Application.Utilities;

public static class ExtensionMethod
{
    public static void ShuffleList<T>(this IList<T> list)
    {
        var rng = new Random();
        int n = list.Count;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            SwapElements(list, i, j);
        }
    }

    private static void SwapElements<T>(IList<T> list, int indexA, int indexB)
    {
        T temp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = temp;
    }
}
