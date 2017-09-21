# aid-core
dotnet core port &amp; command line version of amazon-image-downloader.

You need dotnet core 2.0.0 to run or compile the app. To run the app, cd into the project folder, and run `dotnet run`. If you want to download the product video as well, run `dotnet run -v` or `dotnet run --video` from the project folder.

## Known bugs:
- If you start with the `-v` or `--video` flag, but there is no video on the product page, the app will exit with an error.
- If there are no images, the app will exit with an error.

## Future versions:
- Changing the built in User Agent String and Accept String to circumvent bot prevention.
- Ability to feed a list of links from a file, and download *everything*.
- Export product data (name, price, description, rating, etc, photos, videos) into neatly formatted folders.
