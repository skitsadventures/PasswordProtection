using System.Security.Cryptography; //Added for Hashing functions
using OfficeOpenXml; // Used to insert information onto an Excel sheet
using System.Text;
using System.Linq; // Added for SequenceEqual
using System.IO;
using System;


namespace MyNamespace
{
    public class NameAsked
    {
        public string firstName;

        public void NameInsert()
        {
            do
            {
                Console.WriteLine("Enter your name: ");
                firstName = Console.ReadLine().Trim();

                if (string.IsNullOrEmpty(firstName))
                {
                    Console.WriteLine("Name cannot be empty. Please enter a valid name.");
                }
            } while (string.IsNullOrEmpty(firstName));

            Console.Write("Is the name correct? (yes/no): ");
            IncorrectInput incorrectInput = new IncorrectInput();
            incorrectInput.IncorrectName(this);     //the word "this" refers to this Class' object
        }
    }

    public class AgeAsked
    {
        public int initialAge;

        public void AgeInsert()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Enter your age: ");

            try
            {
                initialAge = int.Parse(Console.ReadLine());
            }
            catch (FormatException)
            {
                Console.WriteLine("Input is not a valid integer. You will be set to 0 years old");
            }

            Console.Write("Is the age correct? (yes/no): ");

            IncorrectInput incorrectInput = new IncorrectInput();
            incorrectInput.IncorrectAge(this);
        }
    }

    public class PWAsked
    {
        public string password;
        public string passwordRetype;

        public void PWInsert()
        {
            // TODO
            // Password is able to become a Null value

            Console.WriteLine("Enter your password: ");
            password = Console.ReadLine().Trim();

            Console.Write("Please retype: ");
            passwordRetype = Console.ReadLine().Trim();

            IncorrectInput incorrectInput = new IncorrectInput();
            incorrectInput.IncorrectPassword(this);     //the word "this" refers to this Class' object
        }

    }

    public class IncorrectInput
    {
        public void IncorrectName(NameAsked nameAsked)
        {
            bool boolCorrectName = true;

            {
                while (boolCorrectName == true)
                {
                    string response = Console.ReadLine();
                    if (response.ToLower() == "y" || response.ToLower().Trim() == "yes")
                    {
                        boolCorrectName = false;
                    }
                    else
                    {
                        nameAsked.NameInsert();
                        boolCorrectName = false;
                    }
                }
            }
        }

        public void IncorrectAge(AgeAsked ageAsked)
        {
            bool boolCorrectAge = true;

            {
                while (boolCorrectAge == true)
                {
                    string response = Console.ReadLine();
                    if (response.ToLower() == "y" || response.ToLower() == "yes")
                    {
                        boolCorrectAge = false;
                    }
                    else
                    {
                        ageAsked.AgeInsert();
                        boolCorrectAge = false;
                    }
                }
            }
        }

        public void IncorrectPassword(PWAsked pwAsked)
        {
            bool boolCorrectPassword = true;

            {
                while (boolCorrectPassword == true)
                {
                    string response = Console.ReadLine();
                    if (pwAsked.password == pwAsked.passwordRetype)
                    {
                        boolCorrectPassword = false;
                    }
                    else
                    {
                        Console.WriteLine("The 2 weren't spelt the same. Please retype: ");
                        pwAsked.PWInsert();
                        boolCorrectPassword = false;


                    }
                }
            }
        }
    }

    public class FileWriter
    {
        public void WriteToFile(string filePath, string excelFilePath, string name, string password, string hashedPassword, int age, byte[] salt)
        {

            // Set the EPPlus LicenseContext
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Writing user details to file in append mode
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("\n" + "Here's your details:");
                writer.WriteLine($"Name: {name}");
                writer.WriteLine($"Age: {age}");
                writer.WriteLine($"Password: {password}");
                writer.WriteLine($"Salt: {Convert.ToBase64String(salt)}"); // Convert salt to Base64 string for storage
                writer.WriteLine($"hashedPassword: {hashedPassword}"); // Convert salt to Base64 string for storage
                writer.WriteLine("----------------------------------"); // Add an empty line for separation
            }

            // Writing user details to Excel file
            FileInfo excelFile = new FileInfo(excelFilePath);
            using (ExcelPackage excelPackage = new ExcelPackage(excelFile))
            {
                ExcelWorksheet worksheet = null;

                // Check if the worksheet with the name "ExcelLoginTest" already exists
                if (excelPackage.Workbook.Worksheets.Any(ws => ws.Name == "ExcelLoginTest"))
                {
                    worksheet = excelPackage.Workbook.Worksheets["ExcelLoginTest"];
                }
                else
                {
                    // Create a new worksheet named "ExcelLoginTest" if it doesn't exist
                    worksheet = excelPackage.Workbook.Worksheets.Add("ExcelLoginTest");

                    // Add headers since it's a new worksheet
                    worksheet.Cells["A1"].Value = "Name";
                    worksheet.Cells["B1"].Value = "Age";
                    worksheet.Cells["C1"].Value = "Password";
                    worksheet.Cells["D1"].Value = "Salt";
                    worksheet.Cells["E1"].Value = "EndResultHash";
                }

                // Find the next available row
                int nextRow = worksheet.Cells["A" + worksheet.Dimension.End.Row].End.Row + 1;

                // Add user details to the next available row
                worksheet.Cells["A" + nextRow].Value = name;
                worksheet.Cells["B" + nextRow].Value = age;
                worksheet.Cells["C" + nextRow].Value = password;
                worksheet.Cells["D" + nextRow].Value = Convert.ToBase64String(salt);
                worksheet.Cells["E" + nextRow].Value = hashedPassword;

                // Save the changes to the Excel file
                excelPackage.Save();
            }
        }
    }

    public class HashClass
    {
        public string HashPassword(string password, byte[] salt)
        {
            // Salted hashing with SHA-256
            using (SHA256 sha256Hash = SHA256.Create())
            {

                // Combine password and salt
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length]; // A new byte array is created with the size of the password byte array plus the salt byte array.

                //This array calls which array to copy elements, calls for array that recieves elements (saltedPassword) 
                Array.Copy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);

                // After this line executes, saltedPassword will contain both the password bytes and the salt bytes
                Array.Copy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                // Compute hash
                byte[] hashedBytes = sha256Hash.ComputeHash(saltedPassword);

                // Convert byte array to a string
                string hashedPassword = Convert.ToBase64String(hashedBytes);

                // Combine the salt and hashed password for storage
                string finalHashedPassword = Convert.ToBase64String(salt) + "" + hashedPassword;

                return finalHashedPassword;
            }
        }


        public byte[] GenerateSalt()
        {
            const int saltSize = 18; // 18 bytes for salt
            byte[] salt = new byte[saltSize];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            // Extract salt and hash from the stored password
            string[] parts = hashedPassword.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHash = Convert.FromBase64String(parts[1]);

            // Compute hash using input password and stored salt
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                Array.Copy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Array.Copy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                byte[] computedHash = sha256Hash.ComputeHash(saltedPassword);

                // Compare the computed hash with the stored hash
                return storedHash.SequenceEqual(computedHash);
            }
        }
    }

    public class Login
    {
        public bool AuthenticateUser(string excelFilePath)
        {
            while (true)
            {
                Console.WriteLine("Are you a new user? (yes/no)");
                string userCheck = Console.ReadLine().Trim().ToLower();

                if (userCheck == "yes" || userCheck == "y")
                {
                    // Perform logic for new user
                    Console.WriteLine("You are a new user. Please create an account.");
                    return true; // Indicate new user
                }
                else if (userCheck == "no" || userCheck == "n")
                {
                    // Perform authentication for existing user
                    Console.WriteLine("Enter your name: ");
                    string name = Console.ReadLine().Trim();

                    HashClass hashClass = new HashClass();

                    // Reading Excel file to find user details
                    using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(excelFilePath)))
                    {
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["ExcelLoginTest"];

                        // Find the row containing the user's name
                        int row = 2; // Start from the second row (assuming headers are in the first row)
                        while (worksheet.Cells[row, 1].Value != null && worksheet.Cells[row, 1].Value.ToString().Trim() != name)
                        {
                            row++;
                        }

                        if (worksheet.Cells[row, 1].Value == null)
                        {
                            Console.WriteLine("User not found.");
                            if (!AskRetry())
                            {
                                Environment.Exit(0); // Exit the program
                            }
                            continue; // Retry authentication
                        }

                        string storedSalt = worksheet.Cells[row, 4].Value.ToString();
                        string storedHashedPassword = worksheet.Cells[row, 5].Value.ToString();

                        Console.WriteLine("Enter your password: ");
                        string passwordCheck = Console.ReadLine().Trim();

                        // Generate hash for provided password and stored salt
                        byte[] salt = Convert.FromBase64String(storedSalt);
                        string finalHashCheck = hashClass.HashPassword(passwordCheck, salt);

                        // Compare the computed hash with the stored hash
                        if (finalHashCheck == storedHashedPassword)
                        {
                            Console.WriteLine("Authentication successful.");
                            return false; // Indicate existing user
                        }
                        else
                        {
                            Console.WriteLine("Authentication failed.");
                            if (!AskRetry())
                            {
                                Environment.Exit(0); // Exit the program
                            }
                            continue; // Retry authentication
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 'yes' or 'no'.");
                }
            }
        }

        private bool AskRetry()
        {
            while (true)
            {
                Console.WriteLine("Would you like to try again? (yes/no)");
                string retryInput = Console.ReadLine().Trim().ToLower();
                if (retryInput == "yes" || retryInput == "y")
                {
                    return true; // Retry
                }
                else if (retryInput == "no" || retryInput == "n")
                {
                    return false; // Do not retry
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 'yes' or 'no'.");
                }
            }
        }
    }

    public class Ascii
    {
        public void Welcome()
        {
            Console.WriteLine(" ▄█     █▄     ▄████████  ▄█        ▄████████  ▄██████▄    ▄▄▄▄███▄▄▄▄      ▄████████ ");
            Console.WriteLine("███     ███   ███    ███ ███       ███    ███ ███    ███ ▄██▀▀▀███▀▀▀██▄   ███    ███ ");
            Console.WriteLine("███     ███   ███    █▀  ███       ███    █▀  ███    ███ ███   ███   ███   ███    █▀  ");
            Console.WriteLine("███     ███  ▄███▄▄▄     ███       ███        ███    ███ ███   ███   ███  ▄███▄▄▄     ");
            Console.WriteLine("███     ███ ▀▀███▀▀▀     ███       ███        ███    ███ ███   ███   ███ ▀▀███▀▀▀     ");
            Console.WriteLine("███     ███   ███    █▄  ███       ███    █▄  ███    ███ ███   ███   ███   ███    █▄  ");
            Console.WriteLine("███ ▄█▄ ███   ███    ███ ███▌    ▄ ███    ███ ███    ███ ███   ███   ███   ███    ███ ");
            Console.WriteLine(" ▀███▀███▀    ██████████ █████▄▄██ ████████▀   ▀██████▀   ▀█   ███   █▀    ██████████ ");
            Console.WriteLine("                         ▀                                                               " + "\n" + "\n");
        }
    }
}