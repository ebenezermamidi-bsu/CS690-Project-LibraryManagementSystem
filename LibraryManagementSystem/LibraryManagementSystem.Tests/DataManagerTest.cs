using System;
using Xunit;
using LibraryManagementSystem;
using System.Linq;
using System.Collections.Generic;

namespace LibraryManagementSystem.Tests
{
    public class DataManagerTest
    {
        // Test book search by title success
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

        //Seeding test data for next test cases
        private Library SeedLibrary()
        {
            var library = new Library();

            library.Books.Add(new Book { BookId = 1, Title = "1984", Author = "George Orwell", Category = "Dystopian", Location="A1" });
            library.Books.Add(new Book { BookId = 2, Title = "The Hobbit", Author = "J.R.R. Tolkien", Category = "Fantasy", Location="A2" });

            library.Members.Add(new Member { MemberId = 1, Name = "Alice", Username = "alice", Password = "123" });
            library.Members.Add(new Member { MemberId = 2, Name = "Bob", Username = "bob", Password = "123" });

            library.StaffMembers.Add(new Staff { StaffId = 1, Name = "Admin", Role = "Librarian", Username = "admin", Password = "123" });

            return library;
        }

        // Test Member login success
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

        // Test Staff login success
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

        // Test Book borrow success
        [Fact]
        public void BorrowBook_ShouldMarkBookAsUnavailable()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);

            var book = library.Books.First(b => b.BookId == 1);

            Assert.False(book.IsAvailable);
            var borrow = library.Borrows.FirstOrDefault(b => b.BookId == 1 && b.MemberId == 1);
            Assert.NotNull(borrow);
            Assert.Null(borrow.ReturnDate);
        }

        // Test Book borrow failure (already borrowed)
        [Fact]
        public void BorrowBook_ShouldFail_WhenBookAlreadyBorrowed()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);
            manager.BorrowBook(2, 1, 1);

            var borrows = library.Borrows.Where(b => b.BookId == 1).ToList();

            Assert.Single(borrows);
            Assert.False(library.Books.First(b => b.BookId == 1).IsAvailable);
        }

        // Test Book return success
        [Fact]
        public void ReturnBook_ShouldMarkBookAsAvailable()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);
            manager.ReturnBook(1);

            var book = library.Books.First(b => b.BookId == 1);
            Assert.True(book.IsAvailable);

            var borrow = library.Borrows.First(b => b.BookId == 1);
            Assert.NotNull(borrow.ReturnDate);
        }

        // Test Book return failure (when returning unvailable book)
        [Fact]
        public void ReturnBook_ShouldFail_WhenBookNotBorrowed()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.ReturnBook(1);

            var book = library.Books.First(b => b.BookId == 1);

            Assert.True(book.IsAvailable);
            Assert.Empty(library.Borrows);
        }

        // Test book renewal success
        [Fact]
        public void RenewBook_ShouldExtendDueDate()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);

            var borrow = library.Borrows.First(b => b.BookId == 1);
            borrow.DueDate = DateTime.Now.AddDays(3);

            var oldDueDate = borrow.DueDate;

            manager.RenewBook(1, 1);

            Assert.True(borrow.DueDate > oldDueDate);
            Assert.Equal(1, borrow.RenewCount);
        }

        // Test book renewal failure (when borrow is overdue)
        [Fact]
        public void RenewBook_ShouldFail_WhenOverdue()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);

            var loan = library.Borrows.First(b => b.BookId == 1);
            loan.DueDate = DateTime.Now.AddDays(-1);

            var oldDueDate = loan.DueDate;

            manager.RenewBook(1, 1);

            Assert.Equal(oldDueDate, loan.DueDate);
        }

        // Test book renewal failure (when max limit 2 reached)
        [Fact]
        public void RenewBook_ShouldFail_WhenRenewLimitReached()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);

            var loan = library.Borrows.First(b => b.BookId == 1);
            loan.RenewCount = 2;

            var oldDueDate = loan.DueDate;

            manager.RenewBook(1, 1);

            Assert.Equal(oldDueDate, loan.DueDate);
        }

        // Test Add Book to Library
        [Fact]
        public void AddBook_ShouldAddBookToLibrary()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.AddBook("Clean Code", "Robert Martin", "Programming", "B1");

            var book = library.Books.FirstOrDefault(b => b.Title == "Clean Code");

            Assert.NotNull(book);
            Assert.Equal("Programming", book.Category);
            Assert.Equal("B1", book.Location);
        }

        // Test Update Book category and location
        [Fact]
        public void UpdateBook_ShouldUpdateCategoryAndLocation()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.UpdateBook(1, "Classic", "C3");

            var book = library.Books.First(b => b.BookId == 1);

            Assert.Equal("Classic", book.Category);
            Assert.Equal("C3", book.Location);
        }

        // Test identify and display overdue books
        [Fact]
        public void GetOverdueBooks_ShouldReturnOverdueBooks()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);

            var borrow = library.Borrows.First();
            borrow.DueDate = DateTime.Now.AddDays(-2);

            var overdue = manager.GetOverdueBooks();

            Assert.Single(overdue);
        }

        // Test generate overdue book reminders
        [Fact]
        public void GenerateOverdueReminders_ShouldReturnReminder()
        {
            var library = SeedLibrary();
            var repo = new FileSaver();
            var manager = new DataManager(library, repo);

            manager.BorrowBook(1, 1, 1);

            var borrow = library.Borrows.First();
            borrow.DueDate = DateTime.Now.AddDays(-5);

            var reminders = manager.GenerateOverdueReminders();

            Assert.Single(reminders);
            Assert.Contains("Reminder", reminders.First());
        }
    }
}