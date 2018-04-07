using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EProjectFile
{
	internal static class CodeDataParser
	{
		public class Expression
		{
		}

		private class ParamListEnd : Expression
		{
		}

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
				return (Mask ? "' " : "") + Expression + ((Comment == null) ? "" : ("' " + Comment)) + "\r\n";
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

			public string UnvalidCode;

			public StatementBlock BlockOnTrue;

			public StatementBlock BlockOnFalse;

			public string Comment;

			public bool Mask;

			public override string ToString()
			{
				return (Mask ? "' " : "") + ((UnvalidCode == null) ? $".如果 ({Condition})" : $".{UnvalidCode}") + ((Comment == null) ? "" : ("' " + Comment)) + "\r\n" + AddPrefixInEachLine(BlockOnTrue.ToString(), "    ") + ".否则\r\n" + AddPrefixInEachLine(BlockOnFalse.ToString(), "    ") + ".如果结束\r\n";
			}
		}

		public class IfStatement : Statement
		{
			public int StartOffest;

			public int EndOffest;

			public Expression Condition;

			public string UnvalidCode;

			public StatementBlock Block;

			public string Comment;

			public bool Mask;

			public override string ToString()
			{
				return (Mask ? "' " : "") + ((UnvalidCode == null) ? $".如果真 ({Condition})" : $".{UnvalidCode}") + ((Comment == null) ? "" : ("' " + Comment)) + "\r\n" + AddPrefixInEachLine(Block.ToString(), "    ") + ".如果真结束\r\n";
			}
		}

		public class LoopStatement : Statement
		{
			public int StartOffest;

			public int EndOffest;

			public StatementBlock Block;

			public string UnvalidCode;

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
				return (base.MaskOnStart ? "' " : "") + ((base.UnvalidCode == null) ? $".判断循环首 ({Condition})" : $".{base.UnvalidCode}") + ((base.CommentOnStart == null) ? "" : ("' " + base.CommentOnStart)) + "\r\n" + AddPrefixInEachLine(base.Block.ToString(), "    ") + (base.MaskOnEnd ? "' " : "") + ".判断循环尾 ()" + ((base.CommentOnEnd == null) ? "" : ("' " + base.CommentOnEnd)) + "\r\n";
			}
		}

		public class DoWhileStatement : LoopStatement
		{
			public Expression Condition;

			public override string ToString()
			{
				return (base.MaskOnStart ? "' " : "") + ".循环判断首 ()" + ((base.CommentOnStart == null) ? "" : ("' " + base.CommentOnStart)) + "\r\n" + AddPrefixInEachLine(base.Block.ToString(), "    ") + (base.MaskOnEnd ? "' " : "") + ((base.UnvalidCode == null) ? $".循环判断尾 ({Condition})" : $".{base.UnvalidCode}") + ((base.CommentOnEnd == null) ? "" : ("' " + base.CommentOnEnd)) + "\r\n";
			}
		}

		public class CounterStatement : LoopStatement
		{
			public Expression Count;

			public Expression Var;

			public override string ToString()
			{
				return (base.MaskOnStart ? "' " : "") + ((base.UnvalidCode == null) ? $".计次循环首 ({Count}, {Var})" : $".{base.UnvalidCode}") + ((base.CommentOnStart == null) ? "" : ("' " + base.CommentOnStart)) + "\r\n" + AddPrefixInEachLine(base.Block.ToString(), "    ") + (base.MaskOnEnd ? "' " : "") + ".计次循环尾 ()" + ((base.CommentOnEnd == null) ? "" : ("' " + base.CommentOnEnd)) + "\r\n";
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
				return (base.MaskOnStart ? "' " : "") + ((base.UnvalidCode == null) ? $".变量循环首 ({Start}, {End}, {Step}, {Var})" : $".{base.UnvalidCode}") + ((base.CommentOnStart == null) ? "" : ("' " + base.CommentOnStart)) + "\r\n" + AddPrefixInEachLine(base.Block.ToString(), "    ") + (base.MaskOnEnd ? "' " : "") + ".变量循环尾 ()" + ((base.CommentOnEnd == null) ? "" : ("' " + base.CommentOnEnd)) + "\r\n";
			}
		}

		public class SwitchStatement : Statement
		{
			public class CaseInfo
			{
				public Expression Condition;

				public string UnvalidCode;

				public StatementBlock Block;

				public string Comment;

				public bool Mask;
			}

			public int StartOffest;

			public int EndOffest;

			public List<CaseInfo> Case = new List<CaseInfo>();

			public StatementBlock DefaultBlock;

			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (Case.Count > 0)
				{
					stringBuilder.Append(Case[0].Mask ? "' " : "");
					stringBuilder.Append((Case[0].UnvalidCode == null) ? $".判断开始 ({Case[0].Condition})" : $".{Case[0].UnvalidCode}");
					stringBuilder.Append((Case[0].Comment == null) ? "" : ("' " + Case[0].Comment));
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
				for (int i = 1; i < Case.Count; i++)
				{
					stringBuilder.Append(Case[i].Mask ? "' " : "");
					stringBuilder.Append((Case[i].UnvalidCode == null) ? $".判断 ({Case[i].Condition})" : $".{Case[i].UnvalidCode}");
					stringBuilder.Append((Case[i].Comment == null) ? "" : ("' " + Case[i].Comment));
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
				return "#Const_" + ((LibraryId < 0) ? ("Neg" + (-LibraryId).ToString()) : LibraryId.ToString()) + $"_{ConstantId}";
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

			public Expression Target;

			private ParamListExpression _ParamList = null;

			public ParamListExpression ParamList
			{
				get
				{
					return _ParamList;
				}
				set
				{
					_ParamList = value;
				}
			}

			public CallExpression(short LibraryId, int MethodId)
			{
				this.LibraryId = LibraryId;
				this.MethodId = MethodId;
			}

			public override string ToString()
			{
				return ((Target == null) ? "" : $"{Target}.") + "Sub_" + ((LibraryId < 0) ? ("Neg" + (-LibraryId).ToString()) : LibraryId.ToString()) + $"_{MethodId}" + ((_ParamList == null) ? "()" : _ParamList.ToString());
			}
		}

		public class ParamListExpression : Expression
		{
			public List<Expression> Value = new List<Expression>();

			public override string ToString()
			{
				return "(" + string.Join(", ", Value) + ")";
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
				Value = value;
			}

			public override string ToString()
			{
				DateTime value = Value;
				//if (false)
				//{
				//	return "[]";
				//}
				if (Value.TimeOfDay.TotalSeconds == 0.0)
				{
					return "[" + Value.ToString("yyyy年MM月dd日") + "]";
				}
				return "[" + Value.ToString("yyyy年MM月dd日HH时mm分ss秒") + "]";
			}
		}

		public class StringLiteral : Expression
		{
			private string Value;

			public StringLiteral(string value)
			{
				Value = value;
			}

			public override string ToString()
			{
				return $"“{Value}”";
			}
		}

		public class NumberLiteral : Expression
		{
			private double Value;

			public NumberLiteral(double value)
			{
				Value = value;
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
				Value = value;
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
		}

		private static string AddPrefixInEachLine(string x, string c)
		{
			if (string.IsNullOrEmpty(x))
			{
				return "";
			}
			if (x.EndsWith("\r\n"))
			{
				return c + x.Substring(0, x.Length - 2).Replace("\r\n", "\r\n" + c) + "\r\n";
			}
			return c + x.Replace("\r\n", "\r\n" + c);
		}

		public static StatementBlock ParseStatementBlock(BinaryReader reader, CodeSectionInfo codeSectionInfo)
		{
			StatementBlock statementBlock = new StatementBlock();
			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				int num = (int)reader.BaseStream.Position;
				byte b = reader.ReadByte();

				switch (b)
				{
				case 80:
				case 81:
				case 82:
				case 84:
				case 110:
				case 111:
				case 113:
					reader.BaseStream.Position = num;
					return statementBlock;
				case 109:
					reader.ReadByte();
					goto default;
				default:
				{
					string text;
					string text2;
					bool flag;
					CallExpression callExpression = ParseCallExpressionWithoutType(reader, out text, out text2, out flag);
					switch (b)
					{
					case 109:
					{
						SwitchStatement switchStatement = new SwitchStatement();
						Expression condition = callExpression.ParamList.Value.ElementAtOrDefault(0);
						StatementBlock statementBlock2 = ParseStatementBlock(reader, codeSectionInfo);
						while (true)
						{
							switch (reader.ReadByte())
							{
							case 110:
								switchStatement.Case.Add(new SwitchStatement.CaseInfo
								{
									Condition = condition,
									Block = statementBlock2,
									UnvalidCode = text,
									Comment = text2,
									Mask = flag
								});
								condition = ParseCallExpressionWithoutType(reader, out text, out text2, out flag).ParamList.Value.ElementAtOrDefault(0);
								statementBlock2 = ParseStatementBlock(reader, codeSectionInfo);
								continue;
							case 111:
								switchStatement.Case.Add(new SwitchStatement.CaseInfo
								{
									Condition = condition,
									Block = statementBlock2,
									UnvalidCode = text,
									Comment = text2,
									Mask = flag
								});
								condition = null;
								statementBlock2 = ParseStatementBlock(reader, codeSectionInfo);
								continue;
							case 84:
								break;
							default:
								throw new Exception();
							}
							break;
						}
						switchStatement.EndOffest = (int)reader.BaseStream.Position;
						reader.ReadByte();
						switchStatement.DefaultBlock = statementBlock2;
						switchStatement.StartOffest = num;
						statementBlock.Statements.Add(switchStatement);
						break;
					}
					case 112:
					{
						StatementBlock block = ParseStatementBlock(reader, codeSectionInfo);
						CallExpression callExpression2 = null;
						int endOffest3 = (int)reader.BaseStream.Position;
						byte b2 = reader.ReadByte();
						if (b2 == 113)
						{
							callExpression2 = ParseCallExpressionWithoutType(reader, out string unvalidCode, out string commentOnEnd, out bool maskOnEnd);
							if (callExpression.LibraryId != 0)
							{
								throw new Exception();
							}
							LoopStatement loopStatement = null;
							switch (callExpression.MethodId)
							{
							case 3:
								loopStatement = new WhileStatement
								{
									Condition = callExpression.ParamList.Value.ElementAtOrDefault(0),
									Block = block,
									UnvalidCode = text
								};
								break;
							case 5:
								loopStatement = new DoWhileStatement
								{
									Condition = callExpression2.ParamList.Value.ElementAtOrDefault(0),
									Block = block,
									UnvalidCode = unvalidCode
								};
								break;
							case 7:
								loopStatement = new CounterStatement
								{
									Count = callExpression.ParamList.Value.ElementAtOrDefault(0),
									Var = callExpression.ParamList.Value.ElementAtOrDefault(1),
									Block = block,
									UnvalidCode = text
								};
								break;
							case 9:
								loopStatement = new ForStatement
								{
									Start = callExpression.ParamList.Value.ElementAtOrDefault(0),
									End = callExpression.ParamList.Value.ElementAtOrDefault(1),
									Step = callExpression.ParamList.Value.ElementAtOrDefault(2),
									Var = callExpression.ParamList.Value.ElementAtOrDefault(3),
									Block = block,
									UnvalidCode = text
								};
								break;
							default:
								throw new Exception();
							}
							loopStatement.StartOffest = num;
							loopStatement.EndOffest = endOffest3;
							loopStatement.CommentOnStart = text2;
							loopStatement.CommentOnEnd = commentOnEnd;
							loopStatement.MaskOnStart = flag;
							loopStatement.MaskOnEnd = maskOnEnd;
							statementBlock.Statements.Add(loopStatement);
							break;
						}
						throw new Exception();
					}
					case 108:
					{
						IfStatement ifStatement = new IfStatement
						{
							Condition = callExpression.ParamList.Value.ElementAtOrDefault(0),
							UnvalidCode = text,
							Block = ParseStatementBlock(reader, codeSectionInfo),
							Comment = text2,
							Mask = flag
						};
						if (reader.ReadByte() != 82)
						{
							throw new Exception();
						}
						int endOffest = (int)reader.BaseStream.Position;
						reader.ReadByte();
						ifStatement.StartOffest = num;
						ifStatement.EndOffest = endOffest;
						statementBlock.Statements.Add(ifStatement);
						break;
					}
					case 107:
					{
						IfElseStatement ifElseStatement = new IfElseStatement
						{
							Condition = callExpression.ParamList.Value.ElementAtOrDefault(0),
							UnvalidCode = text,
							Comment = text2,
							Mask = flag
						};
						ifElseStatement.BlockOnTrue = ParseStatementBlock(reader, codeSectionInfo);
						if (reader.ReadByte() != 80)
						{
							throw new Exception();
						}
						ifElseStatement.BlockOnFalse = ParseStatementBlock(reader, codeSectionInfo);
						if (reader.ReadByte() != 81)
						{
							throw new Exception();
						}
						int endOffest2 = (int)reader.BaseStream.Position;
						reader.ReadByte();
						ifElseStatement.StartOffest = num;
						ifElseStatement.EndOffest = endOffest2;
						statementBlock.Statements.Add(ifElseStatement);
						break;
					}
					default:
						if (text != null)
						{
							statementBlock.Statements.Add(new UnvalidStatement
							{
								UnvalidCode = text,
								Mask = flag
							});
						}
						else if (callExpression.LibraryId == -1)
						{
							statementBlock.Statements.Add(new ExpressionStatement
							{
								Expression = null,
								Comment = text2
							});
						}
						else
						{
							statementBlock.Statements.Add(new ExpressionStatement
							{
								Expression = callExpression,
								Comment = text2,
								Mask = flag
							});
						}
						break;
					}
					break;
				}
				case 83:
				case 85:
					break;
				}
			}
			return statementBlock;
		}

		private static Expression ParseExpression(BinaryReader reader, bool parseMember = true)
		{
			Expression expression = null;
			byte b = reader.ReadByte();
			switch (b)
			{
			case 1:
				expression = new ParamListEnd();
				break;
			case 22:
				expression = null;
				break;
			case 23:
				expression = new NumberLiteral(reader.ReadDouble());
				break;
			case 24:
				expression = new BoolLiteral(reader.ReadInt16() != 0);
				break;
			case 25:
				expression = new DateTimeLiteral(DateTime.FromOADate(reader.ReadDouble()));
				break;
			case 26:
				expression = new StringLiteral(reader.ReadStringWithLengthPrefix());
				break;
			case 27:
				expression = new ConstantExpression(-2, reader.ReadInt32());
				break;
			case 28:
				expression = new ConstantExpression(reader.ReadInt16(), reader.ReadInt16());
				break;
			case 29:
				if (reader.ReadByte() != 56)
				{
					throw new Exception();
				}
				expression = new VariableExpression(reader.ReadInt32());
				break;
			case 30:
				expression = new SubPtrExpression(reader.ReadInt32());
				break;
			case 33:
				expression = ParseCallExpressionWithoutType(reader);
				break;
			case 35:
				expression = new EmnuConstantExpression(reader.ReadInt32(), reader.ReadInt32());
				break;
			case 31:
			{
				ArrayLiteralExpression arrayLiteralExpression = new ArrayLiteralExpression();
				Expression item;
				while (!((item = ParseExpression(reader, true)) is ArrayLiteralEnd))
				{
					arrayLiteralExpression.Value.Add(item);
				}
				expression = arrayLiteralExpression;
				break;
			}
			case 32:
				expression = new ArrayLiteralEnd();
				break;
			case 56:
			{
				int num = reader.ReadInt32();
				if (num == 83951614)
				{
					reader.ReadByte();
					expression = ParseExpression(reader, true);
				}
				else
				{
					expression = new VariableExpression(num);
				}
				break;
			}
			case 59:
				expression = new NumberLiteral((double)reader.ReadInt32());
				break;
			default:
				throw new Exception(string.Format("Unknown Type: {0}", b.ToString("X2")));
			case 55:
				break;
			}
			//bool flag = false;
			if (parseMember && (expression is VariableExpression || expression is CallExpression))
			{
				while (reader.BaseStream.Position != reader.BaseStream.Length)
				{
					switch (reader.ReadByte())
					{
					case 57:
						break;
					case 58:
						goto IL_027a;
					default:
						reader.BaseStream.Position -= 1L;
						goto IL_02eb;
					case 55:
						goto IL_02eb;
					}
					int memberId = reader.ReadInt32();
					int structId = reader.ReadInt32();
					expression = new AccessMemberExpression(expression, structId, memberId);
					continue;
					IL_027a:
					bool parseMember2 = reader.ReadByte() == 56;
					reader.BaseStream.Position -= 1L;
					expression = new AccessArrayExpression(expression, ParseExpression(reader, parseMember2));
				}
			}
			goto IL_02eb;
			IL_02eb:
			return expression;
		}

		private static ParamListExpression ParseParamList(BinaryReader reader)
		{
			ParamListExpression paramListExpression = new ParamListExpression();
			Expression item;
			while (!((item = ParseExpression(reader, true)) is ParamListEnd))
			{
				paramListExpression.Value.Add(item);
			}
			return paramListExpression;
		}

		private static CallExpression ParseCallExpressionWithoutType(BinaryReader reader)
		{
			string text;
			string text2;
			bool flag;
			return ParseCallExpressionWithoutType(reader, out text, out text2, out flag);
		}

		private static CallExpression ParseCallExpressionWithoutType(BinaryReader reader, out string unvalidCode, out string comment, out bool mask)
		{
			int methodId = reader.ReadInt32();
			short libraryId = reader.ReadInt16();
			short num = reader.ReadInt16();
			unvalidCode = reader.ReadStringWithLengthPrefix();
			comment = reader.ReadStringWithLengthPrefix();
			mask = (num == 1 || num == 32);
			if ("".Equals(unvalidCode))
			{
				unvalidCode = null;
			}
			if ("".Equals(comment))
			{
				comment = null;
			}
			CallExpression callExpression = new CallExpression(libraryId, methodId);
			if (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				switch (reader.ReadByte())
				{
				case 54:
					callExpression.ParamList = ParseParamList(reader);
					break;
				case 56:
					reader.BaseStream.Position -= 1L;
					callExpression.Target = ParseExpression(reader, true);
					callExpression.ParamList = ParseParamList(reader);
					break;
				default:
					reader.BaseStream.Position -= 1L;
					throw new Exception();
				}
			}
			return callExpression;
		}

		public static byte[] GenerateBlockOffest(StatementBlock block)
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream()))
			{
				GenerateBlockOffest(binaryWriter, block);
				binaryWriter.Flush();
				return ((MemoryStream)binaryWriter.BaseStream).ToArray();
			}
		}

		public static void GenerateBlockOffest(BinaryWriter writer, StatementBlock block)
		{
			foreach (Statement statement in block.Statements)
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
					((SwitchStatement)statement).Case.ForEach(delegate(SwitchStatement.CaseInfo x)
					{
						GenerateBlockOffest(writer, x.Block);
					});
					GenerateBlockOffest(writer, ((SwitchStatement)statement).DefaultBlock);
				}
			}
		}
	}
}
