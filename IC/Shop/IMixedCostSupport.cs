using ItemChanger;

namespace BreakableWallRandomizer.IC.Shop
{
    /// <summary>
    /// Extracted from MoreLocations. The idea is to be able to have multiple wall type costs in a single shop.
    /// </summary>
    public interface IMixedCostSupport
    {
        /// <summary>
        /// Whether a given unwrapped cost is matched by this.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool MatchesCost(Cost c);
        /// <summary>
        /// Gets a cost displayer for a given matching cost
        /// </summary>
        public CostDisplayer GetDisplayer(Cost c);
    }
}