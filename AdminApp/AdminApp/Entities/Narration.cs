using System;
using System.Collections.Generic;

namespace AdminApp.Models;

public partial class Narration
{
    public int Id { get; set; }

    public int? PoiId { get; set; }

    public string? LanguageCode { get; set; }

    public string? Content { get; set; }

    public virtual Poi? Poi { get; set; }
}
