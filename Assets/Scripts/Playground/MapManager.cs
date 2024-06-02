using Braddss.Pathfinding;
using Braddss.Pathfinding.Maps;
using System.Linq;
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
            Player = 3,
            End = 4,
            Path = 5,
            Open = 6,
            Closed = 7,
            Border = 8,
            Hover = 4096,
        }

        [SerializeField]
        private Vector2Int mapSize = new Vector2Int(20, 20);

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

        private int currentMapHashCode;

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
            map = new Map(mapSize, perlinConfig, isoValue);
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
                CalculatePath(startPos);
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
                }
            }
        }

        private void StepDebug()
        {
            stepTimer += Time.deltaTime;

            if (stepTimer > speed)
            {
                if (speed > 0)
                {
                    stepTimer %= speed;
                }

                ++DebugIndex;

                Debug.Log(DebugIndex);

                Vector2Int[] p = pathfinder.CalculatePathStepwise();

                if (p != null)
                {
                    if (p.Length == 0)
                    {
                        pathNeedsUpdate = true;
                        startPos = null;
                    }
                    else
                    {
                        path = p;
                    }
                }
            }
        }

        public void SetTile(Vector2Int index, PassableState state)
        {
            map.GetTile(index).Passable = state == PassableState.Passable;
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
                    Vector2Int vec = new Vector2Int(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y));
                    if (map.GetTile(vec).Passable)
                    {
                        start = vec;
                        break;
                    }
                }
            }

            var pathFinder = new Pathfinder(map);

            Vector2Int end;

            while (true)
            {

                Vector2Int vec = new Vector2Int(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y));
                if (vec != start && map.GetTile(vec).Passable)
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

            Debug.Log($"Start: {start}, End: {end}");
            path = p;

            PathIndex = 0;
            //SetPathProperties();
        }

        private void CalculatePathStepwise(Vector2Int? start = null)
        {
            pathNeedsUpdate = false;
            if (start == null)
            {
                while (true)
                {
                    Vector2Int vec = new Vector2Int(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y));
                    if (map.GetTile(vec).Passable)
                    {
                        start = vec;
                        break;
                    }
                }
            }

            pathfinder = new Pathfinder(map);

            Vector2Int end;

            while (true)
            {

                Vector2Int vec = new Vector2Int(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y));
                if (vec != start && map.GetTile(vec).Passable)
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
            if (currentMapHashCode == GetMapConfigHashCode())
            {
                return;
            }

            currentMapHashCode = GetMapConfigHashCode();
            map = new Map(mapSize, perlinConfig, isoValue);
            SetMapProperties();

            pathNeedsUpdate = true;
            startPos = null;
        }

        private void SetMapProperties()
        {
            PassableState[] bufferData = map.Tiles.Select(tile => tile.Passable ? PassableState.Passable : PassableState.XPassable).ToArray();

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

            mapMaterial.SetInteger("_SizeX", mapSize.x);
            mapMaterial.SetInteger("_SizeY", mapSize.y);
        }

        private void ApplyPath(PassableState[] states)
        {
            if (path == null)
            {
                return;
            }

            int start = map.ToIndex(path[PathIndex]);
            int end = map.ToIndex(path[^1]);

            states[start] = PassableState.Player;
            states[end] = PassableState.End;

            for (int i = PathIndex + 1; i < path.Length - 1; i++)
            {
                states[map.ToIndex(path[i])] = PassableState.Path;
            }
        }

        private void ApplyDebug(PassableState[] states)
        {
            if (pathfinder == null || path != null)
            {
                return;
            }

            int[] open = pathfinder.Open.Select(t => map.ToIndex(t.Index)).ToArray();

            int[] closed = pathfinder.Closed.Select(t => map.ToIndex(t.Index)).ToArray();

            for (int i = 0; i < open.Length; i++)
            {
                states[open[i]] = PassableState.Open;
            }

            for (int i = 0; i < closed.Length; i++)
            {
                states[closed[i]] = PassableState.Closed;
            }

            int start = map.ToIndex(pathfinder.Start);
            int end = map.ToIndex(pathfinder.End);

            Vector2Int[] tempPath = pathfinder.GetTempPath();

            states[start] = PassableState.Player;
            states[end] = PassableState.End;

            for (int i = 1; i < tempPath.Length - 1; i++)
            {
                states[map.ToIndex(tempPath[i])] = PassableState.Path;
            }
        }

        private void OnDestroy()
        {
            mapBuffer?.Release();
        }

        private int GetMapConfigHashCode()
        {
            return mapSize.GetHashCode() * 3 + perlinConfig.GetHashCode() * 5 + isoValue.GetHashCode() * 7 + showPath.GetHashCode() * 11 + runPathfinding.GetHashCode() * 13 + showDebug.GetHashCode() * 17;
        }
    }
}
