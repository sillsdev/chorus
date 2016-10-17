(Re)building buildUpdate.sh requires ruby and https://github.com/chrisvire/BuildUpdate
Here's the command line commands I used:

cd <path to where you want to generate the update scripts>
<your path to buildupdate.rb (part of BuildUpdate repo above)>\buildupdate.rb -t bt2 -f buildupdate.win.sh -r ..
<your path to buildupdate.rb (part of BuildUpdate repo above)>\buildupdate.rb -t bt323 -f buildupdate.mono.sh -r ..

Explanation:

"-t bt2" points at the Windows configuration that tracks this branch
"-t bt323" points at the Linux configuration that tracks this branch
"-f ____" gives what I want the file to be called
"-r .." takes care of moving the context up from build to the root directory