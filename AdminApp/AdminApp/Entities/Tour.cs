using System;
using System.Collections.Generic;

namespace AdminApp.Models;

public partial class Tour
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<TourPoi> TourPois { get; set; } = new List<TourPoi>();
}
