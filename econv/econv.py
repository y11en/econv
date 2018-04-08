#-*- encoding: utf-8 -*-

import clr
clr.AddReferenceToFileAndPath('EProjectFile.dll')
from EProjectFile import *

from sys import stderr, exit
from collections import OrderedDict
from table import SingleTable, merge_tables, adjust_tables
from wcwidth import wcswidth


ERROR_SUCC = 0
ERROR_ARGV = 1
ERROR_CONV = 2

FILE_TYPES = { 1: '源码', 3: '模块' }
PROJECT_TYPES = {
    0: 'Windows窗口程序', 1: 'Windows控制台程序', 2: 'Windows动态链接库', 1000: 'Windows易语言模块',
    10000: 'Linux控制台程序', 11000: 'Linux易语言模块'
}


def load(filename, password):
    try:

        with ProjectFileReader(filename, password) as project:
            parsers = {
                '系统信息段': ESystemInfo.Parse,
                '用户信息段': ProjectConfigInfo.Parse,
                '程序段': CodeSectionInfo.Parse
            }

            sections = OrderedDict()

            while not project.IsFinish():
                section = project.ReadSection()
                parser = parsers.get(section.SectionName, lambda x, y: x)
                sections[section.SectionName] = parser(section, project.CryptEc)

            return sections

    except Exception as ex:
        print >> stderr, ex
        exit(ERROR_CONV)


def convert(sections):
    return '\n'.join((
        gen_base_info(sections),
        gen_user_info(sections),
    ))


def gen_base_info(sections):
    section = sections['系统信息段']
    
    table=SingleTable([
        [
            '易语言版本', str(section.ESystemVersion), '格式版本',
            '%s-%s'%(section.EProjectFormatVersion, section.Language)
        ],
        ['  文件类型', FILE_TYPES[section.FileType], '项目类型', PROJECT_TYPES[section.ProjectType]]
    ], '基本信息')

    return table.table


def gen_user_info(sections):
    section = sections['用户信息段']

    tables = [
        SingleTable([
            ['*程序名称', section.Name, '*程序版本', str(section.Version)]
        ], '用户信息'),
        SingleTable([
            [' 编译插件', section.CompilePlugins]
        ]),
        SingleTable([
            ['*作者', section.Author, ' 电子信箱', section.Email],
            [' 联系地址', section.Address, '邮政编码', section.ZipCode],
            ['电话', section.TelephoneNumber, '传真', section.FaxNumber],
        ]),
        SingleTable([
            [' 主页地址', section.Homepage],
            [' 版权声明', section.CopyrightNotice],
            ['*程序描述', section.Description],
        ]),
        SingleTable([
            [' [ ]' if section.NotWriteVersion else '[x]', '在编译DLL时允许输出别公开类的公开方法'],
            [' [x]' if section.ExportPublicClassMethod else '[ ]', '将此程序带星号项同时写入编译后的可执行文件版本信息中']
        ])
    ]

    for table in tables:
        table.inner_row_border = True
        table.justify_columns[0] = 'right'

    tables[2].justify_columns[2] = 'right'

    adjust_tables(1, tables[0], tables[2])

    return merge_tables(*tables)


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