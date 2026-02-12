namespace Klak.Hap
{
    public enum CodecType { Unsupported, Hap, HapQ, HapAlpha }

    internal static class NativeLibrary
    {
#if UNITY_IOS && !UNITY_EDITOR
        internal const string Name = "__Internal";
#else
        internal const string Name = "KlakHap";
#endif
    }
}
