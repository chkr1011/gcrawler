# gcrawler
gcrawler is a GIF crawler written in C#. 

## Pages
The initial pages (URL) can be added to the App.config file. The application starts to parse the initial pages and searches for image tags. The images behind these tags are downloaded and checked for animations and a minimum size. The matching files are moved to a directory called "Content". Then, all links to other pages are being processed.

It is possible to specify a hop limit. A hop is defined by a new sub page.

The application creates an index of all parsed pages and skips them if the page has already been processed. These information is currently saved for further executions.

## Images
The application creates a hash for each downloaded GIF and is able to detect duplicates by using SHA-1 hashes. This allowes restarting of the application without added the images to the content directory multiple times.

## PiVi
The Picture Viewer can be used to show all downloaded GIFs.
