namespace LibraryManagementSystem;
using System;
using System.Linq;
using Spectre.Console;

public class DataManager
{
    private readonly Library _library;
    private readonly FileSaver _repo;

    public DataManager(Library library, FileSaver repo)
    {
        _library = library;
        _repo = repo;
    }

    // Method to authenticate Users and retrun if its Member or Staff
    public LoginResult Login(string username, string password)
    {
        var member = _library.Members
            .FirstOrDefault(m => m.Username == username && m.Password == password);

        if (member != null)
        {
            return new LoginResult
            {
                Success = true,
                Member = member,
                Message = $"Welcome Member, {member.Name}!"
            };
        }

        var staff = _library.StaffMembers
            .FirstOrDefault(s => s.Username == username && s.Password == password);

        if (staff != null)
        {
            return new LoginResult
            {
                Success = true,
                Staff = staff,
                Message = $"Welcome {staff.Name}! Your Role is {staff.Role}."
            };
        }

        return new LoginResult
        {
            Success = false,
            Message = "Error: Invalid username or password."
        };
    }

    // Method to search books based on the user input/key word and type (Title, Author etc)
    public string Search(string keyword, string searchType)
    {
        IEnumerable<Book> results = new List<Book>();

        if (searchType == "Title")
        {
            results = _library.Books
                .Where(b => b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        else if (searchType == "Author")
        {
            results = _library.Books
                .Where(b => b.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        else if (searchType == "Category")
        {
            results = _library.Books
                .Where(b => b.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        else if (searchType == "Any")
        {
            results = _library.Books
                .Where(b =>
                    b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    b.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        else // This displays all the books in the linrary
        {
            results = _library.Books;
        }

        var resultList = results.ToList();

        if (!resultList.Any())
            return "No books found.";

        var table = new Table();

        table.Border = TableBorder.Rounded;

        table.AddColumn("[yellow]Book ID[/]");
        table.AddColumn("[yellow]Title[/]");
        table.AddColumn("[yellow]Author[/]");
        table.AddColumn("[yellow]Category[/]");
        table.AddColumn("[yellow]Status[/]");
        table.AddColumn("[yellow]Due Date[/]");

        foreach (var book in resultList)
        {

            var borrow = _library.Borrows
                            .FirstOrDefault(b => b.BookId == book.BookId && b.ReturnDate == null);

            string status = "[green]Available[/]";
            string due = "-";

            if (borrow != null)
            {
                due = borrow.DueDate.ToShortDateString();
                status = borrow.DueDate < DateTime.Now ? "[red]Overdue[/]" : "[yellow]Borrowed[/]";
            }

            table.AddRow(
                book.BookId.ToString(),
                book.Title,
                book.Author,
                book.Category,
                status,
                due
            );
        }

        AnsiConsole.Write(table);

        return $"{resultList.Count} book(s) found.";
    }

    // This method executes a book borrow/loan transaction
    // This is for Staff Only
    public string BorrowBook(int memberId, int bookId, int staffId)
    {
        var member = _library.Members.FirstOrDefault(m => m.MemberId == memberId);
        if (member == null)
            return "Error: Member not found.";

        var book = _library.Books.FirstOrDefault(b => b.BookId == bookId);
        if (book == null)
            return "Error: Book not found.";

        if (!book.IsAvailable)
            return "Error: Book is already borrowed.";

        var borrow = new Borrow
        {
            BorrowId = _library.Borrows.Count + 1,
            BookId = bookId,
            MemberId = memberId,
            StaffId = staffId,
            BorrowDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(14),
            ReturnDate = null
        };

        book.IsAvailable = false;
        _library.Borrows.Add(borrow);

        _repo.Save(_library);

        return $"Book '{book.Title}' successfully borrowed. Due date: {borrow.DueDate:d}";
    }

    // This method executes a book return transaction
    // This is for Staff Only
    public string ReturnBook(int bookId)
    {
        var borrow = _library.Borrows
                        .FirstOrDefault(l => l.BookId == bookId && l.ReturnDate == null);

        if (borrow == null)
        {
            var this_book = _library.Books
                        .FirstOrDefault(l => l.BookId == bookId);

            if (this_book == null)
                return "Error: Book not found.";
            else
                return "Error: Book is not currently borrowed or already returned.";
        }
        else {    

            borrow.ReturnDate = DateTime.Now;

            var book = _library.Books.First(b => b.BookId == bookId);
            book.IsAvailable = true;

            _repo.Save(_library);

            return $"Book '{book.Title}' returned successfully.";
        }
    }

    // Methof to get all active book loans/borrows for a specific member
    public List<Borrow> GetActiveBorrowsForMember(int memberId)
    {
        return _library.Borrows
            .Where(b => b.MemberId == memberId && b.ReturnDate == null)
            .ToList();
    }

    // This method executes a book renewal transaction
    // This is for Member Only
    public string RenewBook(int memberId, int bookId)
    {
        var borrow = _library.Borrows
            .FirstOrDefault(l => l.BookId == bookId &&
                                l.MemberId == memberId &&
                                l.ReturnDate == null);

        if (borrow == null)
            return "Error: Book is not currently borrowed.";

        if (borrow.DueDate < DateTime.Now)
            return $"Error: Book is overdue, and cannot be renewed. Your due date was {borrow.DueDate.ToShortDateString()}.";

        if (borrow.RenewCount >= 2)
        {
            return "Error: Book is renewed two times, renewal limit reached.";
        }

        borrow.DueDate = borrow.DueDate.AddDays(7);
        borrow.RenewCount++;

        _repo.Save(_library);

        return $"Renewal successful. New due date: {borrow.DueDate:d}";
    }
}