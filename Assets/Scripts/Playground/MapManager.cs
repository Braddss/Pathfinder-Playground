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
        [SerializeField]
        private Vector2Int mapSize = new Vector2Int(20, 20);

        [SerializeField]
        private PerlinConfig perlinConfig;

        [SerializeField]
        private float isoValue = 0.5f;

        [SerializeField]
        private bool showPath = true;

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

        // Start is called before the first frame update
        void Start()
        {
            currentMapHashCode = GetMapConfigHashCode();
            currentMapHashCode = GetPathConfigHashCode();
            this.map = new Map(mapSize, perlinConfig, isoValue);
            this.mapBufferId = Shader.PropertyToID("_MapBuffer");
            this.pathBufferId = Shader.PropertyToID("_PathBuffer");

            SetMapProperties();

            var aStar = new AStar(this.map);
            path = aStar.CalculatePath(new Vector2Int(10, 10), new Vector2Int(48, 48)).ToInt2Arr();
            pathIndex = 0;
            SetPathProperties();
        }

        private float stepTimer = 0;

        // Update is called once per frame
        void Update()
        {
            UpdateMapProperties();
            UpdatePathProperties();

            stepTimer += Time.deltaTime;

            if (stepTimer > speed)
            {
                stepTimer %= speed;

                mapMaterial.SetInteger(pathIndexId, ++pathIndex);

                if (pathIndex == path.Length - 1)
                {
                    CalculatePath();
                }
            }
        }

        private void CalculatePath(Vector2Int? start = null, Vector2Int? end = null)
        {
            start ??= new Vector2Int(Random.Range(0, 60), Random.Range(0, 60));
            end ??= new Vector2Int(Random.Range(0, 60), Random.Range(0, 60));
            path = new AStar(map).CalculatePath(start.Value, end.Value).ToInt2Arr();
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

            CalculatePath();
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
            var bufferData = this.map.Tiles.Select(tile => tile.Passable ? 1 : 0).ToArray();

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

            pathBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, path.Length, sizeof(int) * 2);
            
            pathBuffer.SetData(path);

            mapMaterial.SetBuffer(pathBufferId, pathBuffer);
            mapMaterial.SetInteger("_PathIndex", 0);
            mapMaterial.SetInteger("_PathCount", path.Length);
            mapMaterial.SetInteger("_ShowPath", showPath ? 1 : 0);
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
            return showPath.GetHashCode() * 3;
        }
    }
}
