namespace LibraryManagementSystem;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        ConsoleUI theUI = new ConsoleUI();
        theUI.SeedData(); //remove when I get actual data
        theUI.Show();
    }
}
