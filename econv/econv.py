#-*- encoding: utf-8 -*-

import clr
clr.AddReferenceToFileAndPath('EProjectFile.dll')
from EProjectFile import *

from sys import argv, stderr, exit


ERROR_SUCC = 0
ERROR_ARGV = 1
ERROR_CONV = 2


args = len(argv)
if args < 2:
    print >> stderr, "用法: %s 易语言文件 [密码]" % argv[0]
    exit(ERROR_ARGV)

filename = argv[1]
password = argv[2] if args > 2 else None

try:

    with ProjectFileReader(filename, password) as project:
        while not project.IsFinish():
            section = project.ReadSection()
            print(section.Name)

except Exception as ex:
    print >> stderr, ex
    exit(ERROR_CONV)