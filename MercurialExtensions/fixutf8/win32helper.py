#
# Unicode WIN32 api calls
#

import sys
from ctypes import *

usecpmap = True
mapcp = None

# Using ctypes we can call the unicode versions of win32 api calls that
# python does not call.
if sys.platform == "win32" and windll:
	LPWSTR = c_wchar_p
	LPCWSTR = c_wchar_p
	LPCSTR = c_char_p
	INT = c_int
	UINT = c_uint
	BOOL = INT
	DWORD = UINT
	HANDLE = c_void_p

	prototype = WINFUNCTYPE(LPCWSTR)
	GetCommandLine = prototype(("GetCommandLineW", windll.kernel32))

	prototype = WINFUNCTYPE(POINTER(LPCWSTR), LPCWSTR, POINTER(INT))
	CommandLineToArgv = prototype(("CommandLineToArgvW", windll.shell32))

	prototype = WINFUNCTYPE(BOOL, UINT)
	SetConsoleOutputCP = prototype(("SetConsoleOutputCP", windll.kernel32))

	prototype = WINFUNCTYPE(UINT)
	GetConsoleOutputCP = prototype(("GetConsoleOutputCP", windll.kernel32))

	prototype = WINFUNCTYPE(INT)
	GetLastError = prototype(("GetLastError", windll.kernel32))

	prototype = WINFUNCTYPE(HANDLE, DWORD)
	GetStdHandle = prototype(("GetStdHandle", windll.kernel32))

	prototype = WINFUNCTYPE(BOOL, HANDLE, LPCSTR, DWORD,
			POINTER(DWORD), DWORD)
	WriteFile = prototype(("WriteFile", windll.kernel32))

	prototype = WINFUNCTYPE(DWORD, DWORD, LPWSTR)
	GetCurrentDirectory = prototype(("GetCurrentDirectoryW", windll.kernel32))

	hStdOut = GetStdHandle(0xFFFFfff5)
	hStdErr = GetStdHandle(0xFFFFfff4)

	def getcwdwrapper(orig):
		chars = GetCurrentDirectory(0, None) + 1
		p = create_unicode_buffer(chars)
		if 0 == GetCurrentDirectory(chars, p):
			err = GetLastError()
			if err < 0:
				raise pywintypes.error(err, "GetCurrentDirectory",
						win32api.FormatMessage(err))
		return fromunicode(p.value)

	def InternalWriteFile(h, s):
		limit = 0x4000
		l = len(s)
		start = 0
		while start < l:
			end = start + limit
			buffer = s[start:end]
			c = DWORD(0)
			if not WriteFile(h, buffer, len(buffer), byref(c), 0):
				err = GetLastError()
				if err < 0:
					raise pywintypes.error(err, "WriteFile",
							win32api.FormatMessage(err))
				start = start + c.value + 1
			else:
				start = start + len(buffer)

	def consolehascp():
		return 0 != GetConsoleOutputCP()

	def rawprint(h, s):
		#try:
			#changedcp, oldcp = False, GetConsoleOutputCP()
			#u = s ;#.decode('utf-8') - since TortoiseHG 1.8.4 it is utf8 already. This was not true for 1.8.2
			#try:
			#    if oldcp != 65001:
			#        s = u.encode('cp%d' % oldcp)
			#except UnicodeError:
			#    if usecpmap:
			#        cpname, newcp = mapcp(u)
			#        # s is already in utf8
			#        if newcp != 65001:
			#			try:
			#				s = u.encode(cpname)
			#			except UnicodeError:
			#				s = u.decode('utf-8','ignore').encode(cpname,'ignore')
			#				sys.stderr.write('failed to convert: ' + s + '\n');
			#    else:
			#        newcp = 65001
			#    changedcp = SetConsoleOutputCP(newcp)

			InternalWriteFile(h, s)
		#finally:
		#    if changedcp:
		#        SetConsoleOutputCP(oldcp)

	def getargs():
		'''
		getargs() -> [args]

		Returns an array of utf8 encoded arguments passed on the command line.
		'''
		c = INT(0)
		pargv = CommandLineToArgv(GetCommandLine(), byref(c))
		return [fromunicode(pargv[i]) for i in xrange(1, c.value)]
else:
	win32rawprint = False
	win32getargs = False
	hStdOut = 0
	hStdErr = 0

def uisetup(ui):
	global usecpmap, mapcp
	usecpmap = ui.config('fixutf8', 'usecpmap', usecpmap)
	if usecpmap:
		import cpmap
		mapcp = cpmap.reduce
