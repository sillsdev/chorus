#
# Unicode WIN32 api calls
#
# Copyright 2015 Jason Naylor
#
# This software may be used and distributed according to the terms
# of the GNU General Public License, incorporated herein by reference.
#
# Portions of this file were originally licensed as follows:
#
#  Copyright (C) 2010-2011  The IPython Development Team
#
#  Distributed under the terms of the 3-Clause BSD
#

import sys
from ctypes import *
from ctypes.wintypes import *

# stdlib
import os, sys, threading
import ctypes, msvcrt

usecpmap = True
mapcp = None

# Using ctypes we can call the unicode versions of win32 api calls that
# python does not call.
if sys.platform == "win32" and windll:
	LPDWORD = POINTER(DWORD)
	LPHANDLE = POINTER(HANDLE)
	ULONG_PTR = POINTER(ULONG)

	class SECURITY_ATTRIBUTES(ctypes.Structure):
		_fields_ = [("nLength", DWORD),
					("lpSecurityDescriptor", LPVOID),
					("bInheritHandle", BOOL)]

	LPSECURITY_ATTRIBUTES = POINTER(SECURITY_ATTRIBUTES)

	class STARTUPINFO(ctypes.Structure):
		_fields_ = [("cb", DWORD),
					("lpReserved", LPCWSTR),
					("lpDesktop", LPCWSTR),
					("lpTitle", LPCWSTR),
					("dwX", DWORD),
					("dwY", DWORD),
					("dwXSize", DWORD),
					("dwYSize", DWORD),
					("dwXCountChars", DWORD),
					("dwYCountChars", DWORD),
					("dwFillAttribute", DWORD),
					("dwFlags", DWORD),
					("wShowWindow", WORD),
					("cbReserved2", WORD),
					("lpReserved2", LPVOID),
					("hStdInput", HANDLE),
					("hStdOutput", HANDLE),
					("hStdError", HANDLE)]

	LPSTARTUPINFO = POINTER(STARTUPINFO)

	class PROCESS_INFORMATION(ctypes.Structure):
		_fields_ = [("hProcess", HANDLE),
					("hThread", HANDLE),
					("dwProcessId", DWORD),
					("dwThreadId", DWORD)]

	LPPROCESS_INFORMATION = POINTER(PROCESS_INFORMATION)

	LPWSTR = c_wchar_p
	LPCWSTR = c_wchar_p
	LPCSTR = c_char_p
	INT = c_int
	UINT = c_uint
	BOOL = INT
	DWORD = UINT
	HANDLE = c_void_p

	# Win32 API constants needed
	ERROR_HANDLE_EOF = 38
	ERROR_BROKEN_PIPE = 109
	ERROR_NO_DATA = 232
	HANDLE_FLAG_INHERIT = 0x0001
	STARTF_USESTDHANDLES = 0x0100
	CREATE_SUSPENDED = 0x0004
	CREATE_NEW_CONSOLE = 0x0010
	CREATE_NO_WINDOW = 0x08000000
	STILL_ACTIVE = 259
	WAIT_TIMEOUT = 0x0102
	WAIT_FAILED = 0xFFFFFFFF
	INFINITE = 0xFFFFFFFF
	DUPLICATE_SAME_ACCESS = 0x00000002
	ENABLE_ECHO_INPUT = 0x0004
	ENABLE_LINE_INPUT = 0x0002
	ENABLE_PROCESSED_INPUT = 0x0001

	# Win32 API functions needed
	GetLastError = ctypes.windll.kernel32.GetLastError
	GetLastError.argtypes = []
	GetLastError.restype = DWORD

	CreateFile = ctypes.windll.kernel32.CreateFileW
	CreateFile.argtypes = [LPCWSTR, DWORD, DWORD, LPVOID, DWORD, DWORD, HANDLE]
	CreateFile.restype = HANDLE

	CreatePipe = ctypes.windll.kernel32.CreatePipe
	CreatePipe.argtypes = [POINTER(HANDLE), POINTER(HANDLE),
						   LPSECURITY_ATTRIBUTES, DWORD]
	CreatePipe.restype = BOOL

	CreateProcess = ctypes.windll.kernel32.CreateProcessW
	CreateProcess.argtypes = [LPCWSTR, LPCWSTR, LPSECURITY_ATTRIBUTES,
							  LPSECURITY_ATTRIBUTES, BOOL, DWORD, LPVOID, LPCWSTR, LPSTARTUPINFO,
							  LPPROCESS_INFORMATION]
	CreateProcess.restype = BOOL

	GetExitCodeProcess = ctypes.windll.kernel32.GetExitCodeProcess
	GetExitCodeProcess.argtypes = [HANDLE, LPDWORD]
	GetExitCodeProcess.restype = BOOL

	GetCurrentProcess = ctypes.windll.kernel32.GetCurrentProcess
	GetCurrentProcess.argtypes = []
	GetCurrentProcess.restype = HANDLE

	ResumeThread = ctypes.windll.kernel32.ResumeThread
	ResumeThread.argtypes = [HANDLE]
	ResumeThread.restype = DWORD

	ReadFile = ctypes.windll.kernel32.ReadFile
	ReadFile.argtypes = [HANDLE, LPVOID, DWORD, LPDWORD, LPVOID]
	ReadFile.restype = BOOL

	WriteFile = ctypes.windll.kernel32.WriteFile
	WriteFile.argtypes = [HANDLE, LPVOID, DWORD, LPDWORD, LPVOID]
	WriteFile.restype = BOOL

	GetConsoleMode = ctypes.windll.kernel32.GetConsoleMode
	GetConsoleMode.argtypes = [HANDLE, LPDWORD]
	GetConsoleMode.restype = BOOL

	SetConsoleMode = ctypes.windll.kernel32.SetConsoleMode
	SetConsoleMode.argtypes = [HANDLE, DWORD]
	SetConsoleMode.restype = BOOL

	FlushConsoleInputBuffer = ctypes.windll.kernel32.FlushConsoleInputBuffer
	FlushConsoleInputBuffer.argtypes = [HANDLE]
	FlushConsoleInputBuffer.restype = BOOL

	WaitForSingleObject = ctypes.windll.kernel32.WaitForSingleObject
	WaitForSingleObject.argtypes = [HANDLE, DWORD]
	WaitForSingleObject.restype = DWORD

	DuplicateHandle = ctypes.windll.kernel32.DuplicateHandle
	DuplicateHandle.argtypes = [HANDLE, HANDLE, HANDLE, LPHANDLE,
								DWORD, BOOL, DWORD]
	DuplicateHandle.restype = BOOL

	SetHandleInformation = ctypes.windll.kernel32.SetHandleInformation
	SetHandleInformation.argtypes = [HANDLE, DWORD, DWORD]
	SetHandleInformation.restype = BOOL

	CloseHandle = ctypes.windll.kernel32.CloseHandle
	CloseHandle.argtypes = [HANDLE]
	CloseHandle.restype = BOOL

	CommandLineToArgvW = ctypes.windll.shell32.CommandLineToArgvW
	CommandLineToArgvW.argtypes = [LPCWSTR, POINTER(ctypes.c_int)]
	CommandLineToArgvW.restype = POINTER(LPCWSTR)

	LocalFree = ctypes.windll.kernel32.LocalFree
	LocalFree.argtypes = [HLOCAL]
	LocalFree.restype = HLOCAL

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
		InternalWriteFile(h, s)

	def getUtf8NonConfigArgs():
		'''
		getargs() -> [args]

		Returns an array of utf8 encoded arguments passed on the command line.
		
		Skips any --config argument pairs since this is used in a method where
		those arguments are already removed
		'''
		c = INT(0)
		pargv = CommandLineToArgv(GetCommandLine(), byref(c))
		cleanArguments = []
		iterator = iter(xrange(1, c.value))
		for i in iterator:
			if pargv[i] != "--config":
				cleanArguments.append(fromunicode(pargv[i]))
			else:
				iterator.next() # skip appending the --config and whatever argument followed it
		return cleanArguments

	def system_call(orig, cmd, environ={}, cwd=None, onerr=None, errprefix=None, out=None):
		# Overridden to handle the call out to the system merge program, all other parameters
		# are irrelevant
		system(cmd)

	# This class handles all the ugly win32 api required to make a system call with non-ascii args
	class Win32ShellCommandController(object):
		#Runs a shell command in a 'with' context.

		def __init__(self, cmd, mergeout=False):
			"""Initializes the shell command controller.
			The cmd is the program to execute, and mergeout is
			whether to blend stdout and stderr into one output
			in stdout. Merging them together in this fashion more
			reliably keeps stdout and stderr in the correct order
			especially for interactive shell usage.
			"""
			self.cmd = cmd
			self.mergeout = mergeout

		def __enter__(self):
			cmd = self.cmd
			mergeout = self.mergeout

			self.hstdout, self.hstderr = None, None
			self.piProcInfo = None
			try:
				p_hstdout, c_hstdout, p_hstderr, \
				c_hstderr = [None] * 4

				# SECURITY_ATTRIBUTES with inherit handle set to True
				saAttr = SECURITY_ATTRIBUTES()
				saAttr.nLength = ctypes.sizeof(saAttr)
				saAttr.bInheritHandle = True
				saAttr.lpSecurityDescriptor = None

				def create_pipe(uninherit):
					"""Creates a Windows pipe, which consists of two handles.
					The 'uninherit' parameter controls which handle is not
					inherited by the child process.
					"""
					handles = HANDLE(), HANDLE()
					if not CreatePipe(ctypes.byref(handles[0]),
									  ctypes.byref(handles[1]), ctypes.byref(saAttr), 0):
						raise ctypes.WinError()
					if not SetHandleInformation(handles[uninherit],
												HANDLE_FLAG_INHERIT, 0):
						raise ctypes.WinError()
					return handles[0].value, handles[1].value

				p_hstdout, c_hstdout = create_pipe(uninherit=0)
				# 'mergeout' signals that stdout and stderr should be merged.
				# We do that by using one pipe for both of them.
				if mergeout:
					c_hstderr = HANDLE()
					if not DuplicateHandle(GetCurrentProcess(), c_hstdout,
										   GetCurrentProcess(), ctypes.byref(c_hstderr),
										   0, True, DUPLICATE_SAME_ACCESS):
						raise ctypes.WinError()
				else:
					p_hstderr, c_hstderr = create_pipe(uninherit=0)

				# Create the process object
				piProcInfo = PROCESS_INFORMATION()
				siStartInfo = STARTUPINFO()
				siStartInfo.cb = ctypes.sizeof(siStartInfo)
				siStartInfo.hStdOutput = c_hstdout
				siStartInfo.hStdError = c_hstderr
				siStartInfo.dwFlags = STARTF_USESTDHANDLES
				dwCreationFlags = CREATE_SUSPENDED | CREATE_NO_WINDOW  # | CREATE_NEW_CONSOLE

				if not CreateProcess(None,
									 cmd,
									 None, None, True, dwCreationFlags,
									 None, None, ctypes.byref(siStartInfo),
									 ctypes.byref(piProcInfo)):
					raise ctypes.WinError()

				# Close this process's versions of the child handles
				CloseHandle(c_hstdout)
				c_hstdout = None
				if c_hstderr is not None:
					CloseHandle(c_hstderr)
					c_hstderr = None

				# Transfer ownership of the parent handles to the object
				self.hstdout = p_hstdout
				p_hstdout = None
				if not mergeout:
					self.hstderr = p_hstderr
					p_hstderr = None
				self.piProcInfo = piProcInfo

			finally:
				if p_hstdout:
					CloseHandle(p_hstdout)
				if c_hstdout:
					CloseHandle(c_hstdout)
				if p_hstderr:
					CloseHandle(p_hstderr)
				if c_hstderr:
					CloseHandle(c_hstderr)

			return self


		def _stdout_thread(self, handle, func):
			# Allocate the output buffer
			data = ctypes.create_string_buffer(4096)
			while True:
				bytesRead = DWORD(0)
				if not ReadFile(handle, data, 4096,
								ctypes.byref(bytesRead), None):
					le = GetLastError()
					if le == ERROR_BROKEN_PIPE:
						return
					else:
						raise ctypes.WinError()
				# FIXME: Python3
				s = data.value[0:bytesRead.value]
				# print("\nv: %s" % repr(s), file=sys.stderr)
				func(fromunicode(s))

		def run(self, stdout_func=None, stderr_func=None):
			"""Runs the process, using the provided functions for I/O.
			The functions stdout_func and stderr_func are called whenever
			something is printed to stdout or stderr, respectively.
			These functions are called from different threads but because
			they contain code that must be interpreted they will not run
			concurrently because of the GIL.
			"""
			if stdout_func is None and stderr_func is None:
				return self._run_stdio()

			if stderr_func is not None and self.mergeout:
				raise RuntimeError("Shell command was initiated with "
								   "merged stdout, but a separate stderr_func "
								   "was provided to the run() method")

			# Create a thread for each input/output handle
			threads = []
			threads.append(threading.Thread(target=self._stdout_thread,
											args=(self.hstdout, stdout_func)))
			if not self.mergeout:
				if stderr_func is None:
					stderr_func = stdout_func
				threads.append(threading.Thread(target=self._stdout_thread,
												args=(self.hstderr, stderr_func)))
			# Start the I/O threads and the process
			if ResumeThread(self.piProcInfo.hThread) == 0xFFFFFFFF:
				raise ctypes.WinError()
			for thread in threads:
				thread.start()
			# Wait for the process to complete
			if WaitForSingleObject(self.piProcInfo.hProcess, INFINITE) == \
					WAIT_FAILED:
				raise ctypes.WinError()
			# Wait for the I/O threads to complete
			for thread in threads:
				thread.join()

			exitCode = DWORD()
			GetExitCodeProcess(self.piProcInfo.hProcess, ctypes.byref(exitCode))
			return exitCode

		def _stdout_raw(self, s):
			"""Writes the string to stdout"""
			print s
			sys.stdout.flush()

		def _stderr_raw(self, s):
			"""Writes the string to stdout"""
			sys.stderr.write(s)
			sys.stderr.flush()

		def _run_stdio(self):
			"""Runs the process using the system standard I/O.
			IMPORTANT: stdin needs to be asynchronous, so the Python
					   sys.stdin object is not used. Instead,
					   msvcrt.kbhit/getwch are used asynchronously.
			"""

			if self.mergeout:
				return self.run(stdout_func=self._stdout_raw)
			else:
				return self.run(stdout_func=self._stdout_raw,
								stderr_func=self._stderr_raw)


		def __exit__(self, exc_type, exc_value, traceback):
			if self.hstdout:
				CloseHandle(self.hstdout)
				self.hstdout = None
			if self.hstderr:
				CloseHandle(self.hstderr)
				self.hstderr = None
			if self.piProcInfo is not None:
				CloseHandle(self.piProcInfo.hProcess)
				CloseHandle(self.piProcInfo.hThread)
				self.piProcInfo = None


	def system(cmd):
		with Win32ShellCommandController(cmd) as scc:
			retval = scc.run()
			return retval.value
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
