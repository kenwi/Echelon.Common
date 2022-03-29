namespace Echelon.Common.Extensions
{
    internal static class StringEx
    {
        public static string StripBackslashes(this string value)
            => value.Replace(@"\\", @"\");
    }
}
