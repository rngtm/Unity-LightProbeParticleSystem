namespace ParticleLighting
{
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// Box領域を格子状に区切り、ライトプローブの球面調和関数をサンプリングする
    /// </summary>
    [ExecuteAlways]
    public class SHLattice : MonoBehaviour
    {
        [SerializeField] private Vector3 size = new Vector3(1, 1, 1);
        [SerializeField] private Vector3Int grid = new Vector3Int(4, 4, 4);
        [SerializeField] private bool updateEveryFrame = false;

        [Header("=== Gizmos ===")]
        [SerializeField] private float gizmoSphereSize = 0.1f;
        
        [Header("=== Cache ===")]
        [SerializeField] private SphericalHarmonicsL2[] shTable = new SphericalHarmonicsL2[0];
        [SerializeField] private Vector3[] shPositionTable = new Vector3[0];

        public Vector3 MinPosition => transform.position - size / 2;
        public Vector3 MaxPosition => transform.position + size / 2;
        public SphericalHarmonicsL2[] ShTable => shTable;
        public Vector3Int Grid => grid;

        private void Start()
        {
            ApplySH();
        }

        private void Update()
        {
            if (updateEveryFrame)
            {
                ApplySH();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, size);
            for (int i = 0; i < shTable.Length; i++)
            {
                var sh = shTable[i];
                var p = shPositionTable[i];

                var color = new Color(
                    sh[rgb: 0, coefficient: 0],
                    sh[rgb: 1, coefficient: 0],
                    sh[rgb: 2, coefficient: 0],
                    1f);
                Gizmos.color = color;
                Gizmos.DrawSphere(p, gizmoSphereSize);
            }
        }

        private void ApplySH()
        {
            if (grid.x < 2) grid.x = 2;
            if (grid.y < 2) grid.y = 2;
            if (grid.z < 2) grid.z = 2;

            // var sampler = CustomSampler.Create("# CreateSH");
            // sampler.Begin();

            int totalCount = grid.x * grid.y * grid.z;
            if (shTable.Length != totalCount) shTable = new SphericalHarmonicsL2[totalCount];
            if (shPositionTable.Length != totalCount) shPositionTable = new Vector3[totalCount];

            // 格子上に区切り、各点でライトプローブをサンプリングする
            Vector3 minPosition = MinPosition; // 箱の最大座標
            Vector3 maxPosition = MaxPosition; // 箱の最小座標
            int index = 0;
            for (int zi = 0; zi < grid.z; zi++)
            {
                float tz = (float) zi / (grid.z - 1);
                float z = Mathf.Lerp(minPosition.z, maxPosition.z, tz);

                for (int yi = 0; yi < grid.y; yi++)
                {
                    float ty = (float) yi / (grid.y - 1);
                    float y = Mathf.Lerp(minPosition.y, maxPosition.y, ty);

                    for (int xi = 0; xi < grid.x; xi++)
                    {
                        float tx = (float) xi / (grid.x - 1);
                        float x = Mathf.Lerp(minPosition.x, maxPosition.x, tx);

                        // ライトプローブをサンプリングする
                        var p = new Vector3(x, y, z);
                        LightProbes.GetInterpolatedProbe(p, null, out SphericalHarmonicsL2 sh);

                        shPositionTable[index] = p;
                        shTable[index] = sh;
                        index++;
                    }
                }
            }

            // sampler.End();
        }
    }
}