using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LiveStock.Web.Service
{
    public class adminService
    {
        //----------------------------------------------------------
        public readonly string conString;


        public adminService(IConfiguration configuration)
        {
            conString = configuration.GetConnectionString("AzureConString");
        }
        //----------------------------------------------------------
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
        public string GetConnection()
        {
            return conString;
        }

    }
}
