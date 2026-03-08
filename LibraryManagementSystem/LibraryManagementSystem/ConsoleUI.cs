namespace LibraryManagementSystem;

using Spectre.Console;

public class ConsoleUI
{
    DataManager dataManager;
    Library library;
    FileSaver repo;

    public ConsoleUI()
    {
        repo = new FileSaver();
        library = repo.Load();
        dataManager = new DataManager(library, repo); //service
    }

    public void SeedData()
    {
        if (library.Books.Any() || library.Members.Any() || library.StaffMembers.Any())
            return;

        library.Books.Add(new Book { BookId = 1, Title = "To Kill a Mockingbird", Author = "Harper Lee", Category = "Fiction" });
        library.Books.Add(new Book { BookId = 2, Title = "1984", Author = "George Orwell", Category = "Dystopian", IsAvailable = false });
        library.Books.Add(new Book { BookId = 3, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Category = "Classic", IsAvailable = false });
        library.Books.Add(new Book { BookId = 4, Title = "Pride and Prejudice", Author = "Jane Austen", Category = "Romance" });
        library.Books.Add(new Book { BookId = 5, Title = "The Hobbit", Author = "J.R.R. Tolkien", Category = "Fantasy" });
        library.Books.Add(new Book { BookId = 6, Title = "The Catcher in the Rye", Author = "J.D. Salinger", Category = "Classic" });
        library.Books.Add(new Book { BookId = 7, Title = "Sapiens: A Brief History of Humankind", Author = "Yuval Noah Harari", Category = "History" });
        library.Books.Add(new Book { BookId = 8, Title = "Becoming", Author = "Michelle Obama", Category = "Biography" });
        library.Books.Add(new Book { BookId = 9, Title = "The Alchemist", Author = "Paulo Coelho", Category = "Philosophical Fiction" });
        library.Books.Add(new Book { BookId = 10, Title = "Atomic Habits", Author = "James Clear", Category = "Self-Help" });

        library.Members.Add(new Member { MemberId = 1, Name = "Ebby", Username = "ebby", Password = "123" });
        library.Members.Add(new Member { MemberId = 2, Name = "Olivia", Username = "olivia", Password = "123" });

        library.StaffMembers.Add(new Staff
        {
            StaffId = 1,
            Name = "Admin",
            Role = "Librarian",
            Username = "admin",
            Password = "123"
        });

        library.StaffMembers.Add(new Staff
        {
            StaffId = 2,
            Name = "Volunteer",
            Role = "Volunteer",
            Username = "volunteer",
            Password = "123"
        });

        library.Borrows.Add(new Borrow
        {
            BorrowId = 1,
            BorrowDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(7),
            ReturnDate = null,
            BookId = 3,
            MemberId = 2,
            StaffId = 1
        });

        library.Borrows.Add(new Borrow
        {
            BorrowId = 2,
            BorrowDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(7),
            ReturnDate = null,
            BookId = 2,
            MemberId = 1,
            StaffId = 1
        });

        repo.Save(library);

    }

    public static string AskForInput(string message)
    {
        return AnsiConsole.Ask<string>(message) ?? string.Empty;
    }

    public static void showMessage(string message)
    {
        if (message.ToLower().Contains("success"))
            AnsiConsole.MarkupLine($"[green]{message}[/]");
        else if (message.ToLower().Contains("error"))
            AnsiConsole.MarkupLine($"[red]{message}[/]");
        else
            AnsiConsole.MarkupLine($"[yellow]{message}[/]");
    }

    public int SelectMember()
    {
        var prompt = new SelectionPrompt<Member>()
            .Title("Select a member")
            .UseConverter(m => $"{m.MemberId} - {m.Name}")
            .AddChoices(library.Members);

        var selectedMember = AnsiConsole.Prompt(prompt);

        AnsiConsole.MarkupLine($"You selected: [green]{selectedMember.Name}[/]");

        return selectedMember.MemberId;
    }

