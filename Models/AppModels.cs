namespace ozankaya_api.Models
{
    public class Barber
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Chair { get; set; } = string.Empty;
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int BarberId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Services { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsAccepted { get; set; } = false; // Yeni eklenen alan
    }

    public class BlockedSlot
    {
        public int Id { get; set; }
        public int BarberId { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}