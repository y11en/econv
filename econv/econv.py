#-*- encoding: utf-8 -*-

import clr
clr.AddReferenceToFileAndPath('EProjectFile.dll')
from EProjectFile import *

from sys import stderr, exit
from collections import OrderedDict
from table import SingleTable, merge_tables, adjust_tables
from wcwidth import wcswidth
from hashlib import md5


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
                '程序段': CodeSectionInfo.Parse,
                '易包信息段1': EPackageInfo.Parse,
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
        gen_libs_info(sections),
        gen_global_variable(sections),
        gen_class_data(sections),
        gen_unknown_section(sections),
    ))


def gen_base_info(sections):
    section = sections['系统信息段']
    
    table = SingleTable([
        [
            '易语言版本', str(section.ESystemVersion), '格式版本',
            '%s-%s'%(section.EProjectFormatVersion, section.Language)
        ],
        ['  文件类型', FILE_TYPES[section.FileType], '项目类型', PROJECT_TYPES[section.ProjectType]]
    ])

    return table.table


def hash_data(data, hex=False):
    return '%08x-%s' % (len(data), md5('data').hexdigest())


def gen_user_info(sections):
    section = sections['用户信息段']
    cmd = sections['程序段'].DebugCommandParameters
    icon = hash_data(sections['程序段'].IconData)

    tables = [
        SingleTable([
            ['*程序名称', section.Name, '*程序版本', str(section.Version)]
        ]),
        SingleTable([
            [' 编译插件', section.CompilePlugins],
            [' 调试参数', cmd],
            [' 程序图标', icon]
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
        table.justify_columns[0] = 'right'

    tables[2].justify_columns[2] = 'right'

    adjust_tables(1, tables[0], tables[2])

    return merge_tables(*tables)


def get_global_variable_info(var):
    return [
        var.Name,
        var.TypeName,
        ','.join(map(lambda i: str(i), var.UBound)),
        '  x' if var.Flags else '',
        var.Comment
    ]


def gen_global_variable(sections):
    section = sections['程序段']
    variables = section.GlobalVariables
    if len(variables) == 0: return ''

    data = [ get_global_variable_info(var) for var in variables ]
    data = [['全局变量名', '类型', '数组', '公开', '备注']] + data
    
    table = SingleTable(data)
    
    return table.table


def tab(text, n):
    return '\n'.join(map(lambda l: ' '*n+l if l else l, text.split('\n')))


def get_local_variable_info(var):
    return [
        var.Name,
        var.TypeName,
        '  x' if var.Flags else '',
        ','.join(map(lambda i: str(i), var.UBound)),
        var.Comment
    ]

def gen_local_variable(method):
    variables = method.Variables
    if len(variables) == 0: return ''

    data = [ get_local_variable_info(var) for var in variables ]
    data = [['变量名', '类型', '静态', '数组', '备注']] + data
    
    table = SingleTable(data)
    
    return table.table


def gen_method(title, section, cls, methods):
    result = ''
    for idx in cls.Method:
        method, epkg = methods[idx]
        data = [
            [title, '返回值类型', '公开', '易包', '备注'],
            [
                '%s::%s'%(cls.Name,method.Name),
                '',
                'x' if method.Flags else '',
                epkg if epkg else '',
                method.Comment
            ]
        ]
        result += SingleTable(data).table + '\n'
        local_var = gen_local_variable(method)
        if local_var: result +=  local_var + "\n"
        result += tab(method.Code, 2) + '\n'

    return result


def get_class_variable_info(var):
    return [
        var.Name,
        var.TypeName,
        ','.join(map(lambda i: str(i), var.UBound)),
        var.Comment
    ]


def gen_class_data(sections):
    section = sections['程序段']
    epkgs = sections['易包信息段1'].FileNames

    methods = dict(map(lambda (i,m): (m.Id, (m, epkgs[i])), enumerate(section.Methods)))
    classes = dict(map(lambda c: (c.Id, c), section.Classes))

    result = ''
    for cls in section.Classes:
        data = []
        if cls.BaseClass == 0:
            data.append(['程序集名', '保留', '保留', '备注'])
            data.append([cls.Name, '', '', cls.Comment])
            if cls.Variables:
                data.append(['变量名', '类型', '数组', '备注'])

            title = '子程序名'
        else:
            data.append(['类名', '基类', '公开', '备注'])
            base = classes.get(cls.BaseClass)
            data.append([
                cls.Name,
                base.Name if base else '',
                '',
                cls.Comment
            ])
            if cls.Variables:
                data.append(['私有成员名', '类型', '数组', '备注'])
            title = '方法名'

        data += [ get_class_variable_info(var) for var in cls.Variables ]
        result += SingleTable(data).table + '\n'
        result += gen_method(title, section, cls, methods) + '\n'

    return result


def gen_libs_info(sections):
    libs = sections['程序段'].Libraries
    data = [['支持库名', '文件', '版本', '数字签名']]
    data += [ (lib.Name, lib.FileName, lib.Version, lib.GuidString) for lib in libs]
    return SingleTable(data).table


def hex_key(key):
    v = 0
    for i in key:
        v = v * 256 + i
    return hex(v)


def gen_unknown_section(sections):
    data = [['未知段名', 'Key', 'Flags', '数据签名']]
    data += [ (s.SectionName if s.SectionName else '(null)', hex_key(s.Key), s.Flags, hash_data(s.Data))
        for s in filter(lambda s: isinstance(s, SectionInfo), sections.values()) ]
        
    return SingleTable(data).table


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