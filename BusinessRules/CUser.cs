﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Configuration;

namespace BusinessRules
{
    public class CUser
    {
        SqlConnection objConn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["strConn"].ConnectionString);

        /*c# code for hashing and salting - credit Dino Esposito at http://devproconnections.com/aspnet/aspnet-web-security-protect-user-passwords-hashing-and-salt?page=2 */

        public string hashPassword(string password, string salt)
        {
            var combinedPassword = String.Concat(password, salt);
            var sha256 = new SHA256Managed();
            var bytes = UTF8Encoding.UTF8.GetBytes(combinedPassword);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public string getRandomSalt(Int32 size = 12)
        {
            var random = new RNGCryptoServiceProvider();
            var salt = new Byte[size];
            random.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        public bool validatePassword(string enteredPassword, string storedHash, string storedSalt)
        {
            // Consider this function as an internal function where parameters like
            // storedHash and storedSalt are read from the database and then passed.

            var hash = hashPassword(enteredPassword, storedSalt);
            return String.Equals(storedHash, hash);
        }
        // register a new user in the db
        public void register(string username, string password, string role)
        {
            //random salt for each user
            string salt = getRandomSalt(12);
            string hashedPassword = hashPassword(password, salt);

            //ado code to interact w/db
            objConn.Open();
           
            SqlCommand objCmd = new SqlCommand("spRegister", objConn);
            objCmd.CommandType = System.Data.CommandType.StoredProcedure;
            objCmd.Parameters.AddWithValue("@Username", username);
            objCmd.Parameters.AddWithValue("@Password", hashedPassword);
            objCmd.Parameters.AddWithValue("@SaltString", salt);
            objCmd.Parameters.AddWithValue("@Role", role);
            objCmd.ExecuteNonQuery();

            objCmd.Dispose();
            objConn.Close();
        }
        // log in method, takes username and pass from login page
        public string login(string username, string password)
        {
            objConn.Open();
            //query
            string strSQL = "SELECT * FROM Users WHERE Username = '" + username + "'";
            SqlCommand objCmd = new SqlCommand(strSQL, objConn);

            SqlDataReader objRdr = objCmd.ExecuteReader();
            string role = "";

            //fill the datareader
           // objRdr = objCmd.ExecuteReader();

            while (objRdr.Read())
            {
                if (validatePassword(password, objRdr.GetString(2), objRdr.GetString(3))) {
                    role = objRdr.GetString(4);
                }
            }

            //clean up
            objCmd.Dispose();
            objConn.Close();
            return role;

        }
        //find a row in the db with the same name
        public static int getIDByName(string username)
        {
          int userID = -1;

          SqlConnection objConn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["strConn"].ConnectionString);
          objConn.Open();

          string getIDSQL = "SELECT UserID FROM users WHERE Username='" + username + "'";

          SqlCommand objCmd = new SqlCommand(getIDSQL, objConn);
          SqlDataReader objRdr = objCmd.ExecuteReader();

          while (objRdr.Read())
          {
            userID = objRdr.GetInt32(0);
          }

          objCmd.Dispose();
          objConn.Close();
          return userID;
        }
        //find a row in the database table with the same id as one specified. Needs userID
        public static string getNameByID(int userID)
        {
          string userName = "";

          SqlConnection objConn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["strConn"].ConnectionString);
          objConn.Open();
          //query
          string getIDSQL = "SELECT Username FROM users WHERE UserID='" + userID + "'";

          SqlCommand objCmd = new SqlCommand(getIDSQL, objConn);
          SqlDataReader objRdr = objCmd.ExecuteReader();

          while (objRdr.Read())
          {
            userName = objRdr.GetString(0);
          }

          objCmd.Dispose();
          objConn.Close();
          return userName;
        }
        //get a list of all users in the users table
        public SqlDataReader getUsers()
        {

          objConn.Open();
          //query
          string strSQL = "SELECT UserID, Username, Role FROM Users ORDER BY Username";
          SqlCommand objCmd = new SqlCommand(strSQL, objConn);

          SqlDataReader objRdr = objCmd.ExecuteReader();
          return objRdr;

        }
        //delete specified user form table. needs UserID
        public void deleteUser(int UserID)
        {
          objConn.Open();
          //query
          string strSQL = "DELETE FROM Users WHERE UserID = " + UserID.ToString();
     
          SqlCommand objCmd = new SqlCommand(strSQL, objConn);
          objCmd.ExecuteNonQuery();

          objCmd.Dispose();
          objConn.Close();
        }
    }
}
