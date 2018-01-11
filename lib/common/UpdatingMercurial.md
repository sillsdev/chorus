To update the mercurial zip files for the linux distributions you should perform the following steps or some reasonable facsimile:

* On an Ubuntu machine for the appropriate platform add the mercurial releases ppa(ppa:mercurial-ppa/releases)

* Install the mercurial package

* Create a Mercurial folder that you will zip.

* Looking at the installed files copy all of the relevant ones into our zip folder in a way so that local access will work. For example,

		cp /etc/bin/hg to Mercurial/hg
		cp -rL /usr/share/pyshared/mercurial Mercurial/mercurial  
		cp -rL /usr/share/mercurial Mercurial/mercurial  		

	[repeat for hgext directories and whatever else is needed]

* Add an appropriate mercurial.ini file at the root

* Zip up the folder and name it correctly for the platform and replace the old zip in source control. Note that the Mercurial zip file should be zipped on Windows. This solves a problem when unzipping the file with SharpZipLib which otherwise does not extract the `mercurial` subfolder and stores the `mercurial.ini` file with 0 bytes.

To update Mercurial on Windows you will need to do the following:
* Install the desired mercurial, delete the localization files as well as anything else that looks to be large and irrelevant.
* Zip up the folder and copy it over MercurialWindows.zip