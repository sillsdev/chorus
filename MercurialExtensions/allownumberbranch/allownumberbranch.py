# allownumberbranch.py
#
# Copyright (c) 2015 SIL International
# This software is licensed under the LGPL, version 2.1 or later
# (http://www.gnu.org/licenses/lgpl-2.1.html)
#
# The version of Mercurial first released with chorus allowed branches to
# be named with just a number. We used branch names to handle non-simultaneous
# upgrades to new model versions. Some chorus clients handled this by using
# the model version as the branch name. Changing the branch name triggers our
# model version upgrade logic. To avoid unnecessarily simulating an upgrade
# we will wrap and disable the check which forbids creating number only branches
# since Mercurial has to support them for backward compatibility in any case.

from mercurial import extensions, scmutil
import mercurial.ui as _ui

def uisetup(ui):
	extensions.wrapfunction(scmutil, "checknewlabel", checklabelwrapper)

def checklabelwrapper(orig, repo, lbl, kind):
	try:
		int(lbl)
		pass #let number only branches through without complaint
	except ValueError:
		orig(repo, lbl, kind) #let Mercurial test all other branch names
