public class User
{
    string name;
    int age;

    public User(string name="Unknown", int age=10)
    {
        this.name = name;
        this.age = age;
    }

    public void PrintInfo()
    {
        Console.WriteLine($"{name} is {age} years old.");
    }
}
