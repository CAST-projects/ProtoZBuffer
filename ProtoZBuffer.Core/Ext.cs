namespace ProtoZBuffer.Core
{
    public static class Ext
    {
        public static string Capitalize(this string x)
        {
            var chars = x.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        public static string Safe(this string x)
        {
            return x ?? "";
        }
    }
}
