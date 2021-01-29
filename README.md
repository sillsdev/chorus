Overview
========

Chorus is a version control system designed to enable workflows appropriate for typical language
development teams who are geographically distributed. These teams need to edit a set of common
files, working towards a publication. They want to share their work while, crucially, being able
to defer dealing with conflicting edits for periods of time or until a qualified team member can
make decisions about the conflicts. The system is implemented on top of a commonly-used Open
Source Distributed Version Control System. It works in scenarios in which users are connected by
Local Area Network, Internet, or hand-carried storage devices. Chorus supports several workflow
models, including those that maintain a “master” database separate from the incoming submissions
of team members. Quite unlike the version control systems commonly in use, Chorus works
invisibly for the common cases and is kept simple for even beginner computer users.

## Distinctive Features

These features come for free with any Distribute Version Control System:

 * Share files between users, even if they are never connected to the internet.

 * Every member of the team has access to a full history of all work done by the rest of the team.

 * In a crisis, work can be "rolled back" to a previous version.


However, "raw" Distributed Version Controls Systems are relatively difficult to understand,
configure, and use, even for computer-savvy workers.


The following list of features should help you understand why we built this layer over a raw version control system:


 * silently synchronize; will never tell the user to manually merge conflicts first

 * automatically check for team members & devices with which to synchronize

 * Support a Master branch which does not automatically accept changes from anyone

 * Files can be marked as shared by the team or user-specific. This allows things like
   preferences/configurations to be part of the repository but kept separate for each
   individual. This will also allow one team member to make configuration changes for another,
   remote member, and push those changes through the system to that user, without physically
   accessing their computer.

 * 3-Way, schema-savvy XML merging. Various policies can be implemented for choosing a winner in
   the case of conflicts. Regardless of the policy, details of the conflict are logged in an XML
   file which also under version control. At a time and place of the team's choosing, these
   automatic choices can be reviewed and reversed.

 * Configuration help from applications. Applications generally know where their important files
   are, which files are individual-specific, and which should not be backed-up/shared at all.
   Applications that know about Chorus pass this information to it, so that users don't need to
   become experts in how all the files work.

 * Synchronization help from application. Applications often know what points are good ones for
   checking data in. For example, when exiting, or before doing a large and possibly undesirable
   operation, like deleting a large number of items or importing a new data set.

 * In-Application conflict and change history. Rather than ask users to learn
   version-control-specific tools, the Chorus model is that Chorus provides the raw information
   applications need to provide a smooth, integrated workflow in the same environment as the user
   has for editing. For example, a dictionary-editing program using Chorus will allow the user to
   see a full history of the current record, including who made what changes, and what conflicts
   (if any) were encountered during synchronization.

 * A built-in "notes" system which makes it very cheap to give users the ability to add notes to
   any piece of data, and to carry on conversations about the data until they mark the
   note as "resolved".

### Status

Chorus is functional and being used in several applications with different development teams.
However, we are not really interested in supporting
any further uses until things mature and someone writes good developer documentation.
Documentation (what little exists) drips out in the form of occasional blogs
[here](http://chorussr.wordpress.com/).

## Testers

Please see [Tips for Testing Palaso Software](https://docs.google.com/document/d/1dkp0edjJ8iqkrYeXdbQJcz3UicyilLR7GxMRIUAGb1E/edit)

To send and receive with the test server over the Internet, set the following environment variable:

	LANGUAGEFORGESERVER = -qa.languageforge.org

## Developers

### Mailing List

Sign up here: https://groups.google.com/forum/#!forum/chorusdev

### Road Map & Workflow

https://trello.com/board/chorus/4f3a90277ae2b69b010988ac

### Coding Standards

[Palaso Coding Standards](https://docs.google.com/document/d/1t4QVHWwGnrUi036lOXM-hnHVn15BbJkuGVKGLnbo4qk/edit)

### Source Code

Chorus is written in C#. The UI widgets use Windows Forms, but you could make your own using a
different platform and just use the engine.

After cloning the project you should now have a solution that you can build using any edition
of Visual Studio 2010, including the free Express version. We could help you do it in VS 2008,
if necessary.

On Linux you can open and build the solution in MonoDevelop, or run the `build/TestBuild.sh` script.

### Getting up-to-date libraries

The source tree contains a script that downloads all necessary dependencies.

#### Windows

Download "git bash" from "https://git-scm.com/download/win" and open a "git bash" window, then change into the build directory and run the `buildupdate.win.sh` script:

	cd /c/dev/chorus/build
	./buildupdate.win.sh

Alternately, if you are working on libpalaso or another dependency, you can update to your local
build using `UpdateDependencies.bat`, which is run automatically when you run `GetAndBuildThis.bat`:

	cd c:\dev\chorus
	UpdateDependencies.bat

##### Mercurial

If developing on windows, unzip the file `lib/common/mercurial.zip` into `output/Debug` (or `output/Release`), so that you'll end up with a subdirectory `output/Debug/Mercurial`. That way, you know the tests are running against the approved version of Mercurial, not whatever you happen to have on your machine. Then, open C:\dev\chorus\Mercurial\default.d\cacerts.rc and change the path to C:\dev\chorus\Mercurial\cacert.pem (required to send and receive to the Internet using the High bandwidth option).

#### Linux

In order to build and run all the tests on Linux the SIL mono package will need to be installed.
It can be found at http://packages.sil.org/
That version of mono will be used when you run `build/TestBuild.sh`

Open a terminal window, change into the build directory and run the `buildupdate.mono.sh` script:

	cd chorus/build
	./buildupdate.mono.sh

Alternately, if you are working on libpalaso or another dependency, you can update to your local
build using `UpdateDependencies.sh`:

	cd chorus
	./UpdateDependencies.sh

