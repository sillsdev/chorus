To update the mercurial zip files for the linux distributions you should perform the following steps or some reasonable facsimile:

* On an Ubuntu machine for the appropriate platform add the mercurial releases ppa(ppa:mercurial-ppa/releases)

* Create a Mercurial folder that you will zip.

* Looking at the installed files copy all of the relevant ones into our zip folderin a way so that local access will work:
e.g.
cp /etc/bin/hg to Mercurial/hg
cp -rL /usr/share/pyshared/mercurial Mercurial/mercurial
cp -rL /usr/share/mercurial Mercurial/mercurial
[repeat for hgext directories and whatever else is needed]

* Add an appropriate mercurial.ini file at the root

* Zip up the folder and name it correctly for the platform and replace the old zip in source control.
