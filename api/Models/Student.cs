using System.ComponentModel.DataAnnotations;

namespace DemoApi.Models;

public class Student
{
    public int Id { get; set; }

    [Display(Name = "ФИО")]
    [Required(ErrorMessage = "Заполните поле.")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Группа")]
    [Required(ErrorMessage = "Заполните поле.")]
    [StringLength(200)]
    public string Group { get; set; } = string.Empty;

    [Display(Name = "Курс")]
    [Range(1, 6, ErrorMessage = "Курс должен быть от 1 до 6.")]
    public int Course { get; set; }

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Заполните поле.")]
    [StringLength(200)]
    [EmailAddress(ErrorMessage = "Введите корректный Email.")]
    public string Email { get; set; } = string.Empty;
}
