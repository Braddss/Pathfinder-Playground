using Braddss.Pathfinding;
using Braddss.Pathfinding.Astar;
using Codice.Client.BaseCommands.Update.Fast.Transformers;
using System;
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
            XPassable   = 1,
            Passable    = 2,
            Player      = 3,
            End         = 4,
            Path        = 5,
            Open        = 6,
            Closed      = 7,
            Border      = 8,
            Hover       = 4096,
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
        private AStar aStar;

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
            this.map = new Map(mapSize, perlinConfig, isoValue);
            this.mapBufferId = Shader.PropertyToID("_MapBuffer");

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
            if (runPathfinding && !lastRunPathfinding || runPathfinding && pathNeedsUpdate)
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
            if (runPathfinding && !lastRunPathfinding || runPathfinding && pathNeedsUpdate)
            {
                CalculatePathStepwise(startPos);
            }

            if (path != null)
            {
                StepPath();
                return;
            }

            StepDebug();

            //SetDebugProperties();
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

                var p = aStar.CalculatePathStepwise();

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
                    var vec = new Vector2Int(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y));
                    if (map.GetTile(vec).Passable)
                    {
                        start = vec;
                        break;
                    }
                }
            }

            aStar = new AStar(map);

            Vector2Int end = Vector2Int.zero;

            while (true)
            {

                var vec = new Vector2Int(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y));
                if (vec != start && map.GetTile(vec).Passable)
                {
                    end = vec;
                    break;
                }
            }

            path = null;
            aStar.InitCalculatePathStepwise(start.Value, end);

                //while (true)
                //{
                //    p = aStar.CalculatePathStepwise();

                //    var open = aStar.Open;
                //    var closed = aStar.Closed;

                //    if (p != null)
                //    {
                //        break;
                //    }
                //}
                


            Debug.Log($"Start: {start}, End: {end}");

            PathIndex = 0;
            //SetPathProperties(); 
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
            startPos = null;
        }

        private void SetMapProperties()
        {
            var bufferData = this.map.Tiles.Select(tile => tile.Passable ? PassableState.Passable : PassableState.XPassable).ToArray();

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

            var start = map.ToIndex(path[PathIndex]);
            var end = map.ToIndex(path[^1]);

            states[start] = PassableState.Player;
            states[end] = PassableState.End;

            for(int i = PathIndex + 1; i < path.Length - 1; i++)
            {
                states[map.ToIndex(path[i])] = PassableState.Path;
            }
        }

        private void ApplyDebug(PassableState[] states)
        {
            if (aStar == null || path != null)
            {
                return;
            }

            var open = aStar.Open.Select(t => map.ToIndex(t.Index)).ToArray();

            var closed = aStar.Closed.Select(t => map.ToIndex(t.Index)).ToArray();

            for (int i = 0; i < open.Length; i++)
            {
                states[open[i]] = PassableState.Open;
            }

            for (int i = 0; i < closed.Length; i++)
            {
                states[closed[i]] = PassableState.Closed;
            }

            var start = map.ToIndex(aStar.Start);
            var end = map.ToIndex(aStar.End);

            var tempPath = aStar.GetTempPath();

            states[start] = PassableState.Player;
            states[end] = PassableState.End;

            for (int i = 1; i < tempPath.Length - 1; i++)
            {
                states[map.ToIndex(tempPath[i])] = PassableState.Path;
            }
        }

        private void OnDestroy()
        {
            this.mapBuffer?.Release();
        }

        private int GetMapConfigHashCode()
        {
            return mapSize.GetHashCode() * 3 + perlinConfig.GetHashCode() * 5 + isoValue.GetHashCode() * 7 + showPath.GetHashCode() * 11 + runPathfinding.GetHashCode() * 13 + showDebug.GetHashCode() * 17;
        }
    }
}
