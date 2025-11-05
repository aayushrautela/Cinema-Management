namespace CinemaTicketSystemCore.Models.ViewModels
{
    public class ScreeningViewModel
    {
        public int Id { get; set; }
        public string FilmTitle { get; set; } = string.Empty;
        public string CinemaName { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public int CinemaId { get; set; }
    }
}

