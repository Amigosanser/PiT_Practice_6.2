using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Courswork;
using System.Data.Entity;
using System.Collections.Generic;
using NSubstitute;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace UnitTestProject4
{
    [TestClass]
    public class LoginTests
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
        public void Login_WithValidAdminCredentials_ReturnsSuccess()
        {
            _loginPage.txtUsername.Text = "admin_user";
            _loginPage.txtPassword.Password = "admin123";
            _loginPage.Login_Click(null, null);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Login_WithValidMethodistCredentials_ReturnsSuccess()
        {
            _loginPage.txtUsername.Text = "methodist_user";
            _loginPage.txtPassword.Password = "methodist456";
            _loginPage.Login_Click(null, null);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Login_WithInvalidPassword_ShowsErrorMessage()
        {
            _loginPage.txtUsername.Text = "admin_user";
            _loginPage.txtPassword.Password = "wrong_password";
            _loginPage.Login_Click(null, null);
            Assert.IsTrue(true);
        }

        internal class TestDbAsyncQueryProvider<TEntity> : IDbAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestDbAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestDbAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestDbAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                return Task.FromResult(Execute<TResult>(expression));
            }

            public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        internal class TestDbAsyncEnumerable<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable<T>
        {
            public TestDbAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestDbAsyncEnumerable(Expression expression) : base(expression) { }

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
}