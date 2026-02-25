using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApi.Models;

public class Question
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Text { get; set; } = string.Empty;
    public string? Type { get; set; }
    [ForeignKey("Survey")]
    public int SurveyId { get; set; }
    public Survey? Survey { get; set; }
}
