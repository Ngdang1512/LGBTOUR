    using System;
using System.Collections.Generic;

namespace AdminApp.Models;

public partial class Poi
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public double? Lat { get; set; }

    public double? Lng { get; set; }

    public int? Radius { get; set; }

    public string? Image { get; set; }

    public string? AudioPath { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<Narration> Narrations { get; set; } = new List<Narration>();

    public virtual ICollection<TourPoi> TourPois { get; set; } = new List<TourPoi>();
}
