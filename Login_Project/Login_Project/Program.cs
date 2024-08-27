using System;
using OfficeOpenXml;

namespace MyNamespace
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Login Function";

            // Create instances here
            FileWriter writeFile = new FileWriter();
            HashClass hashClass = new HashClass(); // Create an instance of HashClass
            NameAsked urName = new NameAsked();
            AgeAsked urAge = new AgeAsked();
            PWAsked urPW = new PWAsked();
            Login login = new Login();
            Ascii ascii = new Ascii();

            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Define file paths
            string filePath = @"C:\Users\mackr\Desktop\Names.txt";
            string excelFilePath = @"C:\Users\mackr\Desktop\Names.xlsx";

            ascii.Welcome();
            Console.ForegroundColor = ConsoleColor.Gray;

            bool isNewUser = login.AuthenticateUser(excelFilePath); // This asks the user if they are a new user or not

            if (isNewUser)
            {
                urName.NameInsert();
                urAge.AgeInsert();
                urPW.PWInsert(); //Asks for Password and Password Confirmation

                // Generate salt
                byte[] salt = hashClass.GenerateSalt(); // Define and generate salt here

                // Hash password with the salt
                string hashedPassword = hashClass.HashPassword(urPW.password, salt);

                // Calling the WriteToFile method of NameAsked to write user details
                writeFile.WriteToFile(filePath, excelFilePath, urName.firstName, urPW.password, hashedPassword, urAge.initialAge, salt);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Clear();
                //Prints values at the end
                Console.WriteLine("\n" + "Here's your details: ");
                Console.WriteLine($"Name: {urName.firstName}");
                Console.WriteLine($"Age: {urAge.initialAge}");
                Console.WriteLine($"Password: {urPW.password}");
                Console.WriteLine($"hashedPassword: {hashedPassword}");
                //Console.WriteLine($"UniqueSalt: {salt}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Hello you");
            }

            Console.ReadLine();
        }
    }
}