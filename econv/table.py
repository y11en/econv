#-*- encoding: utf-8 -*-

import terminaltables
from terminaltables.width_and_alignment import max_dimensions

class SingleTable(terminaltables.SingleTable):
    CHAR_F_INNER_HORIZONTAL = '─'
    CHAR_F_INNER_INTERSECT = '┼'
    CHAR_F_INNER_VERTICAL = '│'
    CHAR_F_OUTER_LEFT_INTERSECT = '├'
    CHAR_F_OUTER_LEFT_VERTICAL = '│'
    CHAR_F_OUTER_RIGHT_INTERSECT = '┤'
    CHAR_F_OUTER_RIGHT_VERTICAL = '│'
    CHAR_H_INNER_HORIZONTAL = '─'
    CHAR_H_INNER_INTERSECT = '┼'
    CHAR_H_INNER_VERTICAL = '│'
    CHAR_H_OUTER_LEFT_INTERSECT = '├'
    CHAR_H_OUTER_LEFT_VERTICAL = '│'
    CHAR_H_OUTER_RIGHT_INTERSECT = '┤'
    CHAR_H_OUTER_RIGHT_VERTICAL = '│'
    CHAR_INNER_HORIZONTAL = '─'
    CHAR_INNER_INTERSECT = '┼'
    CHAR_INNER_VERTICAL = '│'
    CHAR_OUTER_BOTTOM_HORIZONTAL = '─'
    CHAR_OUTER_BOTTOM_INTERSECT = '┴'
    CHAR_OUTER_BOTTOM_LEFT = '└'
    CHAR_OUTER_BOTTOM_RIGHT = '┘'
    CHAR_OUTER_LEFT_INTERSECT = '├'
    CHAR_OUTER_LEFT_VERTICAL = '│'
    CHAR_OUTER_RIGHT_INTERSECT = '┤'
    CHAR_OUTER_RIGHT_VERTICAL = '│'
    CHAR_OUTER_TOP_HORIZONTAL = '─'
    CHAR_OUTER_TOP_INTERSECT = '┬'
    CHAR_OUTER_TOP_LEFT = '┌'
    CHAR_OUTER_TOP_RIGHT = '┐'


def gen_table(table):
    dimensions = max_dimensions(table.table_data, table.padding_left, table.padding_right)[:3]
    return [ ''.join(r) for r in table.gen_table(*dimensions) ]


def normalize_tables(*tables):
    widths = list(map(lambda t: t.table_width, tables))
    max_width = max(widths)
    for t, w in zip(tables, widths): t.table_data[0][-1] += ' ' * (max_width - w)


def merge_tables(table0, *tables):
    normalize_tables(table0, *tables)
    result = gen_table(table0)

    for table in tables:
        rows = gen_table(table)

        line = list(result[-1])
        line[0] = table.CHAR_OUTER_LEFT_INTERSECT
        line[-1] = table.CHAR_OUTER_RIGHT_INTERSECT

        for i, c in enumerate(rows[0]):
            if c == table.CHAR_OUTER_TOP_INTERSECT:
                if line[i] == table.CHAR_OUTER_BOTTOM_INTERSECT:
                    line[i] = table.CHAR_H_INNER_INTERSECT
                else:
                    line[i] = table.CHAR_OUTER_TOP_INTERSECT

        result[-1] = ''.join(line)
        del rows[0]
        result += rows

    return '\n'.join(result)