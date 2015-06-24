# gcrawler
gcrawler is a GIF crawler written in C#. 

## Pages
The initial pages (URL) can be added to the App.config file. The application starts to parse the initial pages and searches for image tags. The images behind these tags are downloaded and checked for animations and a minimum size. The matching files are moved to a directory called "Content". Then, all links to other pages are being processed.

It is possible to specify a hop limit. A hop is defined by a new sub page.

## PiVi
The Picture Viewer can be used to show all downloaded GIFs.