    public int? SelectBorrowForMember(int memberId)
    {
        var borrows = dataManager.GetActiveBorrowsForMember(memberId);

        if (!borrows.Any())
        {
            showMessage("You have no borrowed books.");
            return null;
        }

        var prompt = new SelectionPrompt<Borrow>()
            .Title("Select a book to renew")
            .UseConverter(b =>
            {
                var book = library.Books.First(x => x.BookId == b.BookId);
                return $"{book.Title} (Due: {b.DueDate:d})";
            })
            .AddChoices(borrows);

        var selectedBorrow = AnsiConsole.Prompt(prompt);

        return selectedBorrow.BookId;
    }

    public void Show()
    {

        AnsiConsole.Write(
            new Panel("[bold green]Welcome to the Library Management System[/]")
                .Border(BoxBorder.Double)
                .BorderStyle(new Style(Color.Gold1))
                .Padding(1, 1)
        );

        while (true)
        {
            var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Please select an option")
                .AddChoices("Search Book", "Login", "Exit"));

            AnsiConsole.MarkupLine($"You selected: [green]{choice}[/]");

            if (choice == "Search Book")
            {
                SearchMenu();
            }
            else if (choice == "Login")
            {
                var username = AskForInput("Username: ");

                var res = new TextPrompt<string>("Password:")
                .Secret();

                var password = AnsiConsole.Prompt(res);

                var result = dataManager.Login(username, password);

                showMessage(result.Message);

                if (!result.Success)
                    continue;

                if (result.Member != null)
                    MemberMenu(result.Member);
                else if (result.Staff != null)
                    StaffMenu(result.Staff);
            }
            else if (choice == "Exit")
                break;
        }
    }

    public void SearchMenu()
    {
        var searchType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Search by:")
                .AddChoices("Title", "Author", "Category", "All")); // Removing "Any" Ebby

        AnsiConsole.MarkupLine($"You selected: [green]{searchType}[/]");

        var keyword = "*";

        if (searchType != "All")
            keyword = AskForInput("Enter search keyword:");

        var message = dataManager.Search(keyword, searchType);

        showMessage(message);
    }

    void MemberMenu(Member member)
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Please select an option")
                .AddChoices("Search Book", "Renew Book", "Logout"));

            AnsiConsole.MarkupLine($"You selected: [green]{choice}[/]");


            if (choice == "Search Book")
            {
                SearchMenu();
            }
            else if (choice == "Renew Book")
            {
                var bookId = SelectBorrowForMember(member.MemberId);

                if (bookId != null)
                {
                    var message = dataManager.RenewBook(member.MemberId, bookId.Value);
                    showMessage(message);
                }
                // Showing list instead of prompting for ID
                //{
                //var message = dataManager.RenewBook(member.MemberId, int.Parse(AskForInput("Enter Book ID: ")));
                //showMessage(message);
                //}
            }
            else break;
        }
    }

    void StaffMenu(Staff staff)
    {
        while (true)
        {
            var choice = "";
            if(staff.Role == "Librarian")
            {
                choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Please select an option")
                        .AddChoices("Search Book", "Borrow Book", "Return Book", "Logout"));
            }
            else if(staff.Role == "Volunteer")
            {
                choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Please select an option")
                        .AddChoices("Manage Book", "Logout"));
            }
            AnsiConsole.MarkupLine($"You selected: [green]{choice}[/]");

            if (choice == "Search Book")
            {
                SearchMenu();
            }
            else if (choice == "Borrow Book")
            {
                //int m = int.Parse(AskForInput("Member ID: "));
                int m = SelectMember();
                int b = int.Parse(AskForInput("Book ID: "));
                var message = dataManager.BorrowBook(m, b, staff.StaffId);
                showMessage(message);
            }
            else if (choice == "Return Book")
            {
                var message = dataManager.ReturnBook(int.Parse(AskForInput("Book ID: ")));
                showMessage(message);
            }
            else if (choice == "Manage Book")
            {
                // TBD
                showMessage("This feature is coming soon!");
            }
            else break;
        }
    }
}
