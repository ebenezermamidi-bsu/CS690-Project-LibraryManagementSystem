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

    // LOGIN
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
            Message = "Invalid username or password."
        };
    }

    // SEARCH
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
        else // All
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

        foreach (var book in resultList)
        {
            table.AddRow(
                book.BookId.ToString(),
                book.Title,
                book.Author,
                book.Category,
                book.IsAvailable ? "[green]Available[/]" : "[red]Borrowed[/]"
            );
        }

        AnsiConsole.Write(table);

        return $"{resultList.Count} book(s) found.";
    }

    // BORROW (Staff Only)
    public string BorrowBook(int memberId, int bookId, int staffId)
    {
        var member = _library.Members.FirstOrDefault(m => m.MemberId == memberId);
        if (member == null)
            return "Member not found.";

        var book = _library.Books.FirstOrDefault(b => b.BookId == bookId);
        if (book == null)
            return "Book not found.";

        if (!book.IsAvailable)
            return "Book is already borrowed.";

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

    public string ReturnBook(int bookId)
    {
        var borrow = _library.Borrows
            .FirstOrDefault(l => l.BookId == bookId && l.ReturnDate == null);

        if (borrow == null)
            return "Active borrow record not found.";

        borrow.ReturnDate = DateTime.Now;

        var book = _library.Books.First(b => b.BookId == bookId);
        book.IsAvailable = true;

        _repo.Save(_library);

        return $"Book '{book.Title}' returned successfully.";
    }

    public List<Borrow> GetActiveBorrowsForMember(int memberId)
    {
        return _library.Borrows
            .Where(b => b.MemberId == memberId && b.ReturnDate == null)
            .ToList();
    }

    public string RenewBook(int memberId, int bookId)
    {
        var borrow = _library.Borrows
            .FirstOrDefault(l => l.BookId == bookId &&
                                l.MemberId == memberId &&
                                l.ReturnDate == null);

        if (borrow == null)
            return "Borrow record not found.";

        if (borrow.DueDate < DateTime.Now)
            return "Cannot renew. Book is already overdue.";

        borrow.DueDate = borrow.DueDate.AddDays(7);

        _repo.Save(_library);

        return $"Renewal successful. New due date: {borrow.DueDate:d}";
    }
}