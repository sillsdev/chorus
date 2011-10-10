
minchar = 0
maxchar = 0xffff

cps = [ 437, # prefer ASCII
		1252, 1250, 1251, 1253, 1254, 1255, 1256,
		1257, 1258,  874,  932,  936,  949,  950,
		1361,  869,  866,  865,  864,  863,  862,
		861,   860,  857,  855,  852,  775,  737,
		850,   437,
		65001] # fallback on utf-8

def canencode(c, cp):
	if cp == 'cp65001':
		return True
	try:
		c.encode(cp)
		return True
	except UnicodeError:
		return False

scps = ['cp%d' % cp for cp in cps]
chars = [unichr(i) for i in range(minchar, 1 + maxchar)]

f = open('cpmap.py', 'w')

f.write('''
####################################################
#
# Do not modify this file, edit buildcpmap.py
#
####################################################
''')
f.write("cps = %s\n" % repr(scps))
f.write("cpmap = %s\n" % dict(('cp%d' % cp, cp) for cp in cps))

f.write("charmap = [\n")
for c, lcp in ((char, [cp for cp in scps if canencode(char, cp)])
		for char in chars):
	f.write("    %s,\n" % repr(lcp))
f.write(''']

def reduce(s):
	l = list(cps)
	for c in s:
		l = [cp for cp in charmap[ord(c)] if cp in l]
	return (l[0], cpmap[l[0]])
''');
f.close()
