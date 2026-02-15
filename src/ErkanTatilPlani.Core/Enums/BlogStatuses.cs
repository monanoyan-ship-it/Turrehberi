namespace ErkanTatilPlani.Core.Enums;

/// <summary>
/// Blog yazisi durumlari
/// </summary>
public static class BlogStatuses
{
    public static readonly TypeItem Draft = new(0, "Draft", "Blog.Status.Draft",
        "Taslak - Henuz yayinlanmadi",
        "bi-file-earmark", "bg-secondary", 1, isDefault: true);

    public static readonly TypeItem Published = new(1, "Published", "Blog.Status.Published",
        "Yayinda - Herkes okuyabilir",
        "bi-check-circle", "bg-success", 2);

    public static readonly TypeItem Archived = new(2, "Archived", "Blog.Status.Archived",
        "Arsivlendi - Artik gosterilmiyor",
        "bi-archive", "bg-warning text-dark", 3);

    public static IEnumerable<TypeItem> All => new[] { Draft, Published, Archived };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Draft = 0;
        public const int Published = 1;
        public const int Archived = 2;
    }
}
