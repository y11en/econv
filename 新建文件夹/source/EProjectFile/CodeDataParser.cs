using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjectFile
{
    public static class CodeDataParser
    {
        private static string AddPrefixInEachLine(string x,string c)//Debug用
        {
            if(string.IsNullOrEmpty(x))
            {
                return "";
            }
            if(x.EndsWith("\r\n"))
            {
                return c + x.Substring(0, x.Length - 2).Replace("\r\n", "\r\n" + c) + "\r\n";
            }
            else
            {
                return c + x.Replace("\r\n", "\r\n" + c);
            }
        }


        public class Expression
        {

        }
        /// <summary>
        /// 解析时临时标记
        /// </summary>
        private class ParamListEnd : Expression
        {

        }
        /// <summary>
        /// 解析时临时标记
        /// </summary>
        private class ArrayLiteralEnd : Expression
        {

        }
        public class Statement : Expression
        {
        }
        public class ExpressionStatement : Statement
        {
            public Expression Expression;
            public string Comment;
            public bool Mask;
            public override string ToString()
            {
                return (Mask ? "' " : "") + Expression + (Comment == null ? "" : "' " + Comment) + "\r\n";
            }
        }
        public class UnvalidStatement : Statement
        {
            public string UnvalidCode;
            public bool Mask;
            public override string ToString()
            {
                return (Mask ? "' " : "") + $"{UnvalidCode}\r\n";
            }
        }
        public class IfElseStatement : Statement
        {
            public int StartOffest;
            public int EndOffest;
            public Expression Condition;
            public string UnvalidCode;//UnvalidCode!=null时Condition==null
            public StatementBlock BlockOnTrue;
            public StatementBlock BlockOnFalse;
            public string Comment;
            public bool Mask;

            public override string ToString()
            {
                return (Mask ? "' " : "") + (UnvalidCode ==null ? $".如果 ({Condition})" : $".{UnvalidCode}") + (Comment == null ? "" : "' " + Comment) + "\r\n"
                    + AddPrefixInEachLine(BlockOnTrue.ToString(), "    ")
                    + ".否则\r\n"
                    + AddPrefixInEachLine(BlockOnFalse.ToString(), "    ")
                    + ".如果结束\r\n";
            }
        }
        public class IfStatement : Statement
        {
            public int StartOffest;
            public int EndOffest;
            public Expression Condition;
            public string UnvalidCode;//UnvalidCode!=null时Condition==null
            public StatementBlock Block;
            public string Comment;
            public bool Mask;
            public override string ToString()
            {
                return (Mask ? "' " : "") + (UnvalidCode == null ? $".如果真 ({Condition})" : $".{UnvalidCode}") + (Comment == null ? "" : "' " + Comment) + "\r\n"
                    + AddPrefixInEachLine(Block.ToString(), "    ")
                    + ".如果真结束\r\n";
            }
        }
        public class LoopStatement : Statement
        {
            public int StartOffest;
            public int EndOffest;
            public StatementBlock Block;
            public string UnvalidCode;//UnvalidCode!=null其他循环参数为null
            public string CommentOnStart;
            public string CommentOnEnd;
            public bool MaskOnStart;
            public bool MaskOnEnd;
        }
        public class WhileStatement : LoopStatement
        {
            public Expression Condition;
            public override string ToString()
            {
                
                return (MaskOnStart ? "' " : "") + (UnvalidCode == null ? $".判断循环首 ({Condition})" : $".{UnvalidCode}") + (CommentOnStart == null ? "" : "' " + CommentOnStart) + "\r\n"
                    + AddPrefixInEachLine(Block.ToString(), "    ")
                    + (MaskOnEnd ? "' " : "") + ".判断循环尾 ()" + (CommentOnEnd == null ? "" : "' " + CommentOnEnd) + "\r\n";
            }
        }
        public class DoWhileStatement : LoopStatement
        {
            public Expression Condition;
            public override string ToString()
            {
                return (MaskOnStart ? "' " : "") + $".循环判断首 ()" + (CommentOnStart == null ? "" : "' " + CommentOnStart) + "\r\n"
                    + AddPrefixInEachLine(Block.ToString(), "    ")
                    + (MaskOnEnd ? "' " : "") + (UnvalidCode == null ? $".循环判断尾 ({Condition})" : $".{UnvalidCode}") + (CommentOnEnd == null ? "" : "' " + CommentOnEnd) + "\r\n";
            }
        }
        public class CounterStatement : LoopStatement
        {
            public Expression Count;
            public Expression Var;
            public override string ToString()
            {
                return (MaskOnStart ? "' " : "") + (UnvalidCode == null ? $".计次循环首 ({Count}, {Var})" : $".{UnvalidCode}") + (CommentOnStart == null ? "" : "' " + CommentOnStart) + "\r\n"
                    + AddPrefixInEachLine(Block.ToString(), "    ")
                    + (MaskOnEnd ? "' " : "") + $".计次循环尾 ()" + (CommentOnEnd == null ? "" : "' " + CommentOnEnd) + "\r\n";
            }
        }
        public class ForStatement : LoopStatement
        {
            public Expression Start;
            public Expression End;
            public Expression Step;
            public Expression Var;
            public override string ToString()
            {
                return (MaskOnStart ? "' " : "") + (UnvalidCode == null ? $".变量循环首 ({Start}, {End}, {Step}, {Var})" : $".{UnvalidCode}") + (CommentOnStart == null ? "" : "' " + CommentOnStart) + "\r\n"
                    + AddPrefixInEachLine(Block.ToString(), "    ")
                    + (MaskOnEnd ? "' " : "") + $".变量循环尾 ()" + (CommentOnEnd == null ? "" : "' " + CommentOnEnd) + "\r\n";
            }
        }
        public class SwitchStatement : Statement
        {
            public int StartOffest;
            public int EndOffest;
            public class CaseInfo
            {
                public Expression Condition;
                public string UnvalidCode;
                public StatementBlock Block;
                public string Comment;
                public bool Mask;
            }
            public List<CaseInfo> Case = new List<CaseInfo>();
            public StatementBlock DefaultBlock;
            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                if (Case.Count > 0)
                {
                    stringBuilder.Append(Case[0].Mask ? "' ": "");
                    stringBuilder.Append(Case[0].UnvalidCode == null ? $".判断开始 ({Case[0].Condition})" : $".{Case[0].UnvalidCode}");
                    stringBuilder.Append(Case[0].Comment == null ? "" : "' " + Case[0].Comment);
                    stringBuilder.Append("\r\n");
                }
                else
                {
                    stringBuilder.Append(".判断开始 (假)\r\n");
                }
                if (Case.Count > 0)
                {
                    stringBuilder.Append(AddPrefixInEachLine(Case[0].Block.ToString(), "    "));
                }
                for(int i=1;i<Case.Count;i++)
                {
                    stringBuilder.Append(Case[i].Mask ? "' " : "");
                    stringBuilder.Append(Case[i].UnvalidCode == null ? $".判断 ({Case[i].Condition})" : $".{Case[i].UnvalidCode}");
                    stringBuilder.Append(Case[i].Comment == null ? "" : "' " + Case[i].Comment);
                    stringBuilder.Append("\r\n");
                    stringBuilder.Append(AddPrefixInEachLine(Case[i].Block.ToString(), "    "));
                }
                stringBuilder.Append(".默认\r\n");
                stringBuilder.Append(AddPrefixInEachLine(DefaultBlock.ToString(), "    "));
                stringBuilder.Append(".判断结束\r\n");
                return stringBuilder.ToString();
            }
        }
        public class StatementBlock
        {
            public List<Statement> Statements = new List<Statement>();
            public StatementBlock()
            {

            }
            public override string ToString()
            {
                return string.Join("", Statements);
            }
        }
        public class ConstantExpression : Expression
        {
            public short LibraryId;
            public int ConstantId;
            public ConstantExpression(short LibraryId, int ConstantId)
            {
                this.LibraryId = LibraryId;
                this.ConstantId = ConstantId;
            }

            public override string ToString()
            {
                return $"#Const_" + (LibraryId < 0 ? "Neg" + (-LibraryId).ToString() : LibraryId.ToString()) + $"_{ConstantId}";
            }
        }
        public class EmnuConstantExpression : Expression
        {
            public int StructId;
            public int MemberId;
            public EmnuConstantExpression(int StructId, int MemberId)
            {
                this.StructId = StructId;
                this.MemberId = MemberId;
            }

            public override string ToString()
            {
                return $"#Struct_{StructId}.Member_{MemberId}";
            }
        }
        public class VariableExpression : Expression
        {
            public int Id;
            public VariableExpression(int Id)
            {
                this.Id = Id;
            }

            public override string ToString()
            {
                return $"Var_{Id}";
            }
        }
        public class SubPtrExpression : Expression
        {
            public int Id;
            public SubPtrExpression(int Id)
            {
                this.Id = Id;
            }

            public override string ToString()
            {
                return $"&Sub_Neg2_{Id}";
            }
        }
        public class CallExpression : Expression
        {
            public short LibraryId;
            public int MethodId;
            public Expression Target;//ThisCall
            private ParamListExpression _ParamList = null;
            public CallExpression(short LibraryId,int MethodId)
            {
                this.LibraryId = LibraryId;
                this.MethodId = MethodId;
            }

            public ParamListExpression ParamList { get => _ParamList; set => _ParamList = value; }

            public override string ToString()
            {
                return  (Target==null?"":$"{Target}.") + $"Sub_" + (LibraryId<0 ? "Neg" + (-LibraryId).ToString() : LibraryId.ToString()) + $"_{MethodId}" + (_ParamList == null ? "()" : _ParamList.ToString());
            }
        }
        public class ParamListExpression : Expression
        {
            public List<Expression> Value = new List<Expression>();
            public override string ToString()
            {
                return "(" + string.Join(", ",Value) + ")";
            }
        }
        public class ArrayLiteralExpression : Expression
        {
            public List<Expression> Value = new List<Expression>();
            public override string ToString()
            {
                return "{" + string.Join(", ", Value) + "}";
            }
        }
        public class DateTimeLiteral : Expression
        {
            private DateTime Value;

            public DateTimeLiteral(DateTime value)
            {
                this.Value = value;
            }
            public override string ToString()
            {
                if (Value == null)
                {
                    return "[]";
                }
                if(Value.TimeOfDay.TotalSeconds == 0)
                {
                    return "[" + Value.ToString("yyyy年MM月dd日") + "]";
                }
                return "[" + Value.ToString("yyyy年MM月dd日HH时mm分ss秒") + "]";
            }
        }
        public class StringLiteral : Expression
        {
            private String Value;
            public StringLiteral(String value)
            {
                this.Value = value;
            }
            public override string ToString()
            {
                return $"“{Value}”";
            }
        }
        public class NumberLiteral : Expression
        {
            private Double Value;
            public NumberLiteral(Double value)
            {
                this.Value = value;
            }
            public override string ToString()
            {
                return $"{Value}";
            }
        }
        public class BoolLiteral : Expression
        {
            private bool Value;
            public BoolLiteral(bool value)
            {
                this.Value = value;
            }
            public override string ToString()
            {
                return Value ? "真" : "假";
            }
        }
        public class AccessMemberExpression : Expression
        {
            public Expression Struct;
            public int StructId;
            public int MemberId;
            public AccessMemberExpression(Expression Struct, int StructId, int MemberId)
            {
                this.Struct = Struct;
                this.StructId = StructId;
                this.MemberId = MemberId;
            }

            public override string ToString()
            {
                return $"{Struct}.Member_{MemberId}";
            }
        }
        public class AccessArrayExpression : Expression
        {
            public Expression Array;
            public Expression Index;
            public AccessArrayExpression(Expression Array, Expression Index)
            {
                this.Array = Array;
                this.Index = Index;
            }

            public override string ToString()
            {
                return $"{Array}[{Index}]";
            }
        }//多维数组通过多个AccessArrayExpression嵌套表示
        public static StatementBlock ParseStatementBlock(BinaryReader reader, BinaryWriter lineOffestWriter)
        {
            var block = new StatementBlock();
            while (!(reader.BaseStream.Position == reader.BaseStream.Length))
            {
                var startOffest = (int)reader.BaseStream.Position;
                var type = reader.ReadByte();
                if (type == 0x67) //FIXME !!! Is this right?
				{
                    //Unknown Type 0x67
                    reader.ReadBytes(15);
					if(reader.ReadByte() != 0x36)
					{
						throw new Exception();
					}
					ParseParamList(reader);
					continue;
				}
                //Debug.WriteLine($"Offest: {startOffest}, Type: 0x{type.ToString("X2")}");
                if (lineOffestWriter != null) 
                {
                    if (true //部分数据不需要将位置写入LineOffest（一般为在IDE无显示的数据）
                        && type != 0x50 // 否则
                        && type != 0x51 // 如果结束
                        && type != 0x52 // 如果真结束
                        && type != 0x55 // 循环块结束标识：0x71前
                        && type != 0x54 // .判断结束
                        && type != 0x53 // Before .判断/.默认
                        && type != 0x6F // .默认
                        )
                    {
                        if(type == 0x6D) //.判断开始（紧接着就是 0x6E）
                        {
                            lineOffestWriter.Write(startOffest + 1);//需要写入的为0x6E的位置
                        }
                        else
                        {
                            lineOffestWriter.Write(startOffest);
                        }
                    }
                }
                switch (type)
                {
                    case 0x6E: // .判断
                    case 0x6F: // .默认
                    case 0x54: // .判断结束
                    case 0x52: // 如果真结束
                    case 0x71: // 循环结束语句：XX循环尾(参数...)
                    case 0x51: // 如果结束
                    case 0x50: // 否则
                        reader.BaseStream.Position = startOffest;
                        return block;
                    case 0x53: // Before .判断/.默认
                    case 0x55: // 循环体结束标识（0x71前）
                        continue;
                    case 0x6D: // .判断开始（紧接着就是 0x6E）
                        reader.ReadByte();
                        break;
                }

                string unvalidCode;
                string comment;
                bool mask;
                var exp = ParseCallExpressionWithoutType(reader, out unvalidCode, out comment, out mask);

                switch (type)
                {
                    case 0x6D:
                        {
                            var s = new SwitchStatement();
                            Expression condition = exp.ParamList.Value.ElementAtOrDefault(0);
                            StatementBlock switch_block = ParseStatementBlock(reader, lineOffestWriter);
                            while(true)
                            {
                                byte switch_type = reader.ReadByte();
                                switch (switch_type)
                                {
                                    case 0x6E: // .判断
                                        s.Case.Add(new SwitchStatement.CaseInfo()
                                        {
                                            Condition = condition,
                                            Block = switch_block,
                                            UnvalidCode = unvalidCode,
                                            Comment = comment,
                                            Mask = mask
                                        });
                                        condition = ParseCallExpressionWithoutType(reader, out unvalidCode, out comment, out mask).ParamList.Value.ElementAtOrDefault(0);
                                        switch_block = ParseStatementBlock(reader, lineOffestWriter);
                                        break;
                                    case 0x6F: // .默认
                                        s.Case.Add(new SwitchStatement.CaseInfo()
                                        {
                                            Condition = condition,
                                            Block = switch_block,
                                            UnvalidCode = unvalidCode,
                                            Comment = comment,
                                            Mask = mask
                                        });
                                        condition = null;
                                        switch_block = ParseStatementBlock(reader, lineOffestWriter);
                                        break;
                                    case 0x54: //.判断结束
                                        s.EndOffest = (int)reader.BaseStream.Position;
                                        reader.ReadByte();
                                        s.DefaultBlock = switch_block;
                                        goto switch_parse_finish;
                                    default:
                                        throw new Exception();
                                }
                            }
                        switch_parse_finish:
                            s.StartOffest = startOffest;
                            block.Statements.Add(s);
                        }
                        break;
                    case 0x70:
                        {
                            var loopblock = ParseStatementBlock(reader, lineOffestWriter);
                            CallExpression endexp = null;
                            var endOffest = (int)reader.BaseStream.Position;
                            string endexp_unvalidCode;
                            string endexp_comment;
                            bool endexp_mask;
                            switch (reader.ReadByte())
                            {
                                case 0x71:
                                    endexp = ParseCallExpressionWithoutType(reader, out endexp_unvalidCode, out endexp_comment, out endexp_mask);
                                    break;
                                default:
                                    throw new Exception();
                            }
                            if (exp.LibraryId != 0)
                            {
                                throw new Exception();
                            }
                            LoopStatement s = null;
                            switch (exp.MethodId)
                            {
                                case 3:
                                    s = new WhileStatement()
                                    {
                                        Condition = exp.ParamList.Value.ElementAtOrDefault(0),
                                        Block = loopblock,
                                        UnvalidCode = unvalidCode
                                    };
                                    break;
                                case 5:
                                    s = new DoWhileStatement()
                                    {
                                        Condition = endexp.ParamList.Value.ElementAtOrDefault(0),
                                        Block = loopblock,
                                        UnvalidCode = endexp_unvalidCode
                                    };
                                    break;
                                case 7:
                                    s = new CounterStatement()
                                    {
                                        Count = exp.ParamList.Value.ElementAtOrDefault(0),
                                        Var = exp.ParamList.Value.ElementAtOrDefault(1),
                                        Block = loopblock,
                                        UnvalidCode = unvalidCode
                                    };
                                    break;
                                case 9:
                                    s = new ForStatement()
                                    {
                                        Start = exp.ParamList.Value.ElementAtOrDefault(0),
                                        End = exp.ParamList.Value.ElementAtOrDefault(1),
                                        Step = exp.ParamList.Value.ElementAtOrDefault(2),
                                        Var = exp.ParamList.Value.ElementAtOrDefault(3),
                                        Block = loopblock,
                                        UnvalidCode = unvalidCode
                                    };
                                    break;
                                default:
                                    throw new Exception();
                            }
                            s.StartOffest = startOffest;
                            s.EndOffest = endOffest;

                            s.CommentOnStart = comment;
                            s.CommentOnEnd = endexp_comment;

                            s.MaskOnStart = mask;
                            s.MaskOnEnd = endexp_mask;

                            block.Statements.Add(s);
                        }
                        break;
                    case 0x6C:
                        {
                            var s = new IfStatement()
                            {
                                Condition = exp.ParamList.Value.ElementAtOrDefault(0),
                                UnvalidCode = unvalidCode,
                                Block = ParseStatementBlock(reader, lineOffestWriter),
                                Comment = comment,
                                Mask = mask
                            };
                            if (reader.ReadByte() != 0x52)
                            {
                                throw new Exception();
                            };
                            var endOffest = (int)reader.BaseStream.Position;
                            reader.ReadByte();
                            s.StartOffest = startOffest;
                            s.EndOffest = endOffest;
                            block.Statements.Add(s);
                        }
                        break;
                    case 0x6B:
                        {
                            var s = new IfElseStatement()
                            {
                                Condition = exp.ParamList.Value.ElementAtOrDefault(0),
                                UnvalidCode = unvalidCode,
                                Comment = comment,
                                Mask = mask
                            };
                            s.BlockOnTrue = ParseStatementBlock(reader, lineOffestWriter);
                            if (reader.ReadByte() != 0x50)
                            {
                                throw new Exception();
                            };
                            s.BlockOnFalse = ParseStatementBlock(reader, lineOffestWriter);
                            if (reader.ReadByte() != 0x51)
                            {
                                throw new Exception();
                            };
                            var endOffest = (int)reader.BaseStream.Position;
                            reader.ReadByte();
                            s.StartOffest = startOffest;
                            s.EndOffest = endOffest;
                            block.Statements.Add(s);
                        }
                        break;
                    default:
                        if(unvalidCode != null)
                        {
                            block.Statements.Add(new UnvalidStatement()
                            {
                                UnvalidCode = unvalidCode,
                                Mask = mask
                            });
                        }
                        else
                        {
                            if (exp.LibraryId == -1)
                            {
                                block.Statements.Add(new ExpressionStatement()
                                {
                                    Expression = null,
                                    Comment = comment
                                });
                            }
                            else
                            {
                                block.Statements.Add(new ExpressionStatement()
                                {
                                    Expression = exp,
                                    Comment = comment,
                                    Mask = mask
                                });
                            }
                        }
                        break;
                }
            }
            return block;
        }
        private static Expression ParseExpression(BinaryReader reader, bool parseMember = true)
        {
            Expression result = null;
            byte type;
            do
            {
                type = reader.ReadByte();
                switch (type)
                {
                    case 0x01:
                        result = new ParamListEnd();
                        break;
                    case 0x16://空 参数
                        result = null;
                        break;
                    case 0x17:
                        result = new NumberLiteral(reader.ReadDouble());
                        break;
                    case 0x18:
                        result = new BoolLiteral(reader.ReadInt16() != 0);
                        break;
                    case 0x19:
                        result = new DateTimeLiteral(DateTime.FromOADate(reader.ReadDouble()));
                        break;
                    case 0x1A:
                        result = new StringLiteral(reader.ReadStringWithLengthPrefix());
                        break;
                    case 0x1B:
                        result = new ConstantExpression(-2, reader.ReadInt32());
                        break;
                    case 0x1C:
                        result = new ConstantExpression(reader.ReadInt16(), reader.ReadInt16());
                        break;
                    case 0x1D:
						//0x1D 0x38 <Int32:VarId>
                        continue;
                    case 0x1E:
                        result = new SubPtrExpression(reader.ReadInt32());
                        break;
                    case 0x21:
                        result = ParseCallExpressionWithoutType(reader);
                        break;
                    case 0x23:
                        result = new EmnuConstantExpression(reader.ReadInt32(), reader.ReadInt32());
                        break;
                    case 0x37:
                        continue;
                    case 0x1F:
                        {
                            var array = new ArrayLiteralExpression();
                            Expression exp;
                            while (!((exp = ParseExpression(reader)) is ArrayLiteralEnd))
                            {
                                array.Value.Add(exp);
                            };
                            result = array;
                        }
                        break;
                    case 0x20:
                        result = new ArrayLiteralEnd();
                        break;
                    case 0x38://ThisCall Or 访问变量
                        {
                            int variable = reader.ReadInt32();
                            if (variable == 0x0500FFFE)
                            {
                                reader.ReadByte();
                                return ParseExpression(reader, true);
                            }
                            else
                            {
                                result = new VariableExpression(variable);
								parseMember = true;
                            }
                        }
                        break;
                    case 0x3B:
                        result = new NumberLiteral(reader.ReadInt32());
                        break;
                    default:
                        throw new Exception($"Unknown Type: {type.ToString("X2")}");
                }
            } while (false);
            if (parseMember 
                &&(result is VariableExpression
                || result is CallExpression
                || result is AccessMemberExpression
                || result is AccessArrayExpression)) {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    switch (reader.ReadByte())
                    {
                        case 0x39:
                            int MemberId = reader.ReadInt32();
                            int StructId = reader.ReadInt32();
                            result = new AccessMemberExpression(result, StructId, MemberId);
                            break;
                        case 0x3A:
                            result = new AccessArrayExpression(result, ParseExpression(reader, false));
                            break;
                        case 0x37:
                            goto parse_member_finish;
                        default:
                            reader.BaseStream.Position -= 1;
                            goto parse_member_finish;
                    }
                }
            }
            parse_member_finish:
            return result;
        }

        private static ParamListExpression ParseParamList(BinaryReader reader)
        {
            var param = new ParamListExpression();
            Expression exp;
            while (!((exp = ParseExpression(reader)) is ParamListEnd))
            {
                param.Value.Add(exp);
            };
            return param;
        }
        private static CallExpression ParseCallExpressionWithoutType(BinaryReader reader)
        {
            var exp = ParseCallExpressionWithoutType(reader, out string unvalidCode, out string comment, out bool mask);
            return exp;
        }

        private static CallExpression ParseCallExpressionWithoutType(BinaryReader reader, out string unvalidCode, out string comment, out bool mask)
        {
            var methodId = reader.ReadInt32();
            var libraryId = reader.ReadInt16();
            var flag = reader.ReadInt16();
            unvalidCode = reader.ReadStringWithLengthPrefix();
            comment = reader.ReadStringWithLengthPrefix();
            mask = (flag & 0x20) != 0;
            //bool expand = (flag & 0x1) != 0;
            if ("".Equals(unvalidCode))
            {
                unvalidCode = null;
            }
            if ("".Equals(comment))
            {
                comment = null;
            }
            var exp = new CallExpression(libraryId, methodId);
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                switch (reader.ReadByte())
                {
                    case 0x36:
                        exp.ParamList = ParseParamList(reader);
                        break;
                    case 0x38://ThisCall
                        reader.BaseStream.Position -= 1;
                        exp.Target = ParseExpression(reader);
                        exp.ParamList = ParseParamList(reader);
                        break;
                    default:
                        reader.BaseStream.Position -= 1;
                        throw new Exception();
                }
            }
            return exp;
        }


        public static byte[] GenerateBlockOffest(StatementBlock block)
        {
            byte[] data;
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                GenerateBlockOffest(writer, block);
                writer.Flush();
                data = ((MemoryStream)writer.BaseStream).ToArray();
            }
            return data;
        }
        public static void GenerateBlockOffest(BinaryWriter writer, StatementBlock block)
        {
            foreach (var statement in block.Statements)
            {
                if (statement is IfElseStatement)
                {
                    writer.Write((byte)1);
                    writer.Write(((IfElseStatement)statement).StartOffest);
                    writer.Write(((IfElseStatement)statement).EndOffest);
                    GenerateBlockOffest(writer, ((IfElseStatement)statement).BlockOnTrue);
                    GenerateBlockOffest(writer, ((IfElseStatement)statement).BlockOnFalse);
                }
                else if (statement is IfStatement)
                {
                    writer.Write((byte)2);
                    writer.Write(((IfStatement)statement).StartOffest);
                    writer.Write(((IfStatement)statement).EndOffest);
                    GenerateBlockOffest(writer, ((IfStatement)statement).Block);
                }
                else if (statement is LoopStatement)
                {
                    writer.Write((byte)3);
                    writer.Write(((LoopStatement)statement).StartOffest);
                    writer.Write(((LoopStatement)statement).EndOffest);
                    GenerateBlockOffest(writer, ((LoopStatement)statement).Block);
                }
                else if (statement is SwitchStatement)
                {
                    writer.Write((byte)4);
                    writer.Write(((SwitchStatement)statement).StartOffest);
                    writer.Write(((SwitchStatement)statement).EndOffest);
                    ((SwitchStatement)statement).Case.ForEach(x => GenerateBlockOffest(writer, x.Block));
                    GenerateBlockOffest(writer, ((SwitchStatement)statement).DefaultBlock);
                }
            }
        }
    }
}
