using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Otz.AccelerationStructures
{
    public struct UniformOriginGrid2D
    {
        public int Subdivisions;
        public float HalfExtents;

        public int CellCount;
        public int CellCountPerDimension;
        public int CellCountPerPlane;
        public float CellSize;
        public float2 BoundsMin;
        public float2 BoundsMax;
        public UniformOriginGrid2D(float halfExtents, int subdivisions)
        {
            Subdivisions = subdivisions;
            HalfExtents = halfExtents;

            CellCount = (int)math.pow(4f, subdivisions);
            CellCountPerDimension = (int)math.pow(2f, subdivisions);
            CellCountPerPlane = CellCountPerDimension * CellCountPerDimension;
            CellSize = (halfExtents * 2f) / (float)CellCountPerDimension;
            BoundsMin = new float2(-halfExtents);
            BoundsMax = new float2(halfExtents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSubdivisionLevelForMaxCellSize(float halfExtents, float maxCellSize, int maxSubdivisionLevel = 5)
        {
            for (int s = 1; s <= maxSubdivisionLevel; s++)
            {
                int cellCountPerDimension = (int)math.pow(2f, s);
                float cellSize = (halfExtents * 2f) / (float)cellCountPerDimension;
                if (cellSize < maxCellSize)
                {
                    return s;
                }
            }
            return maxSubdivisionLevel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInBounds(in UniformOriginGrid2D grid, float2 position)
        {
            return position.x > grid.BoundsMin.x &&
                   position.x < grid.BoundsMax.x &&
                   position.y > grid.BoundsMin.y &&
                   position.y < grid.BoundsMax.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetCellCoordsFromPosition(in UniformOriginGrid2D grid, float2 position)
        {
            float2 localPos = position - grid.BoundsMin;
            int2 cellCoords = new int2
            {
                x = (int)math.floor(localPos.x / grid.CellSize),
                y = (int)math.floor(localPos.y / grid.CellSize),
            };
            cellCoords = math.clamp(cellCoords, int2.zero, new int2(grid.CellCountPerDimension - 1));
            return cellCoords;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetCellCoordsFromIndex(in UniformOriginGrid2D grid, int index)
        {
            return new int2
            {
                x = index % grid.CellCountPerDimension,
                y = (index % grid.CellCountPerPlane) / grid.CellCountPerDimension,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCellIndex(in UniformOriginGrid2D grid, float2 position)
        {
            if (IsInBounds(in grid, position))
            {
                int2 cellCoords = GetCellCoordsFromPosition(in grid, position);
                return GetCellIndexFromCoords(in grid, cellCoords);
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCellIndexFromCoords(in UniformOriginGrid2D grid, int2 coords)
        {
            return (coords.x) + (coords.y * grid.CellCountPerDimension);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AABBIntersectAABB(float2 aabb1Min, float2 aabb1Max, float2 aabb2Min, float2 aabb2Max)
        {
            return (aabb1Min.x <= aabb2Max.x && aabb1Max.x >= aabb2Min.x) &&
                   (aabb1Min.y <= aabb2Max.y && aabb1Max.y >= aabb2Min.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetCellCenter(float2 spatialDatabaseBoundsMin, float cellSize, int2 cellCoords)
        {
            float2 minCenter = spatialDatabaseBoundsMin + new float2(cellSize * 0.5f);
            return minCenter + ((float2)cellCoords * cellSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDistanceSqAABBToPoint(float2 point, float2 aabbMin, float2 aabbMax)
        {
            float2 pointOnBounds = math.clamp(point, aabbMin, aabbMax);
            return math.lengthsq(pointOnBounds - point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetAABBMinMaxCoords(in UniformOriginGrid2D grid, float2 aabbMin, float2 aabbMax, out int2 minCoords, out int2 maxCoords)
        {
            if (AABBIntersectAABB(aabbMin, aabbMax, grid.BoundsMin, grid.BoundsMax))
            {
                // Clamp to bounds
                aabbMin = math.clamp(aabbMin, grid.BoundsMin, grid.BoundsMax);
                aabbMax = math.clamp(aabbMax, grid.BoundsMin, grid.BoundsMax);

                // Get min max coords
                minCoords = GetCellCoordsFromPosition(in grid, aabbMin);
                maxCoords = GetCellCoordsFromPosition(in grid, aabbMax);

                return true;
            }

            minCoords = new int2(-1);
            maxCoords = new int2(-1);
            return false;
        }
    }
}