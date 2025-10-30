using Microsoft.Data.SqlClient;
using LiveStock.Core.Models;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
// Note: Avoid importing iText.Kernel.Geom to prevent 'Path' ambiguity with System.IO.Path
using System.Data;
using System.Threading.Tasks;

namespace LiveStock.Web.Service
{
    public class sheepService
    {
        private readonly string _conString;

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Initializes sheep service with database connection string"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public sheepService(IConfiguration configuration)
        {
            _conString = configuration.GetConnectionString("AzureConString");
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Inserts a new sheep and returns the generated ID"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public int AddSheep(string breed, DateOnly birthDate, int camp, DateTime createdAt, string gender, decimal price, string? notes, string? photoURL)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                con.Open();

                // Insert sheep and return generated Id (avoid SheepID column entirely)
                string sql = @"INSERT INTO Sheep (Breed, BirthDate, CampId, Gender, Price, Notes, PhotoUrl, Status, IsActive, CreatedAt)
                               OUTPUT INSERTED.Id
                               VALUES(@breed, @birthDate, @campId, @gender, @price, @notes, @photoUrl, @status, @isActive, @createdAt)";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@breed", breed);
                    cmd.Parameters.AddWithValue("@birthDate", birthDate.ToDateTime(new TimeOnly(0,0)));
                    cmd.Parameters.AddWithValue("@campId", camp);
                    cmd.Parameters.AddWithValue("@gender", gender);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@notes", (object?)notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@photoUrl", (object?)photoURL ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", "Active");
                    cmd.Parameters.AddWithValue("@isActive", true);
                    cmd.Parameters.AddWithValue("@createdAt", createdAt);

