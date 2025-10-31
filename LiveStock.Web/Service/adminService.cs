using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using LiveStock.Core.Services;

namespace LiveStock.Web.Service
{
    public class adminService
    {
        //----------------------------------------------------------
        public readonly string conString;
        private readonly ILogger<adminService> _logger;


        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Initializes admin service with DB connection string"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public adminService(IConfiguration configuration, ILogger<adminService> logger)
        {
            conString = configuration.GetConnectionString("AzureConString");
            _logger = logger;
        }
        //----------------------------------------------------------
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Validates admin credentials in DB and returns user ID if found"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public int checkUser(string email, string password)
        {
            try
            {
                int userID = -1;

                using(SqlConnection con = new SqlConnection(conString))
                {
                    // Fetch user by email and verify password using salted hash
                    string sql = "SELECT ID, password FROM Admin WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        con.Open();
                        using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string storedPassword = reader.GetString(1);

                                bool verified = PasswordHashingService.VerifyPassword(password, storedPassword);

                                if (verified)
                                {
                                    userID = id;
                                }
                                else
                                {
                                    // Backward-compat: if stored is plaintext and matches, migrate to hashed
                                    if (storedPassword == password)
                                    {
                                        try
                                        {
                                            string newHash = PasswordHashingService.HashPassword(password);
                                            reader.Close();
                                            string updateSql = "UPDATE Admin SET password=@Password WHERE ID=@Id";
                                            using (var updateCmd = new SqlCommand(updateSql, con))
                                            {
                                                updateCmd.Parameters.AddWithValue("@Password", newHash);
                                                updateCmd.Parameters.AddWithValue("@Id", id);
                                                updateCmd.ExecuteNonQuery();
                                            }
                                            userID = id;
                                        }
                                        catch (Exception updEx)
                                        {
                                            _logger.LogError(updEx, "Failed to migrate plaintext password to hash for {Email}", email);
                                            // Still allow login to not lock out user
                                            userID = id;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return userID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "checkUser failed for email {Email}", email);
                return -1;
            }
        }
        //------------------------------------------------------------------------------------------------------------
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Returns configured database connection string"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public string GetConnection()
        {
            return conString;
        }

        //------------------------------------------------------------------------------------------------------------
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Creates a new admin user in the Admin table and returns the new ID"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public int CreateAdmin(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogError("CreateAdmin called with missing email or password");
                    return -1;
                }

                using (SqlConnection con = new SqlConnection(conString))
                {
                    // Assumes Admin table has columns: ID (identity), email, password
                    string sql = "INSERT INTO Admin (email, password) OUTPUT INSERTED.ID VALUES (@Email, @Password)";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        string hashed = PasswordHashingService.HashPassword(password);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", hashed);

                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            int newId = Convert.ToInt32(result);
                            _logger.LogInformation("Admin created with ID={AdminId} for {Email}", newId, email);
                            return newId;
                        }
                    }
                }

                _logger.LogError("CreateAdmin failed to insert admin for {Email}", email);
                return -1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAdmin failed for email {Email}", email);
                return -1;
            }
        }

        //------------------------------------------------------------------------------------------------------------
        // Admin CRUD helpers
        //------------------------------------------------------------------------------------------------------------
        public class AdminUser
        {
            public int Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? Password { get; set; }
        }

        public List<AdminUser> ListAdmins()
        {
            var admins = new List<AdminUser>();
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    string sql = "SELECT ID, email FROM Admin ORDER BY ID";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        con.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                admins.Add(new AdminUser
                                {
                                    Id = reader.GetInt32(0),
                                    Email = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ListAdmins failed");
            }
            return admins;
        }

        public AdminUser? GetAdmin(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    string sql = "SELECT ID, email FROM Admin WHERE ID=@Id";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        con.Open();
                        using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                return new AdminUser
                                {
                                    Id = reader.GetInt32(0),
                                    Email = reader.GetString(1)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAdmin failed for {Id}", id);
            }
            return null;
        }

        public bool UpdateAdmin(int id, string email, string? password)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    string sql;
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        sql = "UPDATE Admin SET email=@Email, password=@Password WHERE ID=@Id";
                    }
                    else
                    {
                        sql = "UPDATE Admin SET email=@Email WHERE ID=@Id";
                    }
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            string hashed = PasswordHashingService.HashPassword(password);
                            cmd.Parameters.AddWithValue("@Password", hashed);
                        }
                        cmd.Parameters.AddWithValue("@Id", id);
                        con.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAdmin failed for {Id}", id);
                return false;
            }
        }

        public bool DeleteAdmin(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    string sql = "DELETE FROM Admin WHERE ID=@Id";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        con.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAdmin failed for {Id}", id);
                return false;
            }
        }

    }
}
