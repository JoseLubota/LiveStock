using iTextSharp.text;
using iTextSharp.text.pdf;
using LiveStock.Core.Models;
using Microsoft.Data.SqlClient;

namespace LiveStock.Web.Service
{
    public class cowService
    {
        private readonly string _conString;

        public cowService(IConfiguration configuration)
        {
            _conString = configuration.GetConnectionString("AzureConString");
        }

        public void AddCow(string earTag, string breed, DateOnly birdthDate, int camp, string gender, decimal price, string photoURL, DateTime createdAt, bool IsPregnant, DateTime? expectedCalvingDate)
        {
            if (!IsPregnant)
                IsPregnant = false;

            using (SqlConnection con = new SqlConnection(_conString))
            {
                string sql = @"INSERT INTO Cows (EarTag, Breed, BirthDate, CampId, Gender, Price, CreatedAt, IsActive, PhotoUrl, IsPregnant, ExpectedCalvingDate)
                              VALUES(@earTag, @breed, @birthDate, @campId, @gender, @price, @createdAt, 1, @photoURL, @ispregnant, @expectedCalvingDate)";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@earTag", earTag);
                cmd.Parameters.AddWithValue("@breed", breed);
                cmd.Parameters.AddWithValue("@birthDate", birdthDate);
                cmd.Parameters.AddWithValue("@campId", camp);
                cmd.Parameters.AddWithValue("@gender", gender);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@createdAt", createdAt);
                cmd.Parameters.AddWithValue("@photoURL", photoURL);
                cmd.Parameters.AddWithValue("@ispregnant", IsPregnant);
                cmd.Parameters.AddWithValue("@expectedCalvingDate", expectedCalvingDate.HasValue ? (object)expectedCalvingDate.Value : DBNull.Value);
                //cmd.Parameters.AddWithValue("@photoID", (object?)photoID ?? DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public Queue<Cow> GetAllCow()
        {
            Queue<Cow> cowQueue = new Queue<Cow>();

            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "SELECT * FROM Cows";
                SqlCommand cmd = new SqlCommand(sql, con);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cow = new Cow
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            EarTag = reader.GetString(reader.GetOrdinal("EarTag")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            CampId = reader.GetInt32(reader.GetOrdinal("CampId")),
                            Gender = reader.GetString(reader.GetOrdinal("Gender")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                            BirthDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("BirthDate"))),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            PhotoUrl = reader.IsDBNull(reader.GetOrdinal("PhotoUrl"))
                                       ? (string?)null : reader.GetString(reader.GetOrdinal("PhotoUrl")),
                            IsPregnant = reader.GetBoolean(reader.GetOrdinal("IsPregnant")),
                            ExpectedCalvingDate = reader.IsDBNull(reader.GetOrdinal("ExpectedCalvingDate"))
                                       ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ExpectedCalvingDate")),


                            /*
                             * reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                        ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                             */
                        };
                        cowQueue.Enqueue(cow);
                    }
                }
            }

            return cowQueue;
        }
        public void DeleteCow(int sheepID)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "DELETE FROM Sheep WHERE SheepID = @earTag";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@earTag", sheepID);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    Console.WriteLine($"No sheep found");
                }
                else
                {
                    Console.WriteLine($"Sheep with ID {sheepID} deleted successfully.");
                }
            }
        }

        public Queue<Sheep> getCowByID(int sheepID)
        {
            Queue<Sheep> sheepList = new Queue<Sheep>();

            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "SELECT * FROM Sheep WHERE SheepID = @earTag";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@earTag", sheepID);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sheep = new Sheep
                        {
                            SheepID = reader.GetInt32(reader.GetOrdinal("SheepID")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            CampId = reader.GetInt32(reader.GetOrdinal("CampId")),
                            Gender = reader.GetString(reader.GetOrdinal("Gender")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                            BirthDate = DateOnly.FromDateTime(
                                reader.GetDateTime(reader.GetOrdinal("BirthDate"))),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                        ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                            PhotoUrl = reader.IsDBNull(reader.GetOrdinal("PhotoUrl"))
                                       ? (string?)null : reader.GetString(reader.GetOrdinal("PhotoUrl"))

                        };
                        sheepList.Enqueue(sheep);
                    }
                }
            }

            return sheepList;
        }

        public Queue<Sheep> FillVoidCowFields(Queue<Sheep> currentShepp, Queue<Sheep> newSheppDetails)
        {
            Queue<Sheep> result = [];

            foreach (var current in currentShepp)
            {
                var updated = newSheppDetails.FirstOrDefault(s => s.SheepID == current.SheepID);

                if (updated != null)
                {
                    // Fill in missing fields
                    current.Breed = string.IsNullOrEmpty(updated.Breed) ? current.Breed : updated.Breed;
                    current.Gender = string.IsNullOrEmpty(updated.Gender) ? current.Gender : updated.Gender;
                    current.CampId = updated.CampId == 0 ? current.CampId : updated.CampId;
                    current.Price = updated.Price == 0 ? current.Price : updated.Price;
                    current.BirthDate = updated.BirthDate == default ? current.BirthDate : updated.BirthDate;
                    current.UpdatedAt = updated.UpdatedAt == default ? current.UpdatedAt : updated.UpdatedAt;
                    current.PhotoUrl = string.IsNullOrEmpty(updated.PhotoUrl) ? current.PhotoUrl : updated.PhotoUrl;
                }
                result.Enqueue(current);

            }

            return result;
        }

        public void UpdateCow(Sheep updateSheep)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = " UPDATE Sheep SET Breed = @breed, CampId = @camp, Gender = @gender,Price = @price, BirthDate = @birthDate, UpdatedAt = @updatedAt, PhotoUrl = @photoUrl WHERE SheepID = @earTag";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@earTag", updateSheep.SheepID);
                    cmd.Parameters.AddWithValue("@breed", updateSheep.Breed);
                    cmd.Parameters.AddWithValue("@camp", updateSheep.CampId);
                    cmd.Parameters.AddWithValue("@price", updateSheep.Price);
                    cmd.Parameters.AddWithValue("@birthDate", updateSheep.BirthDate);
                    cmd.Parameters.AddWithValue("@gender", updateSheep.Gender);
                    cmd.Parameters.AddWithValue("@updatedAt", updateSheep.UpdatedAt);
                    cmd.Parameters.AddWithValue("@photoUrl", updateSheep.PhotoUrl);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        Console.WriteLine($"No sheep dound with the ID {updateSheep.SheepID}");
                    }
                    else
                    {
                        Console.WriteLine($"Updated {updateSheep.SheepID}");
                    }
                }

            }
        }

        public void CowBulkActions(string action, string reason, HashSet<int> sheepIDs)
        {
            switch (action)
            {
                case "markSold":
                    foreach (int id in sheepIDs)
                    {
                        DeleteCow(id);
                    }
                    break;

                case "move":
                    if (int.TryParse(reason, out int newCampId))
                    {
                        foreach (int id in sheepIDs)
                        {
                            MoveCowToCamp(id, newCampId);
                        }

                    }
                    break;

                case "markInactive":
                    foreach (int id in sheepIDs)
                    {
                        MarkCowAsInactive(id);
                    }
                    break;

                case "markAactive":
                    foreach (int id in sheepIDs)
                    {
                        MarkCowAsActive(id);
                    }
                    break;
            }
        }
        public void MoveCowToCamp(int id, int campID)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "UPDATE Sheep SET CampId = @campID WHERE SheepID = @earTag";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@earTag", id);
                    cmd.Parameters.AddWithValue("@campID", campID);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Move Sheep to camp {rows} affected");

                }
            }
        }

        public void MarkCowAsInactive(int id)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "UPDATE Sheep SET IsActive = 0 WHERE SheepID = @earTag";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@earTag", id);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Mark Sheep as inactive {rows} affected");
                }
            }
        }

        public void MarkCowAsActive(int id)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "UPDATE Sheep SET IsActive = 1 WHERE Id = @id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Mark Sheep as active {rows} affected");
                }
            }
        }
        public byte[] ExportSheep()
        {
            var sheepList = GetAllCow();

            var csvBuilder = new System.Text.StringBuilder();

            csvBuilder.AppendLine("Ear Tag,Breed,CampId,Gender,BirthDate,Price,IsActive");
            foreach (var item in sheepList)
            {
                csvBuilder.AppendLine($"{item.EarTag},{item.Breed},{item.CampId},{item.Gender},{item.BirthDate},{item.Price},{item.IsActive}");

            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return bytes;

        }
        public byte[] GenerateCowReport()
        {

            var sheepList = GetAllCow();
            using (var mstream = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(doc, mstream);
                doc.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.BLACK);
                var subTitleFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);

                Paragraph title = new Paragraph("Sheep Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);

                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph($"Date Generated: {DateTime.Now:MMMM-dd, yyyy HH:mm}", subTitleFont));
                doc.Add(new Paragraph($"Total Sheep: {sheepList.Count}", subTitleFont));
                doc.Add(new Paragraph("\n\n"));

                // Table Setup

                PdfPTable table = new PdfPTable(7);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1.2f, 2f, 1.5f, 1.2f, 2f, 1.5f, 1.2f });

                // Headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                string[] headers = { "Ear Tag", "Breed", "Camp ID", "Gender", "Birth Date", "Price", "Active" };

                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(52, 73, 94),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }
                // Data

                var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                foreach (var sheep in sheepList)
                {
                    table.AddCell(new Phrase(sheep.EarTag, dataFont));
                    table.AddCell(new Phrase(sheep.Breed, dataFont));
                    table.AddCell(new Phrase($"Camp {sheep.CampId}", dataFont));
                    table.AddCell(new Phrase(sheep.Gender, dataFont));
                    table.AddCell(new Phrase(sheep.BirthDate.ToString("MMMM-dd-yyyy"), dataFont));
                    table.AddCell(new Phrase($"R {sheep.Price:N2}", dataFont));
                    table.AddCell(new Phrase(sheep.IsActive ? "Yes" : "No", dataFont));
                }
                doc.Add(table);

                // Footer
                doc.Add(new Paragraph("\n\n"));
                Paragraph footer = new Paragraph("", subTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                doc.Add(footer);
                doc.Close();

                return mstream.ToArray();
            }

        }

        public async Task<string> SaveCowPhoto(IFormFile photo)
        {

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/cow");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            return "/uploads/cow/" + fileName;
        }
        public void DeleteCowPhoto(int sheepID)
        {
            var sheepList = getCowByID(sheepID);
            if (sheepList == null || sheepList.Count == 0)
                return;

            if (string.IsNullOrEmpty(sheepList.FirstOrDefault().PhotoUrl))
                return;

            var roothPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(roothPath, sheepList.FirstOrDefault().PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Deleted photo : {filePath}");
            }
        }

    }
}
