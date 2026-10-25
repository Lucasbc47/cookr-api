namespace Cookr.Domain;

public class Ingredient
{
    public const int NameMaxLength = 100;

    private Ingredient() { }

    public Ingredient(string name)
    {
        Rename(name);
    }

#pragma warning disable S1144 // o EF Core usa o setter privado
    public int Id { get; private set; }
#pragma warning restore S1144
    public string Name { get; private set; } = string.Empty;

    public void Rename(string name)
    {
        Name = name.Trim();
    }
}
