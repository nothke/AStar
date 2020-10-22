using UnityEngine;

namespace Nothke.AStar
{
    public static class AStarUtils
    {
        public static void DrawGridAndPathGizmos(SquareGrid grid, AStar astar, Vector2Int target)
        {
            // Print out the cameFrom array
            for (var y = 0; y < grid.height; y++)
            {
                for (var x = 0; x < grid.width; x++)
                {
                    var pos = new Vector3(x, y);

                    Vector2Int id = new Vector2Int(x, y);
                    Vector2Int ptr = id;
                    if (!astar.cameFrom.TryGetValue(id, out ptr))
                    {
                        ptr = id;
                    }

                    Vector3 ptrPos = new Vector3(ptr.x, ptr.y);

                    Color cameFromColor = new Color(1, 1, 0, 0.2f);

                    if (grid.walls.Contains(id))
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(new Vector3(id.x, id.y), Vector3.one);
                    }
                    else if (ptr.x == x + 1)
                    {
                        Gizmos.color = cameFromColor;
                        Gizmos.DrawLine(pos, ptrPos);
                    }
                    else if (ptr.x == x - 1)
                    {
                        Gizmos.color = cameFromColor;
                        Gizmos.DrawLine(pos, ptrPos);
                    }
                    else if (ptr.y == y + 1)
                    {
                        Gizmos.color = cameFromColor;
                        Gizmos.DrawLine(pos, ptrPos);
                    }
                    else if (ptr.y == y - 1)
                    {
                        Gizmos.color = cameFromColor;
                        Gizmos.DrawLine(pos, ptrPos);
                    }
                    else
                    {
                        Gizmos.color = Color.grey;
                        Gizmos.DrawWireCube(pos + new Vector3(0, 0, 1), Vector3.one);
                    }

                    if (grid.forests.Contains(id))
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(new Vector3(id.x, id.y), Vector3.one);
                    }
                }
            }

            Vector2Int current = target;
            for (int i = 0; i < 100; i++)
            {
                Vector2Int ptr;
                if (astar.cameFrom.TryGetValue(current, out ptr))
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine((Vector3Int)current, (Vector3Int)ptr);
                }
                else
                    break;

                current = ptr;
            }
        }
    }
}