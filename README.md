# aid-core
dotnet core port &amp; command line version of [amazon-image-downloader](https://github.com/bodzaital/amazon-image-downloader).

You need dotnet core 2.0.0 to run or compile the app. To run the app, cd into the project folder, and run `dotnet run`.

By default, aid-core will download all available product images. To download video as well, add the `-v` flag. To display debug information in case of an error, add the `-d` flag. To define a custom user agent, use the `-u` flag, then, in double quotes, paste in your string. To define a custom accept string, use the `-a` flag, then, in double quotes, paste your string. To download a batch of links, use the `-t` flag, then give the name of the text file which contains one Amazon product link in a line.

Default user-agent: `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36`  
Default accept: `text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8`

## Examples:

- Download video: `dotnet run -v`
- Download a batch of files with custom user agent and accept string: `dotnet run -t list.txt -u "user-agent" -a "accept-string"`
- Download a batch of files, and display any error message in length: `dotnet run -t list.txt -d`

## Known bugs:
- If there are no images, the app will exit with an error.
