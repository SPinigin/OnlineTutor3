namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Модель связи многие-ко-многим между учителями и предметами
    /// </summary>
    public class TeacherSubject
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }

        public int SubjectId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

