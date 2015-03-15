﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using DbConnectionsManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestErrorHandlerEngine.DbConnectionManager
{
    [TestClass]
    public class UnitTest
    {
        private Connection _conn;
        private Connection _connHost;
        private Connection _connJustTrueServer;
        private Connection _connFalse;



        [TestInitialize]
        public void UnitTestInitializer()
        {
            _conn = new Connection(".", "UsersManagements") { Name = "UM" };
            _connHost = new Connection("localhost", "UsersManagements") { Name = "UM" };
            _connJustTrueServer = new Connection("localhost", "TestNotExistDbName") { Name = "Test" };
            _connFalse = new Connection("TestNotExistServer", "TestNotExistDbName") { Name = "Test" };
        }


        #region Test StringCipher.cs

        [TestMethod, TestCategory("StringCipher.cs")]
        public void RandomPassword()
        {
            for (int i = 0; i < 1000; i++)
            {
                var password = GetHexId(10);

                var plaintext = GetRandomText(10000);

                var encryptedstring = plaintext.Encrypt(password);

                var decryptedstring = encryptedstring.Decrypt(password);

                Assert.AreEqual(plaintext, decryptedstring);
            }
        }

        [TestMethod, TestCategory("StringCipher.cs")]
        public void PublicPassword()
        {
            for (int i = 0; i < 1000; i++)
            {
                var plaintext = GetRandomText(10000);

                var encryptedstring = plaintext.Encrypt();


                var decryptedstring = encryptedstring.Decrypt();

                Assert.AreEqual(plaintext, decryptedstring);
            }
        }

        [TestMethod, TestCategory("StringCipher.cs")]
        public void RandomPasswordUtf8()
        {
            for (int i = 0; i < 1000; i++)
            {
                var password = GetHexId(10);

                var plaintext = GetRandomTextUtf8(10000);

                var encryptedstring = StringCipher.Encrypt(plaintext, password);

                var decryptedstring = StringCipher.Decrypt(encryptedstring, password);

                Assert.AreEqual(plaintext, decryptedstring);
            }
        }

        [TestMethod, TestCategory("StringCipher.cs")]
        public void PublicPasswordUtf8()
        {
            for (int i = 0; i < 1000; i++)
            {
                var plaintext = GetRandomTextUtf8(10000);

                var encryptedstring = plaintext.Encrypt();


                var decryptedstring = encryptedstring.Decrypt();

                Assert.AreEqual(plaintext, decryptedstring);
            }
        }


        protected string GetHexId(int len)
        {
            var chars = "ABCDEF0123456789";

            var random = new System.Random();

            var result = new string(
                Enumerable.Repeat(chars, len)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());


            return result;
        }

        protected string GetRandomText(int len)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var random = new System.Random();

            var result = new string(
                Enumerable.Repeat(chars, random.Next(1, len))
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            return result;
        }

        protected string GetRandomTextUtf8(int len)
        {
            const string chars = @"پضصثقفغعهخحجچگکمنتاآلبیسشظطزرذدئو./ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-|\";

            var random = new System.Random();

            var result = new string(
                Enumerable.Repeat(chars, random.Next(1, len))
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            return result;
        }

        #endregion


        #region Test Connection.cs

        [TestMethod, TestCategory("Connection.cs")]
        public void CreateConnection1Args()
        {
            //
            // --------------- Constructor 1 Arguments ------------------------------------------
            //
            var connFull = new Connection(".", "UsersManagements", "um");
            var conn = new Connection(connFull);

            Assert.AreEqual(conn.ToXml(false).ToString(SaveOptions.None), conn.ToString());
            Assert.AreEqual(conn.ToXml(true).ToString(SaveOptions.None), conn.ToString(true));
            var newConnWithoutId = conn.ToXml(true);
            TestTools.ExceptException<NullReferenceException>(() => newConnWithoutId.Element("add").Attribute("publicKeyToken").Value = "");
            var c = Connection.Parse(newConnWithoutId);
            Assert.AreEqual(c.ConnectionString, conn.ConnectionString); // c.Id != conn.Id but both of them have duplicate connectionString

            var conn2 = new Connection(conn.ConnectionString);

            TestTools.ExpectException<NullReferenceException>(() => conn.ConnectionString = null); // Test throw NullReferenceException

            Assert.AreEqual(conn.ConnectionString, conn2.ConnectionString);
        }

        [TestMethod, TestCategory("Connection.cs")]
        public void CreateConnection2Args()
        {
            //
            // --------------- Constructor 2 Arguments ------------------------------------------
            //
            var connFull = new Connection(".", "UsersManagements", "sa", "123", 3, "Test Connection", "um");
            var conn = new Connection(connFull.ConnectionString);

            Assert.AreEqual(conn.ToXml(false).ToString(SaveOptions.None), conn.ToString());
            Assert.AreEqual(conn.ToXml(true).ToString(SaveOptions.None), conn.ToString(true));
            Assert.AreEqual(
                Connection.Parse(conn.ToXml(true)).ConnectionString,
                conn.ConnectionString);

            var conn2 = new Connection(conn.ConnectionString);

            Assert.AreEqual(conn.ConnectionString, conn2.ConnectionString);
        }

        [TestMethod, TestCategory("Connection.cs")]
        public void CreateConnection3Args()
        {
            //
            // --------------- Constructor 3 Arguments ------------------------------------------
            //
            var conn1 = new Connection(".", "UsersManagements");
            conn1.Name = "UM";
            Assert.AreEqual(conn1.ToXml().ToString(SaveOptions.None), conn1.ToString());
            Assert.AreEqual(conn1.ToXml(true).ToString(SaveOptions.None), conn1.ToString(true));
            Assert.AreEqual(
                Connection.Parse(conn1.ToXml(true)).ConnectionString,
                conn1.ConnectionString);
        }

        [TestMethod, TestCategory("Connection.cs")]
        public void CreateConnection4Args()
        {
            //
            // --------------- Constructor 4 Arguments ------------------------------------------
            //
            var conn = new Connection();

            Assert.AreEqual(conn.ToXml().ToString(SaveOptions.None), conn.ToString());
            Assert.AreEqual(conn.ToXml(true).ToString(SaveOptions.None), conn.ToString(true));
            Assert.AreEqual(
                Connection.Parse(conn.ToXml(true)).ConnectionString,
                conn.ConnectionString);


            var conn1_2 = new Connection()
            {
                ConnectionString = conn.ConnectionString
            };

            Assert.AreEqual(conn.ConnectionString, conn1_2.ConnectionString);
        }

        [TestMethod, TestCategory("Connection.cs")]
        public void CreateConnection6Args()
        {
            //
            // --------------- Constructor 6 Arguments ------------------------------------------
            //
            var conn2 = new Connection(".", "UsersManagements", "sa", "123", -3);

            Assert.AreEqual(conn2.ToXml().ToString(SaveOptions.None), conn2.ToString());
            Assert.AreEqual(conn2.ToXml(true).ToString(SaveOptions.None), conn2.ToString(true));
            Assert.AreEqual(
                Connection.Parse(conn2.ToXml(true)).ConnectionString,
                conn2.ConnectionString);


            var conn2_2 = new Connection()
            {
                ConnectionString = conn2.ConnectionString
            };

            Assert.AreEqual(conn2.ConnectionString, conn2_2.ConnectionString);
        }

        [TestMethod, TestCategory("Connection.cs")]
        public void CreateConnection8Args()
        {
            //
            // --------------- Constructor 8 Arguments ------------------------------------------
            //
            var conn3 = new Connection(".", "UsersManagements", "sa", "123", 3, "Test Connection", "name");

            Assert.AreEqual(conn3.ToXml(false).ToString(SaveOptions.None), conn3.ToString());
            Assert.AreEqual(conn3.ToXml(true).ToString(SaveOptions.None), conn3.ToString(true));
            Assert.AreEqual(
                Connection.Parse(conn3.ToXml(true)).ConnectionString,
                conn3.ConnectionString);

            var conn3_2 = new Connection()
            {
                ConnectionString = conn3.ConnectionString
            };

            Assert.AreEqual(conn3.ConnectionString, conn3_2.ConnectionString);
        }


        [TestMethod, TestCategory("Connection.cs")]
        public void ConnectionExplicitConverting()
        {
            var conn = new Connection("UM", ".", "UsersManagements");

            string connStr = (string)conn;
            XElement connXml = (XElement)conn;

            Assert.AreEqual(connStr, conn.ConnectionString);
            Assert.AreEqual(connXml.ToString(SaveOptions.None), conn.ToXml().ToString(SaveOptions.None));
        }

        [TestMethod, TestCategory("Connection.cs")]
        public void ConnectionImplicitConverting()
        {
            var conn = new Connection("UM", ".", "UsersManagements");

            var connStr = (Connection)conn.ConnectionString;
            var connXml = (Connection)conn.ToXml();

            Assert.AreEqual(connStr.ConnectionString, conn.ConnectionString);
            Assert.AreEqual(connXml.ConnectionString, conn.ConnectionString);
        }

        [TestMethod, TestCategory("Connection.cs")]
        public void ConnectionCloneConverting()
        {
            var conn = new Connection("UM", ".", "UsersManagements");

            var connObj = conn.Clone();
            var connObjThis = new Connection(conn);

            TestTools.ExpectException<ArgumentNullException>(() => Connection.Clone(conn, null)); // throw ArgumentNullException
            TestTools.ExpectException<ArgumentNullException>(() => Connection.Clone(null, connObjThis));  // throw ArgumentNullException

            Connection.Clone(conn, connObjThis);

            Assert.AreEqual(conn.ConnectionString, ((Connection)connObj).ConnectionString);
            Assert.AreEqual(connObjThis.ConnectionString, ((Connection)connObj).ConnectionString);
            Assert.AreEqual(connObjThis.ConnectionString, conn.ConnectionString);
        }

        #endregion


        #region Test ConnectionCollection.cs

        [TestMethod, TestCategory("ConnectionCollection.cs")]
        public void CreateConnectionCollection()
        {
            var items = new ConnectionCollection { _conn, _connHost, _connFalse, _connJustTrueServer };

            Assert.AreEqual(2, items.Count); // because 2connection have duplicate name and update when added to list!

            items.Clear();

            Assert.IsTrue(items.Count == 0); // All removed from list

            var um = items["UM"];

            Assert.IsNull(um); // UM is not exist then return NULL

            items.Add(_conn);

            um = items["UM"];

            Assert.IsNotNull(um);

            Assert.IsTrue(items.Contains("UM")); // UM is added

            Assert.IsFalse(items.Contains("IM")); // IM is not exist

            Assert.IsTrue(items.Contains(_conn)); // UM is added
            Assert.IsFalse(items.Contains(_connJustTrueServer)); // Test is not exist

            var aryConn = new Connection[1];
            items.CopyTo(aryConn, 0);

            Assert.IsNotNull(aryConn[0]);

            Assert.AreEqual(aryConn[0].Name, "UM");


            items.Add(_conn);
            items.Add(_connJustTrueServer);

            var umC = new ConnectionManager(items.Find("UM"));

            Assert.IsNotNull(umC);
            umC.Open();
            Assert.IsTrue(umC.State == ConnectionState.Open);

            items.Remove("Test1");
            items["UM"] = _conn;

            foreach (var item in items)
            {
                Assert.IsNotNull(item);
            }

            items.Dispose();
            Assert.IsTrue(items.Count == 0); // All removed from list
        }

        [TestMethod, TestCategory("ConnectionCollection.cs")]
        public void TestConnectionCollection()
        {
            var items = new ConnectionCollection { _conn, _connHost, _connFalse, _connJustTrueServer };

            items["Test"] = _conn;

            Assert.AreEqual(items["Test"].ConnectionString, _conn.ConnectionString);

            foreach (var cm in items.Select(x => new ConnectionManager(x)))
            {
                TestTools.ExceptException<SqlException>(() =>
                {
                    cm.Open();
                    Assert.IsTrue(cm.State == ConnectionState.Open);
                });
            }
        }

        #endregion


        #region Test ConnectionManager.cs

        [TestMethod, TestCategory("ConnectionManager.cs")]
        public void CreateConnectionManager()
        {
            var cm = ConnectionManager.Add(_conn, "um");

            Assert.IsInstanceOfType(ConnectionManager.Find("UM"), typeof(ConnectionManager)); // check type
            Assert.AreEqual(ConnectionManager.Find("UM").ConnectionString, _conn.ConnectionString); // check content of object
            //
            // Open Connection then close that by disposing
            var c1 = ConnectionManager.Find("UM");
            c1.Open();
            Assert.IsTrue(c1.State == ConnectionState.Open);
            c1.Dispose();
            Assert.IsTrue(c1.SqlConn == null);

            ConnectionManager.Find("UM").Open();
            Assert.IsTrue(ConnectionManager.Find("UM").State == ConnectionState.Closed); // because every time fetch new connection

            //
            // Test Duplicate Creating
            cm = ConnectionManager.Add(_connHost, "um");
            cm.Open();
            ConnectionManager.Add(_conn, "um");
            Assert.IsTrue(cm.State == ConnectionState.Open);

            ConnectionManager.Edit(_connHost, cm.Connection.Name);

            Assert.AreEqual(ConnectionManager.Find(cm.Connection.Name).ConnectionString, _connHost.ConnectionString);

            TestTools.ExceptException<ArgumentNullException>(() => ConnectionManager.Edit(null, cm.Connection.Name)); // throw ArgumentNullException
        }

        [TestMethod, TestCategory("ConnectionManager.cs")]
        public void CheckConnectionOpenConfilict()
        {
            var conn2 = ConnectionManager.Add(_conn, "um");
            conn2.Open();

            TestTools.ExceptException<InvalidOperationException>(() =>
                Assert.AreEqual(ConnectionManager.Find(conn2.Connection.Name).ServerVersion, new ConnectionManager(_conn).ServerVersion));

            Assert.AreEqual(conn2.State, ConnectionState.Open);

            Assert.AreEqual(ConnectionManager.Find("UM").State, ConnectionState.Closed); // when call .State then create new SqlConnection , so that is closed always.
            ConnectionManager.Find("UM").Open(); // Create New SqlConnection

            conn2.Close();
            Assert.AreEqual(ConnectionManager.Find("UM").State, ConnectionState.Closed);
            Assert.AreEqual(conn2.State, ConnectionState.Closed);
        }

        [TestMethod, TestCategory("ConnectionManager.cs")]
        public void TestMultipleConnectionOpenClose()
        {
            var cm = ConnectionManager.Add(_conn, "um");


            //cm.Open();
            var counter = 0;
            var n = 10000;

            for (int i = 0; i < n; i++)
            {
                //counter += i;
                Interlocked.Add(ref counter, i);

                cm.Open();
                cm.Close();
            }

            var sum = n * (n - 1) / 2;
            Assert.AreEqual(counter, sum, string.Format("Counter:{0} , Sum:{1}", counter, sum));
            Assert.IsTrue(cm.State == ConnectionState.Closed);
        }


        [TestMethod, TestCategory("ConnectionManager.cs")]
        public async Task TestMultipleConnectionOpenAsync()
        {
            var cm = ConnectionManager.Add(_conn, "um");
            Assert.IsTrue(cm.State == ConnectionState.Closed);

            var counter = 0;
            var n = 100000;

            for (int i = 0; i < n; i++)
            {
                //counter += i;
                Interlocked.Add(ref counter, i);

                await cm.OpenAsync();
                cm.Close();
            }


            var sum = n * (n - 1) / 2;
            Assert.IsTrue(cm.State == ConnectionState.Closed);
            Assert.AreEqual(counter, sum, string.Format("Counter:{0} , Sum:{1}", counter, sum));
        }


        [TestMethod, TestCategory("ConnectionManager.cs")]
        public void TestMultipleConnectionOpenAsyncByCancellationToken()
        {
            var cm = ConnectionManager.Add(_conn, "um");
            Assert.IsTrue(cm.State == ConnectionState.Closed);

            var counter = 0;
            var n = 100000;
            var ct = new CancellationToken();

            Parallel.For(0, n, async i =>
            {
                //counter += i;
                Interlocked.Add(ref counter, i);

                await cm.OpenAsync(ct);
                Assert.IsTrue(cm.State == ConnectionState.Open);
                cm.Close();
                Assert.IsTrue(cm.State == ConnectionState.Closed);
            });


            var sum = n * (n - 1) / 2;
            Assert.AreEqual(counter, sum, string.Format("Counter:{0} , Sum:{1}", counter, sum));
        }


        [TestMethod, TestCategory("ConnectionManager.cs")]
        public void TestRemoveConnection()
        {
            var c = ConnectionManager.Add(_connHost, "um");
            var cm = ConnectionManager.Add(_conn, "um");
            cm.Open();
            Assert.IsTrue(cm.State == ConnectionState.Open);

            ConnectionManager.Remove(c);
            ConnectionManager.Remove("UM"); // just delete connection from list, so far connection is alive

            Assert.IsTrue(c.SqlConn != null);
            Assert.IsTrue(cm.SqlConn != null);
            Assert.IsTrue(cm.State == ConnectionState.Open);
            Assert.IsTrue(c.State == ConnectionState.Closed);

            cm.Dispose();
            c.Dispose();

            Assert.IsTrue(c.SqlConn == null);
            Assert.IsTrue(cm.SqlConn == null);
        }


        [TestMethod, TestCategory("ConnectionManager.cs")]
        public void TestConnectionCheckers()
        {
            //
            // Test Full Trust Connection
            //
            var cm = ConnectionManager.Add(_conn, "um"); // Test by localhost server name

            Assert.IsFalse(cm.IsReady); // IsReady is false in first time

            Assert.IsTrue(cm.IsServerOnline()); // Server is True but isReady set just false time
            Assert.IsFalse(cm.IsReady);

            cm = ConnectionManager.Add(_connHost, "um"); // Test by Host server name

            Assert.IsFalse(cm.IsReady); // IsReady is false in first time

            Assert.IsTrue(cm.CheckDbConnection()); // Check All [Server + DB]

            Assert.IsTrue(cm.IsReady); // IsReady True , because server and database is correct
            //
            // Test Just True Server by inCorrect database name
            //
            cm = ConnectionManager.Add(_connJustTrueServer, "test"); // Test by true server name

            Assert.IsFalse(cm.IsReady); // IsReady is false in first time

            Assert.IsTrue(cm.IsServerOnline()); // Server is True but isReady set just false time
            Assert.IsFalse(cm.IsReady);

            Assert.IsFalse(cm.CheckDbConnection()); // Check All [Server + DB]

            Assert.IsFalse(cm.IsReady); // IsReady False , because server is true but database is not correct
            //
            // Test by inCorrect connection
            //
            cm = ConnectionManager.Add(_connFalse, "test"); // Test by incorrect server name

            Assert.IsFalse(cm.IsReady);

            Assert.IsFalse(cm.IsServerOnline()); // Server is not correct

            Assert.IsFalse(cm.IsReady); // IsReady is false = server is false

            Assert.IsFalse(cm.CheckDbConnection()); // check all

            Assert.IsFalse(cm.IsReady); // server and DB are false
        }


        [TestMethod, TestCategory("ConnectionManager.cs")]
        public async Task TestConnectionCheckersAsync()
        {
            //
            // Test Full Trust Connection
            //
            var cm = ConnectionManager.Add(_conn, "um"); // Test by localhost server name

            Assert.IsFalse(cm.IsReady); // IsReady is false in first time

            Assert.IsTrue(await cm.IsServerOnlineAsync()); // Server is True but isReady set just false time
            Assert.IsFalse(cm.IsReady);

            cm = ConnectionManager.Add(_connHost, "um"); // Test by Host server name

            Assert.IsFalse(cm.IsReady); // IsReady is false in first time

            Assert.IsTrue(await cm.CheckDbConnectionAsync()); // Check All [Server + DB]

            Assert.IsTrue(cm.IsReady); // IsReady True , because server and database is correct
            //
            // Test Just True Server by inCorrect database name
            //
            cm = ConnectionManager.Add(_connJustTrueServer, "test"); // Test by true server name

            Assert.IsFalse(cm.IsReady); // IsReady is false in first time

            Assert.IsTrue(await cm.IsServerOnlineAsync()); // Server is True but isReady set just false time
            Assert.IsFalse(cm.IsReady);

            Assert.IsFalse(await cm.CheckDbConnectionAsync()); // Check All [Server + DB]

            Assert.IsFalse(cm.IsReady); // IsReady False , because server is true but database is not correct
            //
            // Test by inCorrect connection
            //
            cm = ConnectionManager.Add(_connFalse, "test"); // Test by incorrect server name

            Assert.IsFalse(cm.IsReady);

            Assert.IsFalse(await cm.IsServerOnlineAsync()); // Server is not correct

            Assert.IsFalse(cm.IsReady); // IsReady is false = server is false

            Assert.IsFalse(await cm.CheckDbConnectionAsync()); // check all

            Assert.IsFalse(cm.IsReady); // server and DB are false
        }


        [TestMethod, TestCategory("ConnectionManager.cs")]
        public void TestAddFind()
        {
            //
            // Test Add and Find
            //
            var c1 = ConnectionManager.Add(_conn, "um");
            var c2 = ConnectionManager.Find("um");
            Assert.AreEqual(c1.ConnectionString, c2.ConnectionString);
            //
            // Test Search Connection by Ignore Case Name
            //
            Assert.IsNotNull(ConnectionManager.Find("UM"));
            Assert.IsNotNull(ConnectionManager.Find("um"));
            Assert.IsNotNull(ConnectionManager.Find("uM"));
            Assert.IsNotNull(ConnectionManager.Find("Um"));
        }

        [TestMethod, TestCategory("ConnectionManager.cs")]
        public void TestXml()
        {
            //
            // Convert connections to XML
            //
            ConnectionManager.Add(_conn, "um");
            ConnectionManager.Add(_connJustTrueServer, "test");
            var xmlConnections = ConnectionManager.SaveToXml();
            var xmlConnectionsEncrypted = ConnectionManager.SaveToXml(true);

            ConnectionManager.ClearAll(); ;
            Assert.IsTrue(ConnectionManager.Count == 0);

            ConnectionManager.LoadFromXml(xmlConnections);
            Assert.IsTrue(ConnectionManager.Count == 2);

            ConnectionManager.ClearAll();
            Assert.IsTrue(ConnectionManager.Count == 0);

            ConnectionManager.LoadFromXml(xmlConnectionsEncrypted);
            Assert.IsTrue(ConnectionManager.Count == 2);
        }

        #endregion


        #region Test All
        [TestMethod, TestCategory("All")]
        public void TestIsReady()
        {
            var i = ConnectionManager.Add(_conn, "um");

            Assert.IsFalse(i.IsReady);
            Assert.IsFalse(_conn.IsReady);

            if (i.CheckDbConnection())
            {
                for (int j = 0; j < 100; j++)
                {
                    Assert.IsTrue(i.IsReady);
                    Assert.IsTrue(ConnectionManager.Find("um").IsReady);
                    Assert.IsTrue(_conn.IsReady);

                    var ii = ConnectionManager.Find("UM");
                    Assert.IsTrue(ii.IsReady);

                    Assert.IsTrue(ii.State == ConnectionState.Closed);
                    ii.Open();
                    Assert.IsTrue(ii.State == ConnectionState.Open);
                }
            }
            else
                Assert.Fail("CheckDbConnection is false!!");

            //
            // Check not ready
            //
            Assert.IsTrue(ConnectionManager.Find("UM").IsReady);
            ConnectionManager.Edit(_connFalse, "uM");
            Assert.IsFalse(ConnectionManager.Find("UM").IsReady);
            ConnectionManager.Edit(_connJustTrueServer, "uM");
            Assert.IsFalse(ConnectionManager.Find("UM").IsReady);
            Assert.IsFalse(ConnectionManager.Find("UM").CheckDbConnection());
            ConnectionManager.Edit(_conn, "Um");
            Assert.IsTrue(ConnectionManager.Find("UM").CheckDbConnection()); // when add first time this is work correctly but now not work
            Assert.IsTrue(ConnectionManager.Find("UM").IsReady);
        }
        #endregion
    }
}
