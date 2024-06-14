using ItemChanger;
using Newtonsoft.Json;
using RandomizerCore.Logic;
using System;
using System.Collections.Generic;

namespace BreakableWallRandomizer.IC.Shop
{
    public class WallLogicCost(Term _term, int _logicCost, Func<int, Cost> icConverter) : LogicCost
    {
        public Term term = _term;
        public int logicCost = _logicCost;

        [JsonIgnore]
        private Func<int, Cost> converter = icConverter;

        public override bool CanGet(ProgressionManager pm)
        {
            return pm.Has(term!, logicCost);
        }

        public override IEnumerable<Term> GetTerms()
        {
            if (term == null)
            {
                throw new InvalidOperationException("Term is undefined");
            }
            yield return term;
        }

        public Cost GetIcCost()
        {
            return converter?.Invoke(logicCost) ?? throw new InvalidOperationException("Cost converter is undefined");
        }

        public override string ToString()
        {
            return $"{logicCost} {term.ToString().Replace("Broken_", "")}";
        }
    }
}