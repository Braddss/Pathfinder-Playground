using Braddss.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        private Material mapMaterial;

        private GraphicsBuffer mapBuffer;
        private int mapBufferId;

        private GraphicsBuffer pathBuffer;
        private int pathBufferId;

        private Map map;

        private int currentHashCode;

        // Start is called before the first frame update
        void Start()
        {
            currentHashCode = GetConfigHashCode();
            this.map = new Map(mapSize, perlinConfig, isoValue);
            this.mapBufferId = Shader.PropertyToID("_MapBuffer");
            this.pathBufferId = Shader.PropertyToID("_PathBuffer");

            SetMapProperties();


        }

        // Update is called once per frame
        void Update()
        {
            if (currentHashCode == GetConfigHashCode())
            {
                return;
            }

            currentHashCode = GetConfigHashCode();
            this.map = new Map(mapSize, perlinConfig, isoValue);
            SetMapProperties();
        }

        private void SetMapProperties()
        {
            var bufferData = this.map.Tiles.Select(tile => tile.Passable ? 1 : 0).ToArray();

            if (mapBuffer != null)
            {
                mapBuffer.Release();
            }

            mapBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, bufferData.Length, sizeof(int));

            mapBuffer.SetData(bufferData);

            mapMaterial.SetBuffer(mapBufferId, mapBuffer);

            mapMaterial.SetInteger("_SizeX", mapSize.x);
            mapMaterial.SetInteger("_SizeY", mapSize.y);
        }

        private void SetPathProperties(Vector2Int[] path)
        {
            if (pathBuffer != null)
            {
                pathBuffer.Release();
            }

            pathBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, path.Length, sizeof(int2))
        }

        private void OnDestroy()
        {
            this.mapBuffer.Release();
            this.pathBuffer.Release();
        }

        private int GetConfigHashCode()
        {
            return mapSize.GetHashCode() * 3 + perlinConfig.GetHashCode() * 5 + isoValue.GetHashCode() * 7;
        }
    }
}
