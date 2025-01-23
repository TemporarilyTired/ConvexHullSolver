using System.Numerics;

namespace ConvexHullSolver;

internal interface IConvexHullAlgorithm
{
    public abstract string AlgorithmName { get; }

    public abstract List<(T x, T y)> CalculateConvexHull<T>(List<(T x, T y)> points) where T :
        IComparisonOperators<T, T, Boolean>,
        IEqualityOperators<T, T, Boolean>,
        IMultiplyOperators<T, T, T>,
        ISubtractionOperators<T, T, T>,
        IAdditiveIdentity<T, T>;


    // Returns positive value if ABC occur in clockwise order (i.e., B is left of AC)
    public static T Orient2DFast<T>((T x, T y) A, (T x, T y) B, (T x, T y) C) where T : IComparisonOperators<T, T, Boolean>, IEqualityOperators<T, T, Boolean>, IMultiplyOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        T ACx = A.x - C.x;
        T BCx = B.x - C.x;
        T ACy = A.y - C.y;
        T BCy = B.y - C.y;
        return ACx * BCy - ACy * BCx;
    }
}
