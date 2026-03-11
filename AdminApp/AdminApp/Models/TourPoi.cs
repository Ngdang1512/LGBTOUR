using System;
using System.Collections.Generic;

namespace AdminApp.Models;

public partial class TourPoi
{
    public int Id { get; set; }

    public int? TourId { get; set; }

    public int? PoiId { get; set; }

    public int? DisplayOrder { get; set; }

    public virtual Poi? Poi { get; set; }

    public virtual Tour? Tour { get; set; }
}
