namespace HotelBookingManager;

public class BookingModel
{
    public string BookingId { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string IdentityCard { get; set; } = "";
    public string RoomType { get; set; } = "Standard";
    public DateTime CheckIn { get; set; } = DateTime.Today;
    public int Days { get; set; } = 1;
    public decimal PricePerNight { get; set; }
}
