<h1>Batch Zip Decrypt</h1>

Command-line tool that can recursively decrypt all ZIP/CBZ files in a folder and its sub-directors, using the provided password or multiple passwords from a file to try.
Encryption option to be added in the future.

This tool updates the files in-place, no TEMP files are created elsewhere on disc, copying is handled in memory. (May run into issue with files in the gigabytes of size, have not tested).

File will not be updated if decryption was unsuccessful or aren't password protected, so no worries there. 
Any files without .ZIP or .CBZ extension are ignored.

<h3>Usage:</h3>

`BatchZipDecrypt.exe -path D:\MyZipFiles -passfile D:\passfile.txt`

`-pass PASSWORD` can be used instead of `-passfile` if all the files only have the same password.
