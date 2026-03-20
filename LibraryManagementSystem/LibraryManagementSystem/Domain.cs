namespace LibraryManagementSystem;
using System.Collections.Generic;

public class Book
{
    public int BookId { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Category { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string Location { get; set; } = "A5";
}

public class Member
{
    public int MemberId { get; set; }
    public required string Name { get; set; }

    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class Staff
{
    public int StaffId { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class Borrow
{
    public int BorrowId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public int StaffId { get; set; } 
    public int RenewCount { get; set; } = 0;
}

public class Library
{
    public List<Book> Books { get; set; } = new();
    public List<Member> Members { get; set; } = new();
    public List<Staff> StaffMembers { get; set; } = new();
    public List<Borrow> Borrows { get; set; } = new();
}

public class LoginResult
{
    public bool Success { get; set; }

    public Member? Member { get; set; }

    public Staff? Staff { get; set; }

    public string Message { get; set; } = "";
}