# fixutf8.py - Make Mercurial compatible with non-utf8 locales
#
# Copyright 2009 Stefan Rusek
# Copyright 2015 SIL International
#
# This software may be used and distributed according to the terms
# of the GNU General Public License, incorporated herein by reference.
#
# To load the extension, add it to your .hgrc file:
#
# [extension]
#   fixutf8 =
#
# This module needs no special configuration.

'''
Fix incompatibilities with non-utf8 locales

No special configuration is needed.
'''

#
# How it works:
#
#  There are 2 ways for strings to get into HG, either
# via that command line or filesystem filename. We want
# to make sure that both of those work.
#
#  We use the WIN32 GetCommandLineW() to get the unicode
# version of the command line. And we wrapp all the
# places where we send or get filenames from the os and
# make sure we send UCS-16 to windows and convert back
# to UTF8.
#
#  There are bugs in Python that make print() and
# sys.stdout.write() barf on unicode or utf8 when the
# output codepage is set to 65001 (UTF8). So we do all
# outputing via WriteFile() with the code page set to
# 65001. The trick is to save the existing codepage,
# and restore it before we return back to python.
#
#  The result is that all of our strings are UTF8 all
# the time, and never explicitly converted to anything
# else.
#
import sys
from mercurial import extensions, scmutil
import mercurial.ui as _ui

# The version of Mercurial first released with chorus allowed branches to
# be named with just a number. We used branch names to handle non-simultaneous
# upgrades to new model versions. Some chorus clients handled this by using
# the model version as the branch name. Changing the branch name triggers our
# model version upgrade logic. To avoid unnecessarily simulating an upgrade
# we will wrap and disable the check which forbids creating number only branches
# since Mercurial has to support them for backward compatibility in any case.
# This number only branching needs to happen on all platforms so we execute it first.
# The remainder of this plug-in is needed only on windows and is not executed on Linux.
# We can't add an extension without breaking backward compatability so we have added the number branch here.
def allownumberbranches_uisetup():
	extensions.wrapfunction(scmutil, "checknewlabel", checklabelwrapper)

def checklabelwrapper(orig, repo, lbl, kind):
	try:
		int(lbl)
		pass #let number only branches through without complaint
	except ValueError:
		orig(repo, lbl, kind) #let Mercurial test all other branch names

# extsetup is only required on windows
def extsetup():
	if sys.platform == 'win32':
		winextsetup()

# uisetup on linux needs only to enable number only branches
def uisetup(ui):
	allownumberbranches_uisetup()
	# Handle the actual fixutf8 part of the extension if on windows
	if sys.platform == 'win32':
		winuisetup(ui)

if sys.platform == 'win32':
	import os, shutil

	from mercurial import demandimport

	demandimport.ignore.extend(["win32helper", "osutil"])

	try:
		import win32helper
		import osutil as pureosutil
	except ImportError:
		sys.path.append(os.path.dirname(__file__))
		import win32helper
		import osutil as pureosutil

	stdout = sys.stdout

	from mercurial import windows, util, osutil, dispatch, extensions, i18n, scmutil
	import mercurial.ui as _ui

	def mapconvert(convert, canconvert, doc):
		'''
		mapconvert(convert, canconvert, doc) ->
			(a -> a)

		Returns a function that converts arbitrary arguments
		using the specified conversion function.

		convert is a function to do actual convertions.
		canconvert returns true if the arg can be converted.
		doc is the doc string to attach to created function.

		The resulting function will return a converted list or
		tuple if passed a list or tuple.
		'''

		def _convert(arg):
			if canconvert(arg):
				return convert(arg)
			elif isinstance(arg, tuple):
				return tuple(map(_convert, arg))
			elif isinstance(arg, list):
				return map(_convert, arg)
			return arg

		_convert.__doc__ = doc
		return _convert

	tounicode1252 = mapconvert(
		lambda s: s.decode('cp1252', 'strict'),
		lambda s: isinstance(s, str),
		"Convert a CP1252 byte string to Unicode")

	tounicode = mapconvert(
		lambda s: s.decode('utf-8', 'strict'),
		lambda s: isinstance(s, str),
		"Convert a UTF-8 byte string to Unicode")

	fromunicode = mapconvert(
		lambda s: s.encode('utf-8', 'ignore'),
		lambda s: isinstance(s, unicode),
		"Convert a Unicode string to a UTF-8 byte string")

	win32helper.fromunicode = fromunicode

def test():
	if sys.platform != 'win32':
		return
	print win32helper.getargs()
	print sys.argv

	uargs = ['P:\\hg-fixutf8\\fixutf8.py', 'thi\xc5\x9b', 'i\xc5\x9b',
			 '\xc4\x85', 't\xc4\x99\xc5\x9bt']
	for s in uargs:
		win32helper.rawprint(win32helper.hStdOut, s + "\n")

