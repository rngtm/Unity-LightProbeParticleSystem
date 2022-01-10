namespace ParticleLighting
{
    using UnityEngine;

    public static class ShaderProperty
    {
        public static readonly int _MinPosition = Shader.PropertyToID("_MinPosition");
        public static readonly int _MaxPosition = Shader.PropertyToID("_MaxPosition");
        public static readonly int _ShTex = Shader.PropertyToID("_ShTex");
    }
}