﻿using MUNityAngular.Services;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MUNityTest.DatabaseTestTools;

namespace MUNityTest.ServiceTests
{
    public class AuthServiceTest
    {
        //Change the password if needed localy, keep in mind that inside the git-Action the password needs to be root!
        private string _connectionString = @"server=127.0.0.1;userid=root;password='root'";
        private string test_database_name = "munity-test";

        [SetUp]
        public void Setup()
        {
            //Create the empty Database
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                //Vorhandene Datenbank entfernen
                var cmdStr = "DROP DATABASE IF EXISTS `" + test_database_name + "`";
                var cmdDrop = new MySqlCommand(cmdStr, connection);
                cmdDrop.ExecuteNonQuery();

                cmdStr = "CREATE DATABASE `" + test_database_name + "`";
                var cmdCreate = new MySqlCommand(cmdStr, connection);
                cmdCreate.ExecuteNonQuery();

                //Wechseln auf die neue Datenbank
                connection.ChangeDatabase(test_database_name);
                this._connectionString += ";database=" + test_database_name;

                //Schaue wo die munitysql hin kopiert wird...
                var path = Path.Combine(Environment.CurrentDirectory, "NeededFiles/munity.sql");
                var sql = File.ReadAllText(path);
                cmdStr = sql;
                var cmdFill = new MySqlCommand(sql, connection);
                var filled = cmdFill.ExecuteNonQuery();
                Console.WriteLine(filled + " Test Data added");
            }
        }

        [Test]
        public void RegistrationTest()
        {
            var service = new AuthService(_connectionString);
            service.Register("test", "password", "mail@domain.ttl");

            var result = Tools.Connection(_connectionString).Table("user").HasEntry("username", "test");
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckLoginDataTest()
        {
            var service = new AuthService(_connectionString);
            service.Register("test", "password", "mail@domain.ttl");

            var login = service.CheckLoginData("test", "password");
            Assert.IsTrue(login.valid);
            var failTest = service.CheckLoginData("test", "not_password");
            Assert.IsFalse(failTest.valid);
        }

        [Test]
        public void ChangePasswordTest()
        {
            var service = new AuthService(_connectionString);
            service.Register("test", "password", "mail@domain.ttl");

            var login = service.CheckLoginData("test", "password");
            Assert.IsTrue(login.valid);
            service.SetPassword(login.userid, "newpassword");

            var retry = service.CheckLoginData("test", "newpassword");
            Assert.IsTrue(retry.valid);
            var negativeTest = service.CheckLoginData("test", "password");
            Assert.IsFalse(negativeTest.valid);
        }

        [Test]
        public void UsernameAvailableTest()
        {
            var service = new AuthService(_connectionString);
            Assert.IsTrue(service.UsernameAvailable("test"));
            service.Register("test", "password", "mail@domain.ttl");
            Assert.IsFalse(service.UsernameAvailable("test"));
        }
    }
}
