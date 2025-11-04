namespace CinemaTicketSystemCore.Models.ViewModels
{
    public class RoomViewModel
    {
        public int ScreeningId { get; set; }
        public string FilmTitle { get; set; } = string.Empty;
        public string CinemaName { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public Dictionary<string, bool> SeatStatus { get; set; } = new();
        public Dictionary<string, string> SeatOwners { get; set; } = new();
        public string? CurrentUserId { get; set; }

        public bool IsSeatReserved(int row, int seat)
        {
            string key = $"{row}_{seat}";
            return SeatStatus.ContainsKey(key) && SeatStatus[key];
        }

        public bool IsSeatOwnedByUser(int row, int seat)
        {
            string key = $"{row}_{seat}";
            return SeatOwners.ContainsKey(key) && SeatOwners[key] == CurrentUserId;
        }
    }
}

