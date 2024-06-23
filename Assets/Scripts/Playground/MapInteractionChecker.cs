using Braddss.Pathfinding;
using Unity.Mathematics;
using UnityEngine;

namespace Bradds.Playground
{
    public class MapInteractionChecker : MonoBehaviour
    {
        private Camera mainCam;

        private Material mapMaterial;

        // Start is called before the first frame update
        void Start()
        {
            mainCam = Camera.main;
            mapMaterial = MapManager.I.MapMaterial;
        }
            
        // Update is called once per frame
        void Update()
        {
            CheckInteraction();
        }

        private void CheckInteraction()
        {
            var directionRay = mainCam.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(directionRay, out RaycastHit hit, 1000))
            {
                // send nothing stuff
                SetMaterialProperties(new int2(-1, -1));
                return;
            }

            if (!hit.transform)
            {
                Debug.Log("No transform hit.");
                return;
            }

            var localPosition = hit.transform.InverseTransformPoint(hit.point);

            var pos2d = new float2(localPosition.x, localPosition.z);

            pos2d = pos2d / 10 + new float2(0.5f, 0.5f);

            float2 min = new float2(0.01f, 0.01f);
            float2 max = new float2(0.99f, 0.99f);

            if (pos2d.x < min.x || pos2d.y < min.y || pos2d.x > max.x || pos2d.y > max.y)
            {
                return;
            }

            var mapSize = MapManager.I.MapSize.ToInt2();

            int2 id = new(((pos2d - min) / (max - min)) * mapSize);

            SetMaterialProperties(id);

            if (Input.GetMouseButton(0))
            {
                MapManager.I.SetTile(id.ToVec2Int(), MapManager.TileState.XPassable);
            }
            else if (Input.GetMouseButton(2)) 
            {
                MapManager.I.SetTile(id.ToVec2Int(), MapManager.TileState.Passable);
            }
        }

        private void SetMaterialProperties(int2 hoverIndex)
        {
            mapMaterial.SetInt("_HoverIndexX", hoverIndex.x);
            mapMaterial.SetInt("_HoverIndexY", hoverIndex.y);
        }
    }
}