def utf8wrapper(orig, *args, **kargs):
	try:
		return fromunicode(orig(*tounicode(args), **kargs))
	except UnicodeDecodeError:
		try:
			return fromunicode(orig(*tounicode1252(args), **tounicode1252(kargs)))
		except Exception, e:
			# print "utf8wrapper 1252 decode error: Exception: ", repr(e)
			raise
	except Exception, e:
		# print "utf8wrapper exception while calling %s" % orig.__name__
		raise

def winuisetup(ui):
	if sys.platform != 'win32' or not win32helper.consolehascp():
		return

	win32helper.uisetup(ui)

	try:
		from mercurial import encoding

		encoding.encoding = 'utf8'
	except ImportError:
		util._encoding = "utf-8"

	def localize(h):
		if hasattr(ui, '_buffers'):
			getbuffers = lambda ui: ui._buffers
		else:
			getbuffers = lambda ui: ui.buffers

		def f(orig, ui, *args, **kwds):
			if not getbuffers(ui):
				win32helper.rawprint(h, ''.join(args))
			else:
				orig(ui, *args, **kwds)

		return f

	extensions.wrapfunction(_ui.ui, "write", localize(win32helper.hStdOut))
	extensions.wrapfunction(_ui.ui, "write_err", localize(win32helper.hStdErr))

def winextsetup():
	oldlistdir = osutil.listdir

	osutil.listdir = pureosutil.listdir # force pure listdir
	extensions.wrapfunction(osutil, "listdir", utf8wrapper)

	def normcase_utf8(path):
		try:
			return fromunicode(tounicode(path)).upper()
		except UnicodeDecodeError:
			try:
				return fromunicode(tounicode1252(path)).upper()
			except Exception, e:
				print "utf8wrapper 1252 decode error: Exception: ", repr(e)
				raise
		except Exception, e:
			print "utf8wrapper exception while calling %s" % orig.__name__
			raise

	windows.normcase = normcase_utf8
	util.normcase = normcase_utf8

	# only get the real command line args if we are passed a real ui object
	def disp_parse(orig, ui, args):
		if type(ui) == _ui.ui:
			args = win32helper.getUtf8NonConfigArgs()[-len(args):]
		return orig(ui, args)

	extensions.wrapfunction(dispatch, "_parse", disp_parse)

	class posixfile_utf8(file):
		def __init__(self, name, mode='rb'):
			super(posixfile_utf8, self).__init__(tounicode(name), mode)

	util.posixfile = posixfile_utf8

	if util.atomictempfile:
		class atomictempfile_utf8(posixfile_utf8):
			"""file-like object that atomically updates a file

			All writes will be redirected to a temporary copy of the original
			file.  When rename is called, the copy is renamed to the original
			name, making the changes visible.
			"""

			def __init__(self, name, mode, createmode=None):
				self.__name = name
				self.temp = util.mktempcopy(name, emptyok=('w' in mode),
											createmode=createmode)
				posixfile_utf8.__init__(self, self.temp, mode)

			def close(self):
				if not self.closed:
					posixfile_utf8.close(self)
					util.rename(self.temp, util.localpath(self.__name))

			def __del__(self):
				if not self.closed:
					try:
						os.unlink(self.temp)
					except: pass
					posixfile_utf8.close(self)

		util.atomictempfile = atomictempfile_utf8

	# wrap the os and path functions
	def wrapnames(mod, *names):
		for name in names:
			if hasattr(mod, name):
				extensions.wrapfunction(mod, name, utf8wrapper)

	wrapnames(os.path, 'normpath', 'normcase', 'islink', 'dirname',
			  'isdir', 'isfile', 'exists', 'abspath', 'realpath', 'split')
	wrapnames(os, 'makedirs', 'lstat', 'unlink', 'chmod', 'stat',
			  'mkdir', 'rename', 'removedirs', 'setcwd', 'open',
			  'listdir', 'chdir', 'remove', 'access', 'rmdir', 'tempnam', 'utime' )
	wrapnames(shutil, 'copyfile', 'copymode', 'copystat')
	extensions.wrapfunction(os, 'getcwd', win32helper.getcwdwrapper)
	wrapnames(sys.modules['__builtin__'], 'open')

	def system_call(orig, cmd, environ={}, cwd=None, onerr=None, errprefix=None, out=None):
		return win32helper.system(cmd)

	extensions.wrapfunction(util, 'system', system_call)

if __name__ == "__main__":
	test()
