using System.Collections.Generic;
using UnityEngine;

namespace Nothke.AStar.Test
{
    public class TestAStar
    {
        SquareGrid grid;
        AStar astar;

        public Vector2Int source = new Vector2Int(1, 4);
        public Vector2Int target = new Vector2Int(8, 5);

        public void DrawGridAndPathGizmos()
        {
            AStarUtils.DrawGridAndPathGizmos(grid, astar, target);

            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(new Vector3(source.x, source.y), Vector3.one);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector3(target.x, target.y), Vector3.one);
        }

        public void Init()
        {
            grid = new SquareGrid(64, 32);
            for (var x = 1; x < 4; x++)
            {
                for (var y = 7; y < 9; y++)
                {
                    grid.walls.Add(new Vector2Int(x, y));
                }
            }

            for (var x = 10; x < 16; x++)
            {
                for (var y = 16; y < 18; y++)
                {
                    grid.walls.Add(new Vector2Int(x, y));
                }
            }

            for (var x = 15; x < 25; x++)
            {
                for (var y = 20; y < 24; y++)
                {
                    grid.walls.Add(new Vector2Int(x, y));
                }
            }

            // Original forests example
            grid.forests = new HashSet<Vector2Int>
            {
                new Vector2Int(3, 4), new Vector2Int(3, 5),
                new Vector2Int(4, 1), new Vector2Int(4, 2),
                new Vector2Int(4, 3), new Vector2Int(4, 4),
                new Vector2Int(4, 5), new Vector2Int(4, 6),
                new Vector2Int(4, 7), new Vector2Int(4, 8),
                new Vector2Int(5, 1), new Vector2Int(5, 2),
                new Vector2Int(5, 3), new Vector2Int(5, 4),
                new Vector2Int(5, 5), new Vector2Int(5, 6),
                new Vector2Int(5, 7), new Vector2Int(5, 8),
                new Vector2Int(6, 2), new Vector2Int(6, 3),
                new Vector2Int(6, 4), new Vector2Int(6, 5),
                new Vector2Int(6, 6), new Vector2Int(6, 7),
                new Vector2Int(7, 3), new Vector2Int(7, 4),
                new Vector2Int(7, 5)
            };

            astar = new AStar(grid);
        }

        public void Main()
        {
            // Run A*
            astar.Search(grid,
                new Vector2Int(source.x, source.y),
                new Vector2Int(target.x, target.y));
        }
    }
}