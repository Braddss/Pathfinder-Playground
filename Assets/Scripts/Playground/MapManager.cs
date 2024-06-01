using Braddss.Pathfinding;
using Braddss.Pathfinding.Astar;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bradds.Playground
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager I { get; private set; }

        public enum PassableState
        {
            XPassable = 1,
            Passable = 2,
        }

        [SerializeField]
        private Vector2Int mapSize = new Vector2Int(20, 20);

        [SerializeField]
        private PerlinConfig perlinConfig;

        [SerializeField]
        private float isoValue = 0.5f;

        [SerializeField]
        private bool showPath = true;

        [SerializeField]
        private bool runPathfinding = false;
        private bool lastRunPathfinding = false;

        private bool pathNeedsUpdate = false;

        [SerializeField]
        private float speed = 0.2f;

        private int pathIndex = 0;
        private readonly int pathIndexId = Shader.PropertyToID("_PathIndex");

        [SerializeField]
        private Material mapMaterial;

        private GraphicsBuffer mapBuffer;
        private int mapBufferId;

        private GraphicsBuffer pathBuffer;
        private int pathBufferId;

        private Map map;
        private int2[] path;

        private int currentMapHashCode;
        private int currentPathHashCode;

        public Vector2Int MapSize { get => mapSize; }

        public Material MapMaterial { get => mapMaterial; }

        private void Awake()
        {
            I = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            currentMapHashCode = GetMapConfigHashCode();
            currentMapHashCode = GetPathConfigHashCode();
            this.map = new Map(mapSize, perlinConfig, isoValue);
            this.mapBufferId = Shader.PropertyToID("_MapBuffer");
            this.pathBufferId = Shader.PropertyToID("_PathBuffer");

            SetMapProperties();
        }

        private float stepTimer = 0;

        // Update is called once per frame
        void Update()
        {
            UpdateMapProperties();
            
            if (runPathfinding && !lastRunPathfinding || runPathfinding && pathNeedsUpdate)
            {
                CalculatePath();
            }

            if (runPathfinding)
            {
                StepPath();
            }

            UpdatePathProperties();
            lastRunPathfinding = runPathfinding;
        }

        private void StepPath()
        {
            stepTimer += Time.deltaTime;
            if (stepTimer > speed)
            {
                if (speed > 0)
                {
                    stepTimer %= speed;
                }

                mapMaterial.SetInteger(pathIndexId, ++pathIndex);

                if (pathIndex == path.Length - 1)
                {
                    CalculatePath(path[^1].ToVec2Int());
                }
            }
        }

        public void SetTile(Vector2Int index, PassableState state)
        {
            map.GetTile(index).Passable = state == PassableState.Passable;
            SetMapProperties();

            pathNeedsUpdate = true;
        }

        private void CalculatePath(Vector2Int? start = null)
        {
            pathNeedsUpdate = false;
            if (start == null)
            {
                while (true)
                {
                    var vec = new Vector2Int(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y));
                    if (map.GetTile(vec).Passable)
                    {
                        start = vec;
                        break;
                    }
                }
            }

            var aStar = new AStar(map);
            var p = new Vector2Int[0];

            Vector2Int end = Vector2Int.zero;
            var counter = 0;

            while (p.Length == 0 && counter++ < 10)
            {
                while (true)
                {

                    var vec = new Vector2Int(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y));
                    if (vec != start && map.GetTile(vec).Passable)
                    {
                        end = vec;
                        break;
                    }
                }

                p = aStar.CalculatePath(start.Value, end);
            }

            if (counter == 11)
            {
                CalculatePath();
                return;
            }

            Debug.Log($"Start: {start}, End: {end}");
            path = p.ToInt2Arr();

            pathIndex = 0;
            SetPathProperties();
        }

        private void UpdateMapProperties()
        {
            if (currentMapHashCode == GetMapConfigHashCode())
            {
                return;
            }

            currentMapHashCode = GetMapConfigHashCode();
            this.map = new Map(mapSize, perlinConfig, isoValue);
            SetMapProperties();

            pathNeedsUpdate = true;
        }

        private void UpdatePathProperties()
        {
            if (currentPathHashCode == GetPathConfigHashCode())
            {
                return;
            }

            currentPathHashCode = GetPathConfigHashCode();
            SetPathProperties();
        }

        private void SetMapProperties()
        {
            var bufferData = this.map.Tiles.Select(tile => tile.Passable ? PassableState.Passable : PassableState.XPassable).ToArray();

            mapBuffer?.Release();

            mapBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, bufferData.Length, sizeof(int));

            mapBuffer.SetData(bufferData);

            mapMaterial.SetBuffer(mapBufferId, mapBuffer);

            mapMaterial.SetInteger("_SizeX", mapSize.x);
            mapMaterial.SetInteger("_SizeY", mapSize.y);
        }

        private void SetPathProperties()
        {
            pathBuffer?.Release();

            if (path == null || path.Length == 0)
            {
                return;
            }

            pathBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, path.Length, sizeof(int) * 2);
            
            pathBuffer.SetData(path);

            mapMaterial.SetBuffer(pathBufferId, pathBuffer);
            mapMaterial.SetInteger("_PathIndex", 0);
            mapMaterial.SetInteger("_PathCount", path.Length);
            mapMaterial.SetInteger("_ShowPath", showPath && runPathfinding ? 1 : 0);
        }

        private void OnDestroy()
        {
            this.mapBuffer?.Release();
            this.pathBuffer?.Release();
        }

        private int GetMapConfigHashCode()
        {
            return mapSize.GetHashCode() * 3 + perlinConfig.GetHashCode() * 5 + isoValue.GetHashCode() * 7;
        }

        private int GetPathConfigHashCode()
        {
            return showPath.GetHashCode() * 3 + runPathfinding.GetHashCode() * 5;
        }
    }
}
