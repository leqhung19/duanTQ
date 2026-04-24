namespace DoAn.FRONTEND.Models
{
    public class QRCode
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string? QRContent { get; set; }
    }
}