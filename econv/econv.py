#-*- encoding: utf-8 -*-

import clr
clr.AddReferenceToFileAndPath('EProjectFile.dll')
from EProjectFile import *

from sys import stderr, exit
from collections import OrderedDict

from table import SingleTable


ERROR_SUCC = 0
ERROR_ARGV = 1
ERROR_CONV = 2

LANGUAGES = { 1: '汉语' }
FILE_TYPES = { 1: '源码', 3: '模块' }
PROJECT_TYPES = {
    0: 'Windows窗口程序', 1: 'Windows控制台程序', 2: 'Windows动态链接库', 1000: 'Windows易语言模块',
    10000: 'Linux控制台程序', 11000: 'Linux易语言模块'
}


def load(filename, password):
    try:

        with ProjectFileReader(filename, password) as project:
            parsers = {
                '系统信息段': ESystemInfo.Parse
            }

            sections = OrderedDict()

            while not project.IsFinish():
                section = project.ReadSection()
                sections[section.Name] = parsers.get(section.Name, lambda x: x)(section.Data)

            return sections

    except Exception as ex:
        print >> stderr, ex
        exit(ERROR_CONV)


def convert(sections):
    return gen_base_nfo(sections)


def gen_base_nfo(sections):
    section = sections['系统信息段']
    return SingleTable([[
        '易语言版本: %s' % section.ESystemVersion,
        '语言: %s' % LANGUAGES[section.Language],
        '格式版本: %s' % section.EProjectFormatVersion,
        '文件类型: %s' % FILE_TYPES[section.FileType],
        '项目类型: %s' % PROJECT_TYPES[section.ProjectType],
    ]], "基本信息").table


def main(args, argv):
    if args < 2:
        print >> stderr, "用法: %s 易语言文件 [密码]" % argv[0]
        exit(ERROR_ARGV)

    filename = argv[1]
    password = argv[2] if args > 2 else None

    sections = load(filename, password)
    print convert(sections)


if __name__ == '__main__':
    from sys import argv
    main(len(argv), argv)