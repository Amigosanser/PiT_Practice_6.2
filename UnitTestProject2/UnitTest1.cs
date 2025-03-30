using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Courswork;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace UnitTestProject2
{
    [TestClass]
    public class LoginPageTests
    {
        private const string TestUsername = "testuser";
        private const string TestPassword = "testpass123";
        private string _credentialsFilePath;

        [TestInitialize]
        public void TestInitialize()
        {
            _credentialsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "login_credentials_test.txt");

            if (File.Exists(_credentialsFilePath))
            {
                File.Delete(_credentialsFilePath);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_credentialsFilePath))
            {
                File.Delete(_credentialsFilePath);
            }
        }

        [TestMethod]
        public void GetHash_ReturnsCorrectSHA1Hash()
        {
            var input = "password123";
            var expectedHash = ComputeExpectedSHA1Hash(input);

            var actualHash = LoginPage.GetHash(input);

            Assert.AreEqual(expectedHash, actualHash);
        }

        [TestMethod]
        public void SaveCredentials_CreatesFileWithCorrectContent()
        {
            var loginPage = new LoginPage();
            var privateObject = new PrivateObject(loginPage);
            privateObject.SetField("_credentialsFilePath", _credentialsFilePath);

            privateObject.Invoke("SaveCredentials", TestUsername, TestPassword);

            Assert.IsTrue(File.Exists(_credentialsFilePath));
            var lines = File.ReadAllLines(_credentialsFilePath);
            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual(TestUsername, lines[0]);
            Assert.AreEqual(TestPassword, lines[1]);
        }

        [TestMethod]
        public void ClearCredentials_RemovesFile()
        {
            File.WriteAllLines(_credentialsFilePath, new[] { TestUsername, TestPassword });
            var loginPage = new LoginPage();
            var privateObject = new PrivateObject(loginPage);
            privateObject.SetField("_credentialsFilePath", _credentialsFilePath);

            privateObject.Invoke("ClearCredentials");

            Assert.IsFalse(File.Exists(_credentialsFilePath));
        }

        private string ComputeExpectedSHA1Hash(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Concat(hashBytes.Select(b => b.ToString("X2")));
            }
        }
    }
}