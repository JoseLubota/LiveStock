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
                string sql = @"INSERT INTO Sheep (breed, birthDate, camp, gender, price, photoID)
                              VALUES(@breed, @birthDate, @camp, @gender, @price, @photoID)";

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
                            IsActive = reader.GetBoolean(reader.GetOrdinal("isActive")),
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
        public void DeleteSheep(int sheepID)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "DELETE FROM Sheep WHERE sheepID = @sheepID";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@sheepID", sheepID);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if(rowsAffected == 0)
                {
                    Console.WriteLine($"No sheep found");
                }
                else
                {
                    Console.WriteLine($"Sheep with ID {sheepID} deleted successfully.");
                }
            }
        }

        public List<Sheep> getSheepByID(int sheepID)
        {
            List<Sheep> sheepList = new List<Sheep>();

            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "SELECT * FROM Sheep WHERE sheepID = @sheepID";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@sheepID", sheepID);

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
                            IsActive = reader.GetBoolean(reader.GetOrdinal("isActive")),
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

        public List<Sheep> FillVoidSheppFields(List<Sheep> currentShepp, List<Sheep> newSheppDetails)
        {
            List<Sheep> result = [];

            foreach(var current in currentShepp)
            {
                var updated = newSheppDetails.FirstOrDefault(s => s.SheepID == current.SheepID);

                if(updated != null)
                {
                    // Fill in missing fields
                    current.Breed = string.IsNullOrEmpty(updated.Breed) ? current.Breed : updated.Breed;
                    current.Gender = string.IsNullOrEmpty(updated.Gender) ? current.Gender : updated.Gender;
                    current.CampId = updated.CampId == 0 ? current.CampId : updated.CampId;
                    current.Price = updated.Price == 0 ? current.Price : updated.Price;
                    current.BirthDate = updated.BirthDate == default ? current.BirthDate : updated.BirthDate;
                }
                result.Add(current);
                    
            }

            return result;
        }

        public void UpdateSheep(Sheep updateSheep)
        {
            using(SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = " UPDATE Sheep SET breed = @breed, camp = @camp, gender = @gender,price = @price, birthDate = @birthDate WHERE sheepID = @sheepID";

                using(SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@sheepID", updateSheep.SheepID);
                    cmd.Parameters.AddWithValue("@breed", updateSheep.Breed);
                    cmd.Parameters.AddWithValue("@camp", updateSheep.CampId);
                    cmd.Parameters.AddWithValue("@price", updateSheep.Price);
                    cmd.Parameters.AddWithValue("@birthDate", updateSheep.BirthDate);
                    cmd.Parameters.AddWithValue("@gender", updateSheep.Gender);
                    
                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if(rows == 0)
                    {
                        Console.WriteLine($"No sheep dound with the ID {updateSheep.SheepID}");
                    }
                    else
                    {
                        Console.WriteLine($"Upsated {updateSheep.SheepID}");
                    }
                }

            }
        }
    }
}