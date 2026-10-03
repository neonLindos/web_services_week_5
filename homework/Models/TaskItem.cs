using System.ComponentModel.DataAnnotations;

namespace TaskManagerClient.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Display(Name = "Название")]
    [Required(ErrorMessage = "Заполните поле.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Выполнено")]
    public bool IsCompleted { get; set; }
}
