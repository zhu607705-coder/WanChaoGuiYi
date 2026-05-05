using UnityEngine;

namespace WanChaoGuiYi
{
    public static class TaxSystem
    {
        public static int CalculateRegionalTax(RegionState region)
        {
            float integrationFactor = Mathf.Clamp01(region.integration / 100f);
            float annexationPenalty = Mathf.Clamp01(region.annexationPressure / 150f);
            float effectiveTax = region.taxOutput * integrationFactor * (1f - annexationPenalty);
            return Mathf.Max(0, Mathf.RoundToInt(effectiveTax));
        }
    }
}
