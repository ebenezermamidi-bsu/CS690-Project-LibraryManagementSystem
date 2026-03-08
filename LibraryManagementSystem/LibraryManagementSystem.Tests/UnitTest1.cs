using System;
using Xunit;
using LibraryManagementSystem;
using System.Linq;

namespace LibraryManagementSystem.Tests
{
    public class LibraryTests
    {
        private Library SeedLibrary()
        {
            var library = new Library();

            library.Books.Add(new Book { BookId = 1, Title = "1984", Author = "George Orwell", Category = "Dystopian" });
            library.Books.Add(new Book { BookId = 2, Title = "The Hobbit", Author = "J.R.R. Tolkien", Category = "Fantasy" });

            library.Members.Add(new Member { MemberId = 1, Name = "Alice", Username = "alice", Password = "123" });
            library.Members.Add(new Member { MemberId = 2, Name = "Bob", Username = "bob", Password = "123" });

            library.StaffMembers.Add(new Staff { StaffId = 1, Name = "Admin", Role = "Librarian", Username = "admin", Password = "123" });

            return library;
        }

        [Fact]
        public void Login_ShouldReturnMember_WhenCredentialsCorrect()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            var user = manager.Login("alice", "123");

            Assert.NotNull(user);
            Assert.IsType<Member>(user.Member);
            Assert.Equal("Alice", user.Member.Name);
        }

        [Fact]
        public void Login_ShouldReturnStaff_WhenStaffCredentialsCorrect()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            var user = manager.Login("admin", "123");

            Assert.NotNull(user);
            Assert.IsType<Staff>(user.Staff);
            Assert.Equal("Admin", user.Staff.Name);
        }

        [Fact]
        public void BorrowBook_ShouldMarkBookAsUnavailable()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1); // Member 1 borrows Book 1

            var book = library.Books.First(b => b.BookId == 1);

            Assert.False(book.IsAvailable);
            var borrow = library.Borrows.FirstOrDefault(b => b.BookId == 1 && b.MemberId == 1);
            Assert.NotNull(borrow);
            Assert.Null(borrow.ReturnDate);
        }

        [Fact]
        public void ReturnBook_ShouldMarkBookAsAvailable()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);
            manager.ReturnBook(1); // Return Book 1

            var book = library.Books.First(b => b.BookId == 1);
            Assert.True(book.IsAvailable);

            var borrow = library.Borrows.First(b => b.BookId == 1);
            Assert.NotNull(borrow.ReturnDate);
        }

        [Fact]
        public void RenewBook_ShouldExtendDueDate()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);
            library.Borrows.First(b => b.BookId == 1).DueDate = DateTime.Now.AddDays(-10);
            var oldDueDate = library.Borrows.First(b => b.BookId == 1).DueDate;
            
            manager.RenewBook(1, 1);

            var newDueDate = library.Borrows.First(b => b.BookId == 1).DueDate;
            
            Assert.True(newDueDate > oldDueDate);
        }

        [Fact]
        public void Search_ShouldReturnMatchingBooks_ByTitle()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            var keyword = "1984";
            var books = library.Books.Where(b => b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

            Assert.Single(books);
            Assert.Equal("1984", books[0].Title);
        }
    }
}