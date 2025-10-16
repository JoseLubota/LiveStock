using Microsoft.Data.SqlClient;
using LiveStock.Core.Models;

namespace LiveStock.Web.Service
{
    public class sheepService
    {
        private readonly string _conString;

        public sheepService(IConfiguration configuration)
        {
            _conString = configuration.GetConnectionString("AzureConString");
        }

        public void AddSheep(string breed, DateOnly birdthDate, int camp, string gender, decimal price, int? photoID)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                string sql = @"INSERT INTO Sheep (breed, birthDate, camp, gender, price, status, photoID)
                              VALUES(@breed, @birthDate, @camp, @gender, @price, 'Healty', @photoID)";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@breed", breed);
                cmd.Parameters.AddWithValue("@birthDate",birdthDate);
                cmd.Parameters.AddWithValue("@camp", camp);
                cmd.Parameters.AddWithValue("@gender", gender);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@photoID", (object?)photoID ?? DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public List<Sheep> GetAllSheep()
        {
            List<Sheep> sheepList = new List<Sheep>();

            using(SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "SELECT * FROM Sheep";
                SqlCommand cmd = new SqlCommand(sql, con);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sheep = new Sheep
                        {
                            SheepID = reader.GetInt32(reader.GetOrdinal("sheepID")),
                            Breed = reader.GetString(reader.GetOrdinal("breed")),
                            CampId = reader.GetInt32(reader.GetOrdinal("camp")),
                            Gender = reader.GetString(reader.GetOrdinal("gender")),
                            Price = reader.GetDecimal(reader.GetOrdinal("price")),
                            Status = reader.GetString(reader.GetOrdinal("status")),
                            BirthDate = DateOnly.FromDateTime(
                                reader.GetDateTime(reader.GetOrdinal("birthDate"))),
                            PhotoID = reader.IsDBNull(reader.GetOrdinal("photoID"))
                                    ? null
                                    : reader.GetInt32(reader.GetOrdinal("photoID"))
                        };
                        sheepList.Add(sheep);
                    }
                }
            }

            return sheepList;
        }
    }
}