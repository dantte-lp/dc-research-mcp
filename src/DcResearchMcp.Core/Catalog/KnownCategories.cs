namespace DcResearchMcp.Core.Catalog;

/// <summary>
/// Canonical discipline / topic facets derived from the source path.
/// String-typed because the set grows over time (new ШНК series, new follow-up
/// groups). Use <see cref="All"/> for filter validation; unknown strings are
/// kept as <see cref="General"/>.
/// </summary>
public static class KnownCategories
{
    // r2 disciplines (one per discipline file under 30-findings/_aggregated/r2/)
    public const string AStrategy = "A-strategy";
    public const string BSite = "B-site";
    public const string CArchitecture = "C-architecture";
    public const string DPower = "D-power";
    public const string ECooling = "E-cooling";
    public const string FFireSecurity = "F-fire-security";
    public const string GNetwork = "G-network";
    public const string HOperations = "H-operations";
    public const string IVendors = "I-vendors";
    public const string JAiSpecific = "J-ai-specific";

    /// <summary>Aggregated r2 canon documents (the synthesised per-discipline files).</summary>
    public const string Aggregated = "_aggregated";

    /// <summary>Presentation design follow-up reports (Pres-FU-*).</summary>
    public const string PresentationDesign = "PRES-presentation-design";

    /// <summary>Технические задания (60-tech-spec/).</summary>
    public const string TechSpec = "tech-spec";

    /// <summary>Project overview docs (00-overview/).</summary>
    public const string Overview = "overview";

    /// <summary>International standards: TIA-942-C, ASHRAE, NFPA, BICSI, EN 50600, ISO …</summary>
    public const string IntlStandards = "intl-standards";

    /// <summary>RU standards: ШНК, КМК, ГОСТ Р.</summary>
    public const string UzStandards = "uz-standards";

    /// <summary>Vendor / former-vendor materials.</summary>
    public const string VendorMaterials = "vendor";

    /// <summary>Fallback for files that don't match any other category.</summary>
    public const string General = "general";

    /// <summary>All canonical category strings (for validation and faceting).</summary>
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        AStrategy, BSite, CArchitecture, DPower, ECooling, FFireSecurity,
        GNetwork, HOperations, IVendors, JAiSpecific, Aggregated, PresentationDesign,
        TechSpec, Overview, IntlStandards, UzStandards, VendorMaterials, General,
    };

    /// <summary>True if <paramref name="category"/> is one of the canonical facets.</summary>
    public static bool IsKnown(string category) => All.Contains(category);
}
