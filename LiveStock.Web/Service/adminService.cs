using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
                    string sql = "SELECT ID FROM Admin WHERE email = @Email AND password = @Password";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        userID = Convert.ToInt32(result);
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

    }
}
