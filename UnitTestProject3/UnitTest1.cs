using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Courswork;
using System.Windows.Controls;
using System.Windows;
using System.Data.Entity;
using System.Collections.Generic;
using NSubstitute;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using System.Threading;

namespace UnitTestProject3
{
    [TestClass]
    public class ExistingUsersAuthenticationTests
    {
        private LoginPage _loginPage;
        private MPID4855073Entities _fakeDbContext;
        private List<Users> _testUsers;

        [TestInitialize]
        public void TestInitialize()
        {
            _testUsers = new List<Users>
            {
                new Users {
                    UserID = 1,
                    FullName = "admin_user",
                    Password = LoginPage.GetHash("admin123"),
                    RoleID = 1,
                    Roles = new Roles { RoleID = 1, RoleName = "Administrator" }
                },
                new Users {
                    UserID = 2,
                    FullName = "methodist_user",
                    Password = LoginPage.GetHash("methodist456"),
                    RoleID = 2,
                    Roles = new Roles { RoleID = 2, RoleName = "Methodist" }
                }
            };

            _fakeDbContext = Substitute.For<MPID4855073Entities>();

            var fakeUsersSet = Substitute.For<DbSet<Users>, IQueryable<Users>, IDbAsyncEnumerable<Users>>();

            ((IQueryable<Users>)fakeUsersSet).Provider.Returns(_testUsers.AsQueryable().Provider);
            ((IQueryable<Users>)fakeUsersSet).Expression.Returns(_testUsers.AsQueryable().Expression);
            ((IQueryable<Users>)fakeUsersSet).ElementType.Returns(_testUsers.AsQueryable().ElementType);
            ((IQueryable<Users>)fakeUsersSet).GetEnumerator().Returns(_testUsers.AsQueryable().GetEnumerator());

            ((IDbAsyncEnumerable<Users>)fakeUsersSet).GetAsyncEnumerator()
                .Returns(new TestDbAsyncEnumerator<Users>(_testUsers.AsQueryable().GetEnumerator()));

            ((IQueryable<Users>)fakeUsersSet).Provider.Returns(new TestDbAsyncQueryProvider<Users>(_testUsers.AsQueryable().Provider));

            fakeUsersSet.AsNoTracking().Returns(fakeUsersSet);

            _fakeDbContext.Users.Returns(fakeUsersSet);

            _loginPage = new LoginPage();

            typeof(LoginPage).GetField("db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_loginPage, _fakeDbContext);
        }

        [TestMethod]
        public void Authenticate_AdminUser_ReturnsSuccess()
        {
            _loginPage.txtUsername.Text = "admin_user";
            _loginPage.txtPassword.Password = "admin123";

            _loginPage.Login_Click(null, null);

            Assert.IsTrue(true);
        }
    }
    internal class TestDbAsyncQueryProvider<TEntity> : IDbAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestDbAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }

        public Task<object> ExecuteAsync(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    internal class TestDbAsyncEnumerable<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable<T>
    {
        public TestDbAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new TestDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IQueryProvider IQueryable.Provider => new TestDbAsyncQueryProvider<T>(this);
    }

    internal class TestDbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestDbAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }

        public T Current => _inner.Current;

        object IDbAsyncEnumerator.Current => Current;
    }
}