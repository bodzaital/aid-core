# aid-core v.2

Download all high resolution images and videos from an Amazon product page. Tested only on clothing categories.

## Usage

The target framework is Dotnet Core 2.1.

To download all images and videos: `dotnet aid-core2.dll -u "link"`. The download destination is a folder with the name of the product (or the link to the product).

If a product is already downloaded, the program won't download it again. To override this setting, add the `-!` flag: `dotnet aid-core2.dll -u "link" -!`

Amazon's servers may refuse the connection (503 error). Simply retry again. If the app reports a 503 error every time, change the User Agent in the source code on line 59 in `Program.cs` and recompile (TODO: use cli arguments to do so).

## Changes from v1.1

- The app parses the JSON object using JSON.net instead of half-assed regex.
- Amazon delivers the source code using gzip, so it is decompressed.
- Much cleaner code, less prone to exceptions.
