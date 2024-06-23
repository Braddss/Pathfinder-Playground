using Braddss.Pathfinding;
using Braddss.Pathfinding.Maps;
using System.Linq;
using UnityEngine;
using static Braddss.Pathfinding.Pathfinder;
using Random = UnityEngine.Random;

namespace Bradds.Playground
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager I { get; private set; }

        public enum TileState
        {
            XPassable = 1,
            Passable = 101,
            Player = 103,
            End = 104,
            Path = 105,
            Open = 106,
            Closed = 107,
            Border = 108,
            Hover = 4096,
        }

        [SerializeField]
        private PathfindingAlogrithm algorithm;

        [SerializeField]
        private int mapSize = 20;

        [SerializeField]
        private PerlinConfig perlinConfig;

        [SerializeField]
        private float isoValue = 0.5f;

        [SerializeField]
        private bool showDebug = false;
        private bool lastShowDebug = false;

        [SerializeField]
        private bool showPath = true;

        [SerializeField]
        private bool runPathfinding = false;

        [SerializeField]
        private bool stepPath = false;
        private bool lastRunPathfinding = false;

        private bool pathNeedsUpdate = false;

        private Vector2Int? startPos = null;

        [SerializeField]
        private float speed = 0.2f;

        private int PathIndex
        {
            get => pathIndex;

            set
            {
                pathIndex = value;
                SetMapProperties();
            }
        }

        private int pathIndex = 0;

        private int DebugIndex
        {
            get => debugIndex;

            set
            {
                debugIndex = value;
                SetMapProperties();
            }
        }

        private int debugIndex = 0;

        [SerializeField]
        private Material mapMaterial;

        private GraphicsBuffer mapBuffer;
        private int mapBufferId;

        private Map map;
        private Vector2Int[] path;
        private IPathfinder pathfinder;

        private int currentMapConfigHashCode;

        private int currentMapDataHashCode;

        public Vector2Int MapSize { get => new Vector2Int(mapSize, mapSize); }

        public Material MapMaterial { get => mapMaterial; }

        private void Awake()
        {
            I = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            currentMapConfigHashCode = GetMapConfigHashCode();
            currentMapDataHashCode = GetMapDataHashCode();
            map = new Map(MapSize, perlinConfig, isoValue);
            mapBufferId = Shader.PropertyToID("_MapBuffer");

            SetMapProperties();
        }

        private float stepTimer = 0;

        // Update is called once per frame
        void Update()
        {
            UpdateMapProperties();

            if (runPathfinding)
            {
                RunPathfinding();
            }
        }

        private void LateUpdate()
        {
            lastRunPathfinding = runPathfinding;
            lastShowDebug = showDebug;
        }

        private void RunPathfinding()
        {
            if (lastShowDebug != showDebug)
            {
                pathNeedsUpdate = true;
            }

            if (!showDebug)
            {
                RunStandardPathfinding();
            }
            else
            {
                RunDebugPathfinding();
            }
        }

        private void RunStandardPathfinding()
        {
            if ((runPathfinding && !lastRunPathfinding) || (runPathfinding && pathNeedsUpdate))
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                CalculatePath(startPos);
                sw.Stop();
                Debug.Log(sw.Elapsed);
            }

            if (runPathfinding)
            {
                StepPath();
            }
        }

        private void RunDebugPathfinding()
        {
            if ((runPathfinding && !lastRunPathfinding) || (runPathfinding && pathNeedsUpdate))
            {
                CalculatePathStepwise(startPos);
            }

            if (path != null)
            {
                StepPath();
                return;
            }

            StepDebug();
        }

        private void StepPath()
        {
            stepTimer += Time.deltaTime;


            if (!stepPath)
            {
                if (stepTimer > 1)
                {
                    startPos = path[^1];
                    pathNeedsUpdate = true;
                    stepTimer = 0;
                }

                return;
            }


            if (stepTimer > speed)
            {
                if (speed > 0)
                {
                    stepTimer %= speed;
                }

                ++PathIndex;

                if (PathIndex == path.Length - 1)
                {
                    startPos = path[^1];
                    pathNeedsUpdate = true;
                    stepTimer = 0;
                }
            }
        }

        private void StepDebug()
        {
            stepTimer += Time.deltaTime;

            if (stepTimer < speed)
            {
                return;
            }

            var count = 1;
            if (speed > 0)
            {
                count = Mathf.Max((int) (stepTimer / speed), 1);
                stepTimer %= speed;
            }

            for (int i = 0; i < count; i++)
            {
                

                Vector2Int[] p = pathfinder.CalculatePathStepwise();

                if (p == null)
                {
                    continue;
                }

                if (p.Length == 0)
                {
                    pathNeedsUpdate = true;
                    startPos = null;
                }
                else
                {
                    path = p;
                }

                stepTimer = 0;

                break;
            }

            Debug.Log(DebugIndex);
            ++DebugIndex;
        }

        public void SetTile(Vector2Int index, TileState state)
        {
            map.GetTile(index).PassablePercent = state == TileState.Passable ? (byte)100 : (byte)0;
            SetMapProperties();

            pathNeedsUpdate = true;
            startPos = null;
        }

        private void CalculatePath(Vector2Int? start = null)
        {
            pathNeedsUpdate = false;
            if (start == null)
            {
                while (true)
                {
                    Vector2Int vec = new Vector2Int(Random.Range(0, MapSize.x), Random.Range(0, MapSize.y));
                    if (map.GetTile(vec).PassablePercent > 0)
                    {
                        start = vec;
                        break;
                    }
                }
            }

            var pathFinder = new Pathfinder(map, algorithm);

            Vector2Int end;

            while (true)
            {

                Vector2Int vec = new Vector2Int(Random.Range(0, MapSize.x), Random.Range(0, MapSize.y));
                if (vec != start && map.GetTile(vec).PassablePercent > 0)
                {
                    end = vec;
                    break;
                }
            }

            var p = pathFinder.CalculatePath(start.Value, end);

            if (p.Length == 0)
            {
                CalculatePath();
                return;
            }

            //Debug.Log($"Start: {start}, End: {end}");
            path = p;

            PathIndex = 0;
        }

        private void CalculatePathStepwise(Vector2Int? start = null)
        {
            pathNeedsUpdate = false;
            if (start == null)
            {
                while (true)
                {
                    Vector2Int vec = new Vector2Int(Random.Range(0, MapSize.x), Random.Range(0, MapSize.y));
                    if (map.GetTile(vec).PassablePercent > 0)
                    {
                        start = vec;
                        break;
                    }
                }
            }

            pathfinder = new Pathfinder(map, algorithm);

            Vector2Int end;

            while (true)
            {

                Vector2Int vec = new Vector2Int(Random.Range(0, MapSize.x), Random.Range(0, MapSize.y));
                if (vec != start && map.GetTile(vec).PassablePercent > 0)
                {
                    end = vec;
                    break;
                }
            }

            path = null;
            pathfinder.InitCalculatePathStepwise(start.Value, end);

            Debug.Log($"Start: {start}, End: {end}");

            PathIndex = 0;
        }

        private void UpdateMapProperties()
        {
            var mapConfigHash = GetMapConfigHashCode();
            var mapDataHash = GetMapDataHashCode();
            if (currentMapConfigHashCode == mapConfigHash && currentMapDataHashCode == mapDataHash)
            {
                return;
            }

            if (mapConfigHash != currentMapConfigHashCode)
            {
                map = new Map(MapSize, perlinConfig, isoValue);
            }

            currentMapConfigHashCode = mapConfigHash;
            currentMapDataHashCode = mapDataHash;

            SetMapProperties();

            pathNeedsUpdate = true;
            startPos = null;
        }

        private void SetMapProperties()
        {
            TileState[] bufferData = map.Tiles.Select(tile => (TileState)Mathf.Clamp(tile.PassablePercent + 1, 1, 101)).ToArray();

            if (showDebug)
            {
                ApplyDebug(bufferData);
            }

            if (showPath)
            {
                ApplyPath(bufferData);
            }


            mapBuffer?.Release();

            mapBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, bufferData.Length, sizeof(int));

            mapBuffer.SetData(bufferData);

            mapMaterial.SetBuffer(mapBufferId, mapBuffer);

            mapMaterial.SetInteger("_SizeX", MapSize.x);
            mapMaterial.SetInteger("_SizeY", MapSize.y);
        }

        private void ApplyPath(TileState[] states)
        {
            if (path == null)
            {
                return;
            }

            int start = map.ToIndex(path[PathIndex]);
            int end = map.ToIndex(path[^1]);

            states[start] = TileState.Player;
            states[end] = TileState.End;

            for (int i = PathIndex + 1; i < path.Length - 1; i++)
            {
                states[map.ToIndex(path[i])] = TileState.Path;
            }
        }

        private void ApplyDebug(TileState[] states)
        {
            if (pathfinder == null || path != null)
            {
                return;
            }

            int[] open = pathfinder.Open.Select(t => map.ToIndex(t.Index)).ToArray();

            int[] closed = pathfinder.Closed.Select(t => map.ToIndex(t.Index)).ToArray();

            for (int i = 0; i < open.Length; i++)
            {
                states[open[i]] = TileState.Open;
            }

            for (int i = 0; i < closed.Length; i++)
            {
                states[closed[i]] = TileState.Closed;
            }

            int start = map.ToIndex(pathfinder.Start);
            int end = map.ToIndex(pathfinder.End);

            Vector2Int[] tempPath = pathfinder.GetTempPath();

            states[start] = TileState.Player;
            states[end] = TileState.End;

            for (int i = 1; i < tempPath.Length - 1; i++)
            {
                states[map.ToIndex(tempPath[i])] = TileState.Path;
            }
        }

        private void OnDestroy()
        {
            mapBuffer?.Release();
        }

        private int GetMapConfigHashCode()
        {
            return MapSize.GetHashCode() * 3 + perlinConfig.GetHashCode() * 5 + isoValue.GetHashCode() * 7;
        }

        private int GetMapDataHashCode()
        {
            return showPath.GetHashCode() * 11 + runPathfinding.GetHashCode() * 13 + showDebug.GetHashCode() * 17;
        }
    }
}
