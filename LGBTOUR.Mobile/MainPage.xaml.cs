using System.Collections.ObjectModel;
using LGBTOUR.Mobile.Models;

namespace LGBTOUR.Mobile;

public partial class MainPage : ContentPage
{
    List<Place> allPlaces;
    public ObservableCollection<Place> DisplayPlaces { get; set; }

    private string _selectedCategory = "Popular";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            // Dùng luôn hàm OnPropertyChanged có sẵn của ContentPage, không cần tự viết nữa
            OnPropertyChanged(); 
        }
    }

    public MainPage()
    {
        InitializeComponent();

        allPlaces = new List<Place>
        {
            new Place { Name = "Chợ đêm Phố Cổ", Location = "Hoàn Kiếm, Hà Nội", Rating = "4.8", Category = "Popular", ImageUrl = "https://images.unsplash.com/photo-1555921015-5532091f6026?w=500" },
            new Place { Name = "Phở Gia Truyền", Location = "Bát Đàn, Hà Nội", Rating = "4.9", Category = "Food", ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?w=500" },
            new Place { Name = "Làng gốm Bát Tràng", Location = "Gia Lâm, Hà Nội", Rating = "4.7", Category = "Popular", ImageUrl = "https://images.unsplash.com/photo-1583417319070-4a69db38a482?w=500" },
            new Place { Name = "Tràng Tiền Plaza", Location = "Hoàn Kiếm, Hà Nội", Rating = "4.5", Category = "Shopping", ImageUrl = "https://images.unsplash.com/photo-1567401893414-76b7b1e5a7a5?w=500" }
        };

        // Mặc định ban đầu chỉ hiển thị các mục Phổ biến
        DisplayPlaces = new ObservableCollection<Place>(allPlaces.Where(p => p.Category == "Popular"));
        BindingContext = this;
    }

    // Đã thêm các lớp bảo vệ (if) để C# không báo lỗi cảnh báo màu vàng nữa
    private void OnCategoryTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer gesture)
        {
            if (gesture.CommandParameter is string category)
            {
                SelectedCategory = category;

                var filtered = allPlaces.Where(p => p.Category == category).ToList();
                DisplayPlaces.Clear();
                foreach (var p in filtered) 
                {
                    DisplayPlaces.Add(p);
                }
            }
        }
    }

    private async void OnCardTapped(object sender, EventArgs e)
    {
        // Điều hướng sang trang chi tiết mà không làm mất thanh Bottom Bar
        await Shell.Current.GoToAsync("DetailPage");
    }
}