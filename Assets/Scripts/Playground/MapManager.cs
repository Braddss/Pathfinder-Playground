using Braddss.Pathfinding;
using Braddss.Pathfinding.Astar;
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

        private int pathIndex = 0;
        private readonly int pathIndexId = Shader.PropertyToID("_PathIndex");

        [SerializeField]
        private Material mapMaterial;

        private GraphicsBuffer mapBuffer;
        private int mapBufferId;

        private GraphicsBuffer pathBuffer;
        private int pathBufferId;

        private GraphicsBuffer openBuffer;
        private int openBufferId;

        private GraphicsBuffer closedBuffer;
        private int closedBufferId;

        private Map map;
        private int2[] path;
        private AStar aStar;


        private int currentMapHashCode;
        private int currentPathHashCode;
        private int currentDebugHashCode;

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
            currentPathHashCode = GetPathConfigHashCode();
            currentDebugHashCode = GetDebugConfigHashCode();
            this.map = new Map(mapSize, perlinConfig, isoValue);
            this.mapBufferId = Shader.PropertyToID("_MapBuffer");
            this.pathBufferId = Shader.PropertyToID("_PathBuffer");
            this.openBufferId = Shader.PropertyToID("_OpenBuffer");
            this.closedBufferId = Shader.PropertyToID("_ClosedBuffer");

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

            UpdatePathProperties();
            UpdateDebugProperties();
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

            var p = aStar.CalculatePathStepwise();

            if (p != null)
            {
                path = p.ToInt2Arr();
            }

            SetDebugProperties();
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
                    startPos = path[^1].ToVec2Int();
                    pathNeedsUpdate = true;
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
            path = p.ToInt2Arr();

            pathIndex = 0;
            SetPathProperties();
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
            startPos = null;
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

        private void UpdateDebugProperties()
        {
            if (currentDebugHashCode == GetDebugConfigHashCode()) 
            {
                return; 
            }

            currentDebugHashCode = GetDebugConfigHashCode();

            SetDebugProperties();
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
            mapMaterial.SetInteger("_OpenCount", 0);
            mapMaterial.SetInteger("_ClosedCount", 0);
            mapMaterial.SetInteger("_ShowPath", showPath && runPathfinding ? 1 : 0);
        }

        private void SetDebugProperties()
        {
            openBuffer?.Release();
            closedBuffer?.Release();

            //mapMaterial.SetInteger("_ShowDebug", showDebug && runPathfinding ? 1 : 0);

            if (!(showDebug && runPathfinding))
            {
                return;
            }

            var open = aStar.Open.Select(t => t.Index.ToInt2()).ToArray();

            if (open.Length > 0)
            {
                openBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, open.Length, sizeof(int) * 2);
                openBuffer.SetData(open);
            }


            var closed = aStar.Closed.Select(t => t.Index.ToInt2()).ToArray();

            if (closed.Length > 0)
            {
                closedBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, closed.Length, sizeof(int) * 2);
                closedBuffer.SetData(closed);
            }
 
            mapMaterial.SetBuffer(openBufferId, openBuffer);
            mapMaterial.SetBuffer(closedBufferId, closedBuffer);

            mapMaterial.SetInteger("_OpenCount", open.Length);
            mapMaterial.SetInteger("_ClosedCount", closed.Length);

            mapMaterial.SetInteger("_ShowDebug", showDebug && runPathfinding ? 1 : 0);
        }

        private void OnDestroy()
        {
            this.mapBuffer?.Release();
            this.pathBuffer?.Release();
            openBuffer?.Release();
            closedBuffer?.Release();

            mapMaterial.SetInteger("_PathIndex", 0);
            mapMaterial.SetInteger("_PathCount", 0);
            mapMaterial.SetInteger("_OpenCount", 0);
            mapMaterial.SetInteger("_ClosedCount", 0);
            mapMaterial.SetInteger("_ShowPath", 0);
            mapMaterial.SetInteger("_ShowDebug", 0);
        }

        private int GetMapConfigHashCode()
        {
            return mapSize.GetHashCode() * 3 + perlinConfig.GetHashCode() * 5 + isoValue.GetHashCode() * 7;
        }

        private int GetPathConfigHashCode()
        {
            return showPath.GetHashCode() * 3 + runPathfinding.GetHashCode() * 5;
        }

        private int GetDebugConfigHashCode()
        {
            return showDebug.GetHashCode();
        }
    }
}
