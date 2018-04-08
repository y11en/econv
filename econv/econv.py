#-*- encoding: utf-8 -*-

import clr
clr.AddReferenceToFileAndPath('EProjectFile.dll')
from EProjectFile import *

from sys import stderr, exit
from collections import OrderedDict
from table import SingleTable, merge_tables
from wcwidth import wcswidth


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
                '系统信息段': ESystemInfo.Parse,
                '用户信息段': ProjectConfigInfo.Parse,
            }

            sections = OrderedDict()

            while not project.IsFinish():
                section = project.ReadSection()
                sections[section.SectionName] = parsers.get(section.SectionName, lambda x: x)(section)

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
    return SingleTable([[
        '易语言版本: %s' % section.ESystemVersion,
        '语言: %s' % LANGUAGES[section.Language],
        '格式版本: %s' % section.EProjectFormatVersion,
        '文件类型: %s' % FILE_TYPES[section.FileType],
        '项目类型: %s' % PROJECT_TYPES[section.ProjectType],
    ]], "基本信息").table


def gen_user_info(sections):
    section = sections['用户信息段']

    tables = [
        SingleTable([
            ['*程序名称', section.Name, '*程序版本', str(section.Version)]
        ]),
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
        ])
    ]

    for t in tables:
        t.inner_row_border = True
        t.justify_columns[0] = 'right'

    table0_col1_width = tables[0].column_widths[1]
    table2_col1_width = tables[2].column_widths[1]
    col1_max_width = max(table0_col1_width, table2_col1_width)
    if table0_col1_width < col1_max_width:
        tables[0].table_data[0][1] += ' ' * (col1_max_width - wcswidth(tables[0].table_data[0][1]))
    else:
        tables[2].table_data[0][1] += ' ' * (col1_max_width - wcswidth(tables[2].table_data[0][1]))

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