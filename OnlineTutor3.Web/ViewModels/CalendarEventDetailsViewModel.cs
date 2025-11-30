namespace OnlineTutor3.Web.ViewModels
{
    public class CalendarEventDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? ClassName { get; set; }
        public string? StudentName { get; set; }
        public string? Location { get; set; }
        public string? Color { get; set; }
        public bool IsCompleted { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
    }
}

