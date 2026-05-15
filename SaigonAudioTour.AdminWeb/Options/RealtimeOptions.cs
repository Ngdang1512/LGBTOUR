namespace SaigonAudioTour.AdminWeb.Options;

public sealed class RealtimeOptions
{
    public string ApiBaseUrl { get; set; } = "http://localhost:5117";

    public string HubUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ApiBaseUrl))
            {
                return "http://localhost:5117/hubs/activity";
            }

            return $"{ApiBaseUrl.TrimEnd('/')}/hubs/activity";
        }
    }
}
