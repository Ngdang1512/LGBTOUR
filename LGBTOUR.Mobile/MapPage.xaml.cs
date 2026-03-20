namespace LGBTOUR.Mobile;

public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();
        LoadMap();
    }

    private void LoadMap()
    {
        // 1. Tạo một trang web mini bằng HTML, chứa thẻ iframe theo đúng yêu cầu của Google
        var mapHtml = @"
        <!DOCTYPE html>
        <html>
        <head>
            <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
            <style>
                body { margin: 0; padding: 0; overflow: hidden; }
                iframe { width: 100vw; height: 100vh; border: none; }
            </style>
        </head>
        <body>
            <iframe src='https://maps.google.com/maps?saddr=Dinh+Doc+Lap,Ho+Chi+Minh&daddr=Nha+Tho+Duc+Ba,Ho+Chi+Minh+to:Cho+Ben+Thanh,Ho+Chi+Minh&output=embed'></iframe>
        </body>
        </html>";

        // 2. Ép WebView đọc đoạn mã HTML này thay vì đọc link trực tiếp
        MapWebView.Source = new HtmlWebViewSource
        {
            Html = mapHtml
        };
    }
}