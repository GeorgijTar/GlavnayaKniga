using System;

namespace GlavnayaKniga.Application.DTOs
{
    public class PositionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? EducationRequirements { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? BaseSalary { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string DisplayName => $"{Name}";
        public string CategoryDisplay => GetCategoryDisplay();
        public string StatusDisplay => IsArchived ? "Архивная" : "Активная";

        private string GetCategoryDisplay()
        {
            return Category switch
            {
                "Manager" => "Руководитель",
                "Specialist" => "Специалист",
                "Worker" => "Рабочий",
                _ => "Прочее"
            };
        }
    }
}