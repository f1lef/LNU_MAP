namespace Backend.Models;

public class RegisterRequest
{//Реєстрація пошти
    public string Email { get; set; } = string.Empty;
//HASS PASSWORD
    public string Password { get; set; } = string.Empty;
//Група
    public string Group { get; set; } = string.Empty;
}