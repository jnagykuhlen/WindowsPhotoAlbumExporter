using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Data.SQLite;

namespace WindowsPhotoAlbumExporter
{
    public class Program
    {
        private static string CommandQueryAlbums = "SELECT Album_Name FROM Album;";
        private static string CommandQueryImagePaths = "SELECT Folder_Path || '\\' || Item_FileName FROM Folder, Item, AlbumItemLink WHERE Folder_Id = Item_ParentFolderId AND Item_Id = AlbumItemLink_ItemId AND AlbumItemLink_AlbumId = (SELECT Album_Id FROM Album WHERE Album_Name = :Album_Name);";

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 3)
                    throw new Exception("Too many arguments. Usage:\nWindowsPhotoAlbumExporter.exe [TargetPath] [AlbumName] [DatabasePath]");

                string exportDirectory = null;
                string albumName = null;
                string databasePath = null;

                if (args.Length > 0)
                    exportDirectory = args[0];

                if (args.Length > 1)
                    albumName = args[1];

                if (args.Length > 2)
                    databasePath = args[2];

                if (databasePath == null)
                {
                    try
                    {
                        string packagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages");
                        string photosPath = Directory.EnumerateDirectories(packagesPath, "Microsoft.Windows.Photos_*").First();
                        databasePath = Path.Combine(photosPath, "LocalState\\MediaDb.v1.sqlite");
                    }
                    catch(Exception exception)
                    {
                        throw new Exception("Cannot find photo database.", exception);
                    }
                    
                }

                List<string> imagePaths = new List<string>(512);

                SQLiteFunction.RegisterFunction(typeof(NoCaseLinguisticFunction));

                string connectionString = String.Format("Data Source={0}; Read Only=True;", databasePath);
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    if (albumName == null)
                    {
                        List<string> albumNames = QueryAlbumNames(connection);
                        
                        Console.WriteLine("Found albums:");
                        for (int i = 0; i < albumNames.Count; ++i)
                            Console.WriteLine("({0}) {1}", i + 1, albumNames[i]);

                        Console.WriteLine();
                        Console.WriteLine("Please enter the number or name of the album you want to export:");

                        string input = Console.ReadLine();
                        int index;

                        if (Int32.TryParse(input, out index))
                        {
                            --index;
                            if (index < 0 || index >= albumNames.Count)
                                throw new Exception("Invalid album number.");

                            albumName = albumNames[index];
                        }
                        else
                        {
                            albumName = input;
                        }
                    }

                    imagePaths = QueryImagePaths(connection, albumName);

                    connection.Close();
                }

                if (imagePaths.Count == 0)
                    throw new Exception(String.Format("No photos found in album \"{0}\", or album does not exist.", albumName));

                Console.WriteLine("{0} photos found in album \"{1}\".", imagePaths.Count, albumName);

                if (exportDirectory == null)
                {
                    Console.WriteLine("Please enter the directory the photos should be exported to:");
                    exportDirectory = Console.ReadLine();
                }

                if (!Directory.Exists(exportDirectory))
                    throw new Exception("Export directory does not exist.");
                
                Console.WriteLine("Start copying to \"{0}\".", exportDirectory);

                try
                {
                    int totalItems = imagePaths.Count;
                    for (int i = 0; i < totalItems; ++i)
                    {
                        string sourcePath = imagePaths[i];
                        string destinationPath = Path.Combine(exportDirectory, Path.GetFileName(sourcePath));
                        File.Copy(sourcePath, destinationPath);
                        Console.WriteLine("{0} / {1} items copied ({2}%)", i + 1, totalItems, 100 * (i + 1) / totalItems);
                    }

                    Console.WriteLine("{0} photos were copied successfully.", totalItems);
                }
                catch(Exception exception)
                {
                    throw new Exception("Copying failed.", exception);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }

        private static List<string> QueryAlbumNames(SQLiteConnection connection)
        {
            List<string> albumNames = new List<string>(32);

            using (SQLiteCommand command = new SQLiteCommand(CommandQueryAlbums, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        albumNames.Add(reader.GetString(0));

                    reader.Close();
                }
            }

            return albumNames;
        }

        private static List<string> QueryImagePaths(SQLiteConnection connection, string albumName)
        {
            List<string> imagePaths = new List<string>(512);
            using (SQLiteCommand command = new SQLiteCommand(CommandQueryImagePaths, connection))
            {
                command.Parameters.AddWithValue(":Album_Name", albumName);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        imagePaths.Add(reader.GetString(0));

                    reader.Close();
                }
            }

            return imagePaths;
        }
    }
}
