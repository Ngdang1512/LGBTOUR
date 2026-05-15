# Saigon Audio Tour

Saigon Audio Tour is a multi-project .NET solution with:

- **SaigonAudioTour.Api**: ASP.NET Core API for POIs, narrations, audio, telemetry, and auth
- **SaigonAudioTour.Web**: Blazor WebAssembly PWA for QR quick access and audio playback
- **SaigonAudioTour.AdminWeb**: Admin MVC app for managing POIs and narration uploads
- **SaigonAudioTour.Mobile**: .NET MAUI mobile client

## Local URLs

- API: `http://localhost:5117`
- Web PWA: `http://localhost:5200`
- Admin Web: `http://localhost:5102`

## Run Order

1. Start the API.
2. Start the Admin Web app if you need management screens.
3. Start the Web PWA for QR access and playback.
4. Run the mobile app separately for emulator/device testing.

## Useful Endpoints

- `GET /api/pois`
- `GET /api/narrations/{poiId}?lang=vi`
- `GET /api/narrations/tts/{poiId}?lang=vi`
- `GET /hubs/telemetry` for SignalR telemetry

## PWA Install

The Web app is configured as a PWA.

- Open `http://localhost:5200` in a supported browser.
- Use the browser menu to install the app.
- On iPhone/iPad, use **Share** → **Add to Home Screen**.
- The app includes offline fallback and cached assets for basic browsing when disconnected.

## Notes

- Narration audio can be uploaded from the Admin Web UI at `/Narrations/Create`.
- If a POI has no stored narration, the Web client falls back to the TTS endpoint.
- Telemetry events are sent through the SignalR hub and stored server-side for analytics.
