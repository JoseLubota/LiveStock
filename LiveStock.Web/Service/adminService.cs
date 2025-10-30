using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LiveStock.Web.Service
{
    public class adminService
    {
        //----------------------------------------------------------
        public readonly string conString;


        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Initializes admin service with DB connection string"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public adminService(IConfiguration configuration)
        {
            conString = configuration.GetConnectionString("AzureConString");
        }
        //----------------------------------------------------------
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Validates admin credentials in DB and returns user ID if found"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public int checkUser(string email, string password)
        {
            int userID = -1;

            using(SqlConnection con = new SqlConnection(conString))
            {
                string sql = "SELECT ID FROM Admin WHERE email = @Email AND password = @Password";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);
                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        userID = Convert.ToInt32(result);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return userID;
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
