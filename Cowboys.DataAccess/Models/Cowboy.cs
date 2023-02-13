using Microsoft.EntityFrameworkCore;

namespace Cowboys.DataAccess;

[Index(nameof(Name), IsUnique = true)]
public class Cowboy
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int Damage { get; set; }
}