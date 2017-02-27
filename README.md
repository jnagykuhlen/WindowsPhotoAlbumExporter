# Overview

In its current version (17.214.10010.0), the Microsoft Photos app on Windows 10 cannot be used reliably to copy all image files contained in an album to a directory on disk. When selecting a large number of album photos, the image data seems to be copied into RAM all at once, stalling the system and eventually crashing the application (observed with 500 pictures in JPG format at 4K resolution, about 2MB each). Even when copying only few pictures at once, the files are first written to an intermediate cache, resulting in additional system load and unnecessary write operations.

Until Microsoft fixes the described issue, this small command line tool can be used to copy image files organized in an album to a specified path on disk or external storage. The files are directly copied from the referenced source path without any intermediate buffering and with minimum resource consumption. It works well with large albums, containing image data of multiple gigabytes.

# Usage

The tool is command-line-only, there exists no GUI at the moment. Simply double-click *WindowsPhotoAlbumExporter.exe* and follow the instructions to select an album and set the export path. Alternatively, the tool can be called with optional parameters as follows:

`WindowsPhotoAlbumExporter.exe [TargetPath] [AlbumName] [DatabasePath]`

Image files are exported to the directory specified by `TargetPath`. When omitted, the path must be typed in later on. `AlbumName` is the name of the album as chosen in the Microsoft Photos app. When this parameter is not specified, the tool will provide a list of albums found on this PC from which the user can choose. The third parameter `DatabasePath` is normally not required since this tool will find the database storing album information automatically.

When you worry about your album data being corrupted when using this tool, consider creating a backup of the database found at the following path:

`C:\Users\[Username]\AppData\Local\Packages\Microsoft.Windows.Photos_[Id]\LocalState\MediaDb.v1.sqlite`

The copy can be targeted by this tool instead of the original file by specifying the `Database` parameter accordingly.

# How to build

Download *System.Data.SQlite* from the [official website](https://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki). Create a folder named `External` in the repository root directory and move the *System.Data.SQlite.dll* there. Finally, compile using Visual Studio.

# Insights

The Microsoft Photo app seems to store information on all albums in a central SQLite database found at the path mentioned above. Besides additional metadata, an album is essentially a list of paths to the source picture files stored somewhere else on disk. Thus, there is no single folder in which all photos of an album can be found to copy them somewhere else using Windows Explorer. The purpose of this tool is to open the SQLite database, query the paths of all pictures belonging to the album in question and then copying each of these files to the target directory one by one.