                    var insertedIdObj = cmd.ExecuteScalar();
                    int insertedId = Convert.ToInt32(insertedIdObj);
                    return insertedId;
                }
            }
        }
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Fetches all sheep from the database and maps to models"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public Queue<Sheep> GetAllSheep()
        {
            Queue<Sheep> sheepQueue = new Queue<Sheep>();

            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "SELECT Id, Breed, CampId, Gender, Price, IsActive, BirthDate, Status, CreatedAt, UpdatedAt, Notes, PhotoUrl FROM Sheep";
                SqlCommand cmd = new SqlCommand(sql, con);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sheep = new Sheep
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            CampId = reader.GetInt32(reader.GetOrdinal("CampId")),
                            Gender = reader.GetString(reader.GetOrdinal("Gender")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                            BirthDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("BirthDate"))),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? (string?)null : reader.GetString(reader.GetOrdinal("Notes")),
                            PhotoUrl = reader.IsDBNull(reader.GetOrdinal("PhotoUrl"))
                                       ?(string?)null : reader.GetString(reader.GetOrdinal("PhotoUrl"))

                            /*
                             * reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                        ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                             */
                        };
                        sheepQueue.Enqueue(sheep);
                    }
                }
            }

            return sheepQueue;
        }
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Deletes a sheep by ID from the database"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public void DeleteSheep(int id)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "DELETE FROM Sheep WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    Console.WriteLine($"No sheep found");
                }
                else
                {
                    Console.WriteLine($"Sheep with ID {id} deleted successfully.");
                }
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Returns the specified sheep's details in a queue"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public Queue<Sheep> getSheepById(int id)
        {
            Queue<Sheep> sheepList = new Queue<Sheep>();

            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "SELECT Id, Breed, CampId, Gender, Price, IsActive, BirthDate, Status, CreatedAt, UpdatedAt, Notes, PhotoUrl FROM Sheep WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sheep = new Sheep
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
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
                            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? (string?)null : reader.GetString(reader.GetOrdinal("Notes")),
                             PhotoUrl = reader.IsDBNull(reader.GetOrdinal("PhotoUrl"))
                                       ? (string?)null : reader.GetString(reader.GetOrdinal("PhotoUrl"))

                        };
                        sheepList.Enqueue(sheep);
                    }
                }
            }

            return sheepList;
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Fills missing fields on current sheep entries using new details"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public Queue<Sheep> FillVoidSheppFields(Queue<Sheep> currentShepp, Queue<Sheep> newSheppDetails)
        {
            Queue<Sheep> result = [];

            foreach (var current in currentShepp)
            {
                var updated = newSheppDetails.FirstOrDefault(s => s.Id == current.Id);

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
                    current.Notes = string.IsNullOrEmpty(updated.Notes) ? current.Notes : updated.Notes;
                }
                result.Enqueue(current);

            }

            return result;
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates sheep properties including photo, notes, and timestamps"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public void UpdateSheep(Sheep updateSheep)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = " UPDATE Sheep SET Breed = @breed, CampId = @camp, Gender = @gender, Price = @price, BirthDate = @birthDate, UpdatedAt = @updatedAt, Notes = @notes, PhotoUrl = @photoUrl WHERE Id = @id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@breed", updateSheep.Breed);
                    cmd.Parameters.AddWithValue("@camp", updateSheep.CampId);
                    cmd.Parameters.AddWithValue("@price", updateSheep.Price);
                    cmd.Parameters.AddWithValue("@birthDate", updateSheep.BirthDate.ToDateTime(new TimeOnly(0,0)));
                    cmd.Parameters.AddWithValue("@gender", updateSheep.Gender);
                    cmd.Parameters.AddWithValue("@notes", (object?)updateSheep.Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@photoUrl", updateSheep.PhotoUrl);
                    cmd.Parameters.AddWithValue("@id", updateSheep.Id);
                    cmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        Console.WriteLine($"No sheep dound with the ID {updateSheep.Id}");
                    }
                    else
                    {
                        Console.WriteLine($"Updated {updateSheep.Id}");
                    }
                }

            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Performs bulk operations (sell, delete, move, status) on sheep"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public void SheepBulkActions(string action, string reason, HashSet<int> ids)
        {
            switch (action)
            {
                case "markSold":
                    foreach (int id in ids)
                    {
                        // Soft-sell only: update status and record income
                        MarkSheepAsSold(id, reason);
                    }
                    break;

                case "delete":
                    foreach (int id in ids)
                    {
                        // Remove dependent records first to avoid FK conflicts
                        using (SqlConnection con = new SqlConnection(_conString))
                        {
                            con.Open();
                            using (SqlCommand delMed = new SqlCommand("DELETE FROM MedicalRecords WHERE AnimalType = 'Sheep' AND SheepId = @id", con))
                            {
                                delMed.Parameters.AddWithValue("@id", id);
                                delMed.ExecuteNonQuery();
                            }
                            using (SqlCommand delMoves = new SqlCommand("DELETE FROM CampMovements WHERE SheepId = @id", con))
                            {
                                delMoves.Parameters.AddWithValue("@id", id);
                                delMoves.ExecuteNonQuery();
                            }
                        }

                        // Hard delete the sheep
                        DeleteSheep(id);
                    }
                    break;

                case "move":
                    if (int.TryParse(reason, out int newCampId))
                    {
                        foreach (int id in ids)
                        {
                            MoveSheepToCamp(id, newCampId);
                        }

                    }
                    break;

                case "markInactive":
                    foreach (int id in ids)
                    {
                        MarkSheepAsInactive(id);
                    }
                    break;

                case "markAactive":
                    foreach (int id in ids)
                    {
                        MarkSheepAsActive(id);
                    }
                    break;
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Soft marks a sheep as sold and records income in finance"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public void MarkSheepAsSold(int id, string? reference)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                con.Open();

                // Get sale amount (Price) and basic info
                decimal? amount = null;
                string breed = string.Empty;
                using (SqlCommand getCmd = new SqlCommand("SELECT Price, Breed FROM Sheep WHERE Id = @id", con))
                {
                    getCmd.Parameters.AddWithValue("@id", id);
                    using (var reader = getCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            amount = reader.IsDBNull(0) ? (decimal?)null : reader.GetDecimal(0);
                            breed = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        }
                    }
                }

                if (amount == null)
                {
                    // No sheep found or price missing; skip
                    return;
                }

                // Mark sheep as sold (soft-delete style)
                using (SqlCommand updCmd = new SqlCommand("UPDATE Sheep SET Status = @status, IsActive = 0, UpdatedAt = @updatedAt WHERE Id = @id", con))
                {
                    updCmd.Parameters.AddWithValue("@status", "Sold");
                    updCmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
                    updCmd.Parameters.AddWithValue("@id", id);
                    updCmd.ExecuteNonQuery();
                }

                // Record income in FinancialRecords
                using (SqlCommand finCmd = new SqlCommand(@"INSERT INTO FinancialRecords
                    (Type, Description, Amount, TransactionDate, Category, Reference, Notes, CreatedAt)
                    VALUES (@type, @description, @amount, @transactionDate, @category, @reference, @notes, @createdAt)", con))
                {
                    finCmd.Parameters.AddWithValue("@type", "Income");
                    finCmd.Parameters.AddWithValue("@description", $"Sheep Sold - ID {id} ({breed})");
                    finCmd.Parameters.AddWithValue("@amount", amount);
                    finCmd.Parameters.AddWithValue("@transactionDate", DateTime.UtcNow);
                    finCmd.Parameters.AddWithValue("@category", "Livestock Sales");
                    finCmd.Parameters.AddWithValue("@reference", (object?)reference ?? DBNull.Value);
                    finCmd.Parameters.AddWithValue("@notes", DBNull.Value);
                    finCmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
                    finCmd.ExecuteNonQuery();
                }
            }
        }
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Updates a sheep's assigned camp"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public void MoveSheepToCamp(int id, int campID)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "UPDATE Sheep SET CampId = @campId WHERE Id = @id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@campId", campID);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Move Sheep to camp {rows} affected");

                }
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Marks a sheep as inactive"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public void MarkSheepAsInactive(int id)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                const string sql = "UPDATE Sheep SET IsActive = 0 WHERE Id = @id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Mark Sheep as inactive {rows} affected");
                }
            }
        }

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Marks a sheep as active"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public void MarkSheepAsActive(int id)
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
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Exports sheep data to a CSV byte array"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public byte[] ExportSheep()
        {
            var sheepList = GetAllSheep();

            var csvBuilder = new System.Text.StringBuilder();

            csvBuilder.AppendLine("Id,Breed,CampId,Gender,BirthDate,Price,IsActive");
            foreach (var item in sheepList)
            {
                csvBuilder.AppendLine($"{item.Id},{item.Breed},{item.CampId},{item.Gender},{item.BirthDate},{item.Price},{item.IsActive}");

            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return bytes;

        }
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Generates a PDF report summarizing sheep details"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public byte[] GenerateSheepReport()
        {

            var sheepList = GetAllSheep();
            using (var mstream = new MemoryStream())
            {
                var writer = new PdfWriter(mstream);
                var pdf = new PdfDocument(writer);
                var doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
                doc.SetMargins(50, 50, 50, 50);

                // Title
                var title = new Paragraph("Sheep Report")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetBold();
                doc.Add(title);

                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph($"Date Generated: {DateTime.Now:MMMM-dd, yyyy HH:mm}")
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.GRAY));
                doc.Add(new Paragraph($"Total Sheep: {sheepList.Count}")
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.GRAY));
                doc.Add(new Paragraph("\n\n"));

                // Table Setup
                var table = new Table(new float[] { 1.2f, 2f, 1.5f, 1.2f, 2f, 1.5f, 1.2f });
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                string[] headers = { "ID", "Breed", "Camp ID", "Gender", "Birth Date", "Price", "Active" };
                foreach (string header in headers)
                {
                    var cell = new Cell()
                        .Add(new Paragraph(header).SetBold().SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(new DeviceRgb(52, 73, 94))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPadding(6);
                    table.AddCell(cell);
                }
                // Data
                foreach(var sheep in sheepList)
                {
                    table.AddCell(new Cell().Add(new Paragraph(sheep.Id.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(sheep.Breed)));
                    table.AddCell(new Cell().Add(new Paragraph($"Camp {sheep.CampId}")));
                    table.AddCell(new Cell().Add(new Paragraph(sheep.Gender)));
                    table.AddCell(new Cell().Add(new Paragraph(sheep.BirthDate.ToString("MMMM-dd-yyyy"))));
                    table.AddCell(new Cell().Add(new Paragraph($"R {sheep.Price:N2}")));
                    table.AddCell(new Cell().Add(new Paragraph(sheep.IsActive ? "Yes": "No")));
                }
                doc.Add(table);

                // Footer
                doc.Add(new Paragraph("\n\n"));
                var footer = new Paragraph("")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.GRAY);
                doc.Add(footer);
                doc.Close();

                return mstream.ToArray();
            }   

        } 

        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Saves an uploaded sheep photo to disk and returns its URL"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public async Task<string> SaveSheepPhoto(IFormFile photo)
        {

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/sheep");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }
            
            return "/uploads/sheep/" + fileName;
        }
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        // "Deletes the sheep's photo file from disk if it exists"
        //-_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_ -_-_-_-_-_-_-_-_
        public void DeleteSheepPhoto(int id)
        {
            var sheepList = getSheepById(id);
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
