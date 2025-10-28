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
        public void DeleteCow(int id)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "DELETE FROM Cows WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    Console.WriteLine($"No cow found");
                }
                else
                {
                    Console.WriteLine($"Sheep with ID {id} deleted successfully.");
                }
            }
        }

        public Queue<Cow> getCowByID(int ID)
        {
            Cow cow = GetAllCow().FirstOrDefault(c => c.Id == ID);
            Queue<Cow> cowQueue = [];
            cowQueue.Enqueue(cow);

            return cowQueue;
        }

        public Queue<Cow> FillVoidCowFields(Queue<Cow> currentCow, Queue<Cow> newCowDetails)
        {
            Queue<Cow> result = [];

            foreach (var current in currentCow)
            {
                Console.WriteLine($" New Cow Details has {newCowDetails.Count} entries");
                var updated = newCowDetails.FirstOrDefault(s => s.Id == current.Id);

                if (updated != null)
                {
                    // Fill in missing fields
                    current.Breed = string.IsNullOrEmpty(updated.Breed) ? current.Breed : updated.Breed;
                    current.EarTag = string.IsNullOrEmpty(updated.EarTag) ? current.EarTag : updated.EarTag;
                    current.Gender = string.IsNullOrEmpty(updated.Gender) ? current.Gender : updated.Gender;
                    current.CampId = updated.CampId == 0 ? current.CampId : updated.CampId;
                    current.Price = updated.Price == 0 ? current.Price : updated.Price;
                    current.BirthDate = updated.BirthDate == default ? current.BirthDate : updated.BirthDate;
                    current.UpdatedAt = updated.UpdatedAt == default ? current.UpdatedAt : updated.UpdatedAt;
                    current.PhotoUrl = string.IsNullOrEmpty(updated.PhotoUrl) ? current.PhotoUrl : updated.PhotoUrl;
                    current.IsPregnant = updated.IsPregnant == default ? current.IsPregnant : updated.IsPregnant;
                    current.ExpectedCalvingDate = updated.ExpectedCalvingDate == default ? current.ExpectedCalvingDate : updated.ExpectedCalvingDate;
                }
                result.Enqueue(current);

            }

            return result;
        }

        public void UpdateCow(Cow updateCow)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = " UPDATE Cows SET EarTag = @earTag, Breed = @breed, CampId = @camp, Gender = @gender,Price = @price, BirthDate = @birthDate, UpdatedAt = @updatedAt, PhotoUrl = @photoUrl, IsPregnant = @ispregnant, ExpectedCalvingDate = @expectedCalvingDate WHERE Id = @id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Id", updateCow.Id);
                    cmd.Parameters.AddWithValue("@earTag", updateCow.EarTag);
                    cmd.Parameters.AddWithValue("@breed", updateCow.Breed);
                    cmd.Parameters.AddWithValue("@camp", updateCow.CampId);
                    cmd.Parameters.AddWithValue("@price", updateCow.Price);
                    cmd.Parameters.AddWithValue("@birthDate", updateCow.BirthDate);
                    cmd.Parameters.AddWithValue("@gender", updateCow.Gender);
                    cmd.Parameters.AddWithValue("@updatedAt", updateCow.UpdatedAt);
                    cmd.Parameters.AddWithValue("@photoUrl", updateCow.PhotoUrl);
                    cmd.Parameters.AddWithValue("@ispregnant", updateCow.IsPregnant);
                    cmd.Parameters.AddWithValue("@expectedCalvingDate", updateCow.ExpectedCalvingDate.HasValue ? (object)updateCow.ExpectedCalvingDate.Value : DBNull.Value);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        Console.WriteLine($"No Cow found with the ID {updateCow.Id}");
                    }
                    else
                    {
                        Console.WriteLine($"Updated {updateCow.Id}");
                    }
                }

            }
        }

        public void CowBulkActions(string action, string reason, HashSet<int> cowId)
        {
            switch (action)
            {
                case "markSold":
                    foreach (int id in cowId)
                    {
                        DeleteCow(id);
                    }
                    break;

                case "move":
                    if (int.TryParse(reason, out int newCampId))
                    {
                        foreach (int id in cowId)
                        {
                            MoveCowToCamp(id, newCampId);
                        }

                    }
                    break;

                case "markInactive":
                    foreach (int id in cowId)
                    {
                        MarkCowAsInactive(id);
                    }
                    break;

                case "markAactive":
                    foreach (int id in cowId)
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
                const string sql = "UPDATE Cows SET CampId = @campID WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@campID", campID);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Move Cow to camp {rows} affected");

                }
            }
        }

        public void MarkCowAsInactive(int id)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "UPDATE Cows SET IsActive = 0 WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Mark Cows as inactive {rows} affected");
                }
            }
        }

        public void MarkCowAsActive(int id)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "UPDATE Cows SET IsActive = 1 WHERE Id = @id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Mark Cows as active {rows} affected");
                }
            }
        }
        public byte[] ExportCow()
        {
            var sheepList = GetAllCow();

            var csvBuilder = new System.Text.StringBuilder();

            csvBuilder.AppendLine("Id,Ear Tag,Breed,CampId,Gender,BirthDate,Price,IsActive,IsPregnant,ExpectedCalvingDate");
            foreach (var item in sheepList)
            {
                csvBuilder.AppendLine($"{item.EarTag},{item.EarTag},{item.Breed},{item.CampId},{item.Gender},{item.BirthDate},{item.Price},{item.IsActive},{item.IsPregnant},{item.ExpectedCalvingDate}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return bytes;

        }
        public byte[] GenerateCowReport()
        {

            var cowQueue = GetAllCow();
            using (var mstream = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(doc, mstream);
                doc.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.BLACK);
                var subTitleFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.DARK_GRAY);

                Paragraph title = new Paragraph("Cow Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);

                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph($"Date Generated: {DateTime.Now:MMMM-dd, yyyy HH:mm}", subTitleFont));
                doc.Add(new Paragraph($"Total Cows: {cowQueue.Count}", subTitleFont));
                doc.Add(new Paragraph("\n\n"));

                // Table Setup

                PdfPTable table = new PdfPTable(9);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] {2f, 1.5f, 1.2f, 2f, 1.5f,1.2f, 1.2f, 2f, 2f });

                // Headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                string[] headers = {"Ear Tag", "Breed", "Camp ID", "Gender", "Birth Date", "Price", "Active", "IsPregnant", "ExpectedCalvingDate" };

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
                foreach (var cow in cowQueue)
                {
                    table.AddCell(new Phrase(cow.EarTag, dataFont));
                    table.AddCell(new Phrase(cow.Breed, dataFont));
                    table.AddCell(new Phrase($"Camp {cow.CampId}", dataFont));
                    table.AddCell(new Phrase(cow.Gender, dataFont));
                    table.AddCell(new Phrase(cow.BirthDate.ToString("MMMM-dd-yyyy"), dataFont));
                    table.AddCell(new Phrase($"R {cow.Price:N2}", dataFont));
                    table.AddCell(new Phrase(cow.IsActive ? "Yes" : "No", dataFont));
                    table.AddCell(new Phrase(cow.IsPregnant ? "Yes" : "No", dataFont));
                    table.AddCell(new Phrase(cow.ExpectedCalvingDate?.ToString("MMMM-dd-yyyy"), dataFont));
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
