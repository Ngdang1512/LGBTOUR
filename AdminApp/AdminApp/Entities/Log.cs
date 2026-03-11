using System;
using System.Collections.Generic;

namespace AdminApp.Models;

public partial class Log
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public int? PoiId { get; set; }

    public DateTime? ListenTime { get; set; }

    public double? Lat { get; set; }

    public double? Lng { get; set; }

    public virtual Poi? Poi { get; set; }
}
