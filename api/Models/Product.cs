using System.ComponentModel.DataAnnotations;

namespace DemoApi.Models;

public class Product
{
    public int Id { get; set; }

    [Display(Name = "Название")]
    [Required(ErrorMessage = "Заполните поле.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Цена")]
    [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Цена должна быть от 0 до 1 000 000 000.")]
    public decimal Price { get; set; }

    [Display(Name = "Количество")]
    [Range(0, 1000000, ErrorMessage = "Количество должно быть от 0 до 1 000 000.")]
    public int Quantity { get; set; }
}
