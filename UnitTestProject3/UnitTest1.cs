using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Courswork;
using System.Data.Entity;
using System.Collections.Generic;
using NSubstitute;

namespace UnitTestProject3
{
    [TestClass]
    public class SignUpPageTests
    {
        private SignUpPage _signUpPage;
        private List<Users> _testUsers;
        private List<Parents> _testParents;
        private MPID4855073Entities _fakeDbContext;

        [TestInitialize]
        public void TestInitialize()
        {
            _testUsers = new List<Users>();
            _testParents = new List<Parents>
            {
                new Parents { ParentID = 1, Name = "Родитель 1", ContactInfo = "contact1" },
                new Parents { ParentID = 2, Name = "Родитель 2", ContactInfo = "contact2" }
            };

            _fakeDbContext = Substitute.For<MPID4855073Entities>();

            var fakeUsersSet = Substitute.For<DbSet<Users>, IQueryable<Users>>();
            ((IQueryable<Users>)fakeUsersSet).Provider.Returns(_testUsers.AsQueryable().Provider);
            ((IQueryable<Users>)fakeUsersSet).Expression.Returns(_testUsers.AsQueryable().Expression);
            ((IQueryable<Users>)fakeUsersSet).ElementType.Returns(_testUsers.AsQueryable().ElementType);
            ((IQueryable<Users>)fakeUsersSet).GetEnumerator().Returns(_testUsers.AsQueryable().GetEnumerator());
            fakeUsersSet.When(x => x.Add(Arg.Any<Users>())).Do(x => _testUsers.Add(x.Arg<Users>()));
            _fakeDbContext.Users.Returns(fakeUsersSet);

            var fakeParentsSet = Substitute.For<DbSet<Parents>, IQueryable<Parents>>();
            ((IQueryable<Parents>)fakeParentsSet).Provider.Returns(_testParents.AsQueryable().Provider);
            ((IQueryable<Parents>)fakeParentsSet).Expression.Returns(_testParents.AsQueryable().Expression);
            ((IQueryable<Parents>)fakeParentsSet).ElementType.Returns(_testParents.AsQueryable().ElementType);
            ((IQueryable<Parents>)fakeParentsSet).GetEnumerator().Returns(_testParents.AsQueryable().GetEnumerator());
            _fakeDbContext.Parents.Returns(fakeParentsSet);

            _fakeDbContext.Methodists.Returns(Substitute.For<DbSet<Methodists>>());
            _fakeDbContext.Children.Returns(Substitute.For<DbSet<Children>>());

            _fakeDbContext.SaveChanges().Returns(1);

            _signUpPage = new SignUpPage();

            typeof(SignUpPage).GetField("db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_signUpPage, _fakeDbContext);
        }
    }
}