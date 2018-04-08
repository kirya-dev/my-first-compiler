using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyCompiler
{
    public class SyntaxParser
    {
        public LexicalParser lexicalParser;

        public char T;
        public int j;
        public int autoLabelValue = 0;
        public int cmpType;
        public int currenIdentifier;
        public int baseType;
        public int arrayType;
        public int arraySize;

        public StreamWriter code;
        public StreamWriter vars;
        string folder;

        public SyntaxParser(string filename)
        {
            //folder = Path.GetFileNameWithoutExtension(filename);
            folder = "src";

            lexicalParser = new LexicalParser(new FileStream(filename, FileMode.Open));

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            code = new StreamWriter(@"src\code.asm");
            vars = new StreamWriter(@"src\data.asm");
        }

        public void Parse()
        {
            try
            {
                Program();
            }
            catch (Exception e)
            {
                Console.WriteLine("Syntax error: " + e.Message);
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        public void Dispose()
        {
            code.Close();
            vars.Close();
        }

        public void Scan(bool flag)
        {
            Token token = flag ? lexicalParser.GetToken() : lexicalParser.SeekToken();
            T = token.a;
            j = token.n;
        }

        public bool inFollow(int x, List<int> array)
        {
            return (array.Contains(x));
        }

        public string GenerateAsmLabel()
        {
            return "label" + autoLabelValue++;
        }

        // E -> T {+T | -T}
        public void Exp()
        {
            Term();
            Scan(false);
            while (!((T == 'R') && inFollow(j, App.FollowE)))
                if (CheckDelimiter(1)) // +
                {
                    Scan(true);
                    Term();
                    code.WriteLine("    pop   ebx");
                    code.WriteLine("    pop   eax");
                    code.WriteLine("    add   eax, ebx");
                    code.WriteLine("    push  eax");
                }
                else if (CheckDelimiter(2))  // -
                {
                    Scan(true);
                    Term();
                    code.WriteLine("    pop   ebx");
                    code.WriteLine("    pop   eax");
                    code.WriteLine("    sub   eax, ebx");
                    code.WriteLine("    push  eax");
                }
                else if (!(
                    ((T == 'R') && inFollow(j, App.SINGLE))
                    || ((T == 'D') && inFollow(j, App.@double))
                    || ((T == 'K') && inFollow(j, App.literacy))
                    || (T == 'I'))
                )
                    ErrorExpected("E arithmetic");
                else
                    break;
        }

        // T -> F {*F | /F}
        public void Term()
        {
            Factor(); // F
            Scan(false);
            while (!((T == 'R') && inFollow(j, App.FollowT)))
                if (CheckDelimiter(3)) // '*'
                {
                    Scan(true);
                    Factor(); // F
                    Scan(false);
                    code.WriteLine("    pop   ebx");
                    code.WriteLine("    pop   eax");
                    code.WriteLine("    mul   ebx");
                    code.WriteLine("    push  eax");
                }
                else if (CheckDelimiter(4))  // '/'
                {
                    Scan(true);
                    Factor(); // F
                    Scan(false);
                    code.WriteLine("    pop   ebx");
                    code.WriteLine("    pop   eax");
                    code.WriteLine("    xor   edx, edx");
                    code.WriteLine("    div   ebx");
                    code.WriteLine("    push  eax");
                }
                else if (!(((T == 'R') && inFollow(j, App.SINGLE)) || ((T == 'D') && inFollow(j, App.@double)) || ((T == 'K') && inFollow(j, App.literacy)) || (T == 'I')))
                    ErrorExpected("T arithmetic");
                else
                    break;
        }

        // F -> '(' E ')' | I ('[' E ']' | eps) | C
        public void Factor()
        {
            Scan(false);
            if (T == 'C') // C
            {
                Scan(true);
                code.WriteLine("    mov   eax, {0}", lexicalParser.numbers[j]);
                code.WriteLine("    push  eax");
            } // I ('[' E ']')
            else if (T == 'I')
            {
                PushVariableToStack(ParseIdentifier());

            } // '(' E ')'
            else if (CheckDelimiter(5))
            {
                Scan(true);
                Exp();
                ScanDelimiter(6); // ')'
            }
        }

        // El -> Tl {'or' Tl}
        public void ExpLogic()
        {
            // TODO method
            TermLogic();
            Scan(false);
            while (!(((T == 'R') && inFollow(j, App.FollowL)) || ((T == 'K') && ((j == 8) || (j == 22))))) // K8 = else; K22 = then
            {
                ScanKeyword(17, false); // 'or'
                Scan(true);
                TermLogic();
                code.WriteLine("    pop   ebx");
                code.WriteLine("    pop   eax");
                code.WriteLine("    or    eax, ebx");
                code.WriteLine("    push  eax");
            }
        }
        // Tl -> Fl {'and' Fl}
        public void TermLogic()
        {
            // TODO method
            FactorLogic();
            Scan(false);
            while (!(((T == 'R') && inFollow(j, App.FollowL)) || ((T == 'K') && ((j == 8) || (j == 17) || (j == 22))))) // K8 = else; K17 = or; K22 = then
            {
                ScanKeyword(0, false); // 'and'
                Scan(true);
                FactorLogic();
                code.WriteLine("    pop   ebx");
                code.WriteLine("    pop   eax");
                code.WriteLine("    and   eax, ebx");
                code.WriteLine("    push  eax");
            }
        }

        // Fl -> 'true' | 'false' | 'not' Fl | '(' El ')' | '[' E Zn E ']' | I ('[' E ']' | eps)
        public void FactorLogic()
        {
            int idenNum;
            string trueLabel;
            string falseLabel;
            Scan(false);
            if (T == 'C')
                ErrorExpected("F logic. met number");
            // 'true' | 'false' | I ('[' E ']' | eps)
            if (((T == 'K') && ((j == 11) || (j == 23))) || (T == 'I')) // K11 = false; K23 = true
            {
                Scan(true);
                if (T == 'K') // 'true' | 'false'
                    switch (j)
                    {
                        case 11: // 'false'
                            code.WriteLine("    push  0");
                            break;
                        case 23: // 'true'
                            code.WriteLine("    push  1");
                            break;
                    }
                if (T == 'I') // I ('[' E ']' | eps)
                {
                    code.WriteLine("    mov   edi, 0");
                    idenNum = j;
                    Scan(false);
                    if (CheckDelimiter(10)) // '[' E ']'
                    {
                        Scan(true);
                        Exp();
                        code.WriteLine("    pop   edi");
                        ScanDelimiter(11); // ']'
                    }
                    code.WriteLine("    xor   ah, ah");
                    code.WriteLine("    mov   al, {0}[edi]", lexicalParser.identifiers[idenNum].name);
                    code.WriteLine("    push  eax");
                }
            }
            else
            {
                if (CheckKeyword(15)) // 'not'
                {
                    Scan(true);
                    FactorLogic();
                    code.WriteLine("    pop   eax");
                    code.WriteLine("    not   eax");
                    code.WriteLine("    push  eax");
                }
                else
                {
                    if (CheckDelimiter(10)) // '[' E Zn E ']'
                    {
                        Scan(true);
                        Exp();
                        Z();
                        Exp();

                        trueLabel = GenerateAsmLabel();
                        falseLabel = GenerateAsmLabel();

                        code.WriteLine("    pop   ebx");
                        code.WriteLine("    pop   eax");
                        code.WriteLine("    cmp   eax, ebx");

                        switch (cmpType)
                        {
                            case 0: // ==
                                code.WriteLine("    je    {0}", trueLabel);
                                break;
                            case 1: // !=
                                code.WriteLine("    jne   {0}", trueLabel);
                                break;
                            case 2: // <=
                                code.WriteLine("    jbe   {0}", trueLabel);
                                break;
                            case 3: // >=
                                code.WriteLine("    jae   {0}", trueLabel);
                                break;
                            case 7: // >
                                code.WriteLine("    ja    {0}", trueLabel);
                                break;
                            case 8: // <
                                code.WriteLine("    jb    {0}", trueLabel);
                                break;
                        }

                        code.WriteLine("    push  0");
                        code.WriteLine("    jmp   {0}", falseLabel);
                        code.WriteLine("{0}: ", trueLabel);
                        code.WriteLine("    push  1");
                        code.WriteLine("{0}: ", falseLabel);

                        ScanDelimiter(11); // ']'
                    } // '(' El ')'
                    else if (CheckDelimiter(5))
                    {
                        Scan(true);
                        ExpLogic();
                        ScanDelimiter(6); // ')'
                    }
                }
            }
        }

        // Zn -> '>' | '<' | '==' | '!=' | '>=' | '<='
        public void Z()
        {
            Scan(false);
            if (((T == 'R') && inFollow(j, App.SINGLE)) || ((T == 'D') && inFollow(j, App.@double))) // '>' | '<' | '==' | '!=' | '>=' | '<='
            {
                cmpType = j;
                Scan(true);
            }
            else
                ErrorExpected("Z logic");
        }

        // <program> -> 'program' I ';' <variables> <operators_block> '.'
        public void Program()
        {
            code.WriteLine("    .386");
            code.WriteLine("    .model flat, stdcall");
            code.WriteLine("    option casemap:none");

            code.WriteLine("    include kernel32.inc");
            code.WriteLine("    includelib kernel32.lib");

            code.WriteLine("    include masm32.inc");
            code.WriteLine("    includelib masm32.lib");

            code.WriteLine("    include data.asm");

            ScanKeyword(18); // 'program'

            Scan(true);
            if (T != 'I')
                ErrorExpected("name of program");

            ScanDelimiter(15); // ';'

            Variables(); // <variables>

            code.WriteLine("    .stack 100h");
            code.WriteLine("    .code");
            code.WriteLine("start:");

            OperatorsBlock();

            ScanDelimiter(12); // '.'

            code.WriteLine("error:");
            code.WriteLine("    push 5000d ;delay 5 seconds");
            code.WriteLine("    call Sleep");
            code.WriteLine("    invoke ExitProcess, 0");
            code.WriteLine("end   start");
        }

        // <variables> -> 'var' { <type> I ('[' C ']' | eps) { ',' I ('[' C ']' | eps) } ';' } 'end_var'
        public void Variables()
        {
            currenIdentifier = 0;
            int varListBegin = 0;
            int varListEnd = 0;

            vars.WriteLine(".data");
            vars.WriteLine("; SUPPORTING VARIABLES");
            vars.WriteLine("    @buffer   db      6");
            vars.WriteLine("    blength   db      ?");
            vars.WriteLine("    @buf      db      256 DUP (?)");
            vars.WriteLine("    clrf      db      0Dh, 0Ah, 0");
            vars.WriteLine("    output    db      6 DUP (?), 0");
            vars.WriteLine("    err_msg   db      \"Input error, try again\", 0Dh, 0Ah, 0");
            vars.WriteLine("    @true     db      \"true\"");
            vars.WriteLine("    @@true    db      \"true\"");
            vars.WriteLine("    @false    db      \"false\"");
            vars.WriteLine("    @@false   db      \"false\"");

            vars.WriteLine("; USING VARIABLES");

            ScanKeyword(24); // var

            // { <type> I ('[' C ']' | eps) { ',' I ('[' C ']' | eps) } ';' }
            Scan(false);
            while (!CheckKeyword(9)) // 'end'
            {
                Type();

                Scan(true);
                if (T != 'I') // I
                    ErrorExpected("name of new variable");
                currenIdentifier++;
                if (j != currenIdentifier)
                    ErrorExpected(".repeated declare");

                varListBegin = j;
                varListEnd = j;

                Scan(false);
                if (CheckDelimiter(10)) // ('[' C ']' | eps)
                {
                    Scan(true); // '['
                    Scan(true);
                    if (T != 'C') // 'C'
                        ErrorExpected("number of defention array in var block");
                    arraySize = lexicalParser.numbers[j];
                    ScanDelimiter(11); // ']'

                    lexicalParser.identifiers[currenIdentifier].isArray = true;
                    lexicalParser.identifiers[currenIdentifier].type = baseType;
                    lexicalParser.identifiers[currenIdentifier].size = arraySize;
                }
                else
                {
                    lexicalParser.identifiers[currenIdentifier].isArray = false;
                    lexicalParser.identifiers[currenIdentifier].type = baseType;
                }

                Scan(false);
                while (!CheckDelimiter(15)) // { ',' I ('[' C ']' | eps) }
                {
                    Scan(true); // ','
                    Scan(true);
                    if (T != 'I') // I
                        ErrorExpected("name of new variable");
                    currenIdentifier++;
                    if (j != currenIdentifier)
                        ErrorExpected(".repeated declare");

                    varListEnd = j;

                    Scan(false);
                    if (CheckDelimiter(10)) // ('[' C ']' | eps)
                    {
                        Scan(true); // '['

                        Scan(true);
                        if (T != 'C') // 'C'
                            ErrorExpected("number of defention array in var block");
                        arraySize = lexicalParser.numbers[j];
                        ScanDelimiter(11); // ']'

                        lexicalParser.identifiers[currenIdentifier].isArray = true;
                        lexicalParser.identifiers[currenIdentifier].type = baseType;
                        lexicalParser.identifiers[currenIdentifier].size = arraySize;
                    }
                    else
                    {
                        lexicalParser.identifiers[currenIdentifier].isArray = false;
                        lexicalParser.identifiers[currenIdentifier].type = baseType;
                    }
                    Scan(false);
                }

                ScanDelimiter(15); // ';'

                for (int i = varListBegin; i <= varListEnd; i++)
                {
                    Identifier id = lexicalParser.identifiers[i];
                    string type = (id.type == 13) ? "dd" : "db";
                    if (id.isArray)
                        vars.WriteLine("{0} {1} {2} DUP (?)", id, type, id.size);
                    else
                        vars.WriteLine("{0} {1} ?", id, type);
                }

                Scan(false);
            }

            ScanKeyword(9); // 'end'
        }

        // <type> -> 'bool' | 'char' | 'int'
        public void Type()
        {
            Scan(true);
            if (T != 'K')
                ErrorExpected("baseType of new variable");
            else
            {
                if ((j == 3) || (j == 6) || (j == 13)) // | 'bool' | 'char' | 'int'
                    baseType = j;
                else
                    ErrorExpected("baseType of new variable");
            }
        }
        // <OperatorsBlock> -> 'begin' { <Operator> ';' } 'end'
        public void OperatorsBlock()
        {
            ScanKeyword(2); // 'begin'

            Scan(false);
            while (!CheckKeyword(9)) // 'end'
            {
                Operators();
                ScanDelimiter(15); // ';'
                Scan(false);
            }
            Scan(true); // 'end'

        }

        // <Operators> -> <OperatorsBlock> | <if> | <let> | <read> | <readln> | <while> | <write> | <writeln> | <for> | eps
        public void Operators()
        {
            Scan(false);
            if (!CheckDelimiter(15)) // ';'
                if (T == 'K')
                    switch (j)
                    {
                        case 2: // <operatos_block>
                            OperatorsBlock();
                            break;
                        case 12: // <if>
                            Compare();
                            break;
                        case 14: // <let>
                            Assigment();
                            break;
                        case 19: // <read>
                            Read(false);
                            break;
                        case 20: // <readln>
                            Read(true);
                            break;
                        case 25: // <while>
                            CycleWhile();
                            break;
                        case 26: // <write>
                            Write(false);
                            break;
                        case 27: // <writeln>
                            Write(true);
                            break;
                        case 28: // <for>
                            ForBasic();
                            break;
                        default:
                            Assigment();
                            break;
                    }
                else
                    Assigment();
        }

        // <if> -> 'if' El 'then' <operator> ('else' <operator> | eps)
        public void Compare()
        {
            code.WriteLine("; IF()");
            Scan(true); // 'if'
            ExpLogic();
            string thenLabel = GenerateAsmLabel();
            string elseLabel = GenerateAsmLabel();
            code.WriteLine("    pop   eax");
            code.WriteLine("    cmp   eax, 0");
            code.WriteLine("    jz    {0}", thenLabel);
            ScanKeyword(22); // 'then'

            code.WriteLine("; IF THEN");
            Operators();
            code.WriteLine("    jmp   {0}", elseLabel);
            code.WriteLine("{0}: ", thenLabel);
            Scan(false);
            if (CheckKeyword(8)) // 'else' <operator> | eps
            {
                code.WriteLine("; IF ELSE ");
                Scan(true);
                Operators();
            }
            code.WriteLine("{0}: ", elseLabel);
        }
        // <let> -> ('let' | eps) I ('[' E ']' | eps) '=' (E | El | L | I ('[' E ']' | eps))
        public void Assigment()
        {
            code.WriteLine("; LET()");
            Scan(false);
            if (CheckKeyword(14)) // 'let'
                Scan(true);

            Identifier id = ParseIdentifier();

            ScanDelimiter(9); // '='

            switch (id.type)
            {
                case 3: // El
                    ExpLogic();
                    PopStackToVariable(id);
                    break;
                case 6: // L | I ('[' E ']' | eps))
                    Scan(true);
                    if (T == 'L')
                    {
                        code.WriteLine("    mov   al, \"{0}\"", lexicalParser.literals[j][0]);
                        code.WriteLine("    pop   edi");
                        code.WriteLine("    mov   {0}[edi], al", id);
                    } // I
                    else
                    {
                        ErrorExpected("bad code branch");
                        // TODO
                        //int sIdenNum = j;
                        //code.WriteLine("    mov   edi, 0");
                        //scan(false);
                        //if (CheckDelimiter(10)) // '[' E ']'
                        //{
                        //    scan(true); // '['
                        //    E();
                        //    code.WriteLine("    pop   edi");
                        //    TakeDelimiter(11); // ']'
                        //}
                        //scan(false);
                        //code.WriteLine("    mov   al, {0}[edi]", lexicalParser.identifiers[sIdenNum].name);
                        //code.WriteLine("    pop   edi");
                        //code.WriteLine("    mov   {0}[edi], al", lexicalParser.identifiers[idenNum].name);
                    }
                    Scan(false);
                    break;
                case 13:
                    Exp();
                    PopStackToVariable(id);
                    break;
            }
        }

        // <while> -> 'while' '(' El ')' <operator>
        public void CycleWhile()
        {
            code.WriteLine("; WHILE()");
            Scan(true); // 'while'
            ScanDelimiter(5); // '('

            string repeatLabel = GenerateAsmLabel();
            code.WriteLine("{0}:", repeatLabel);
            ExpLogic();
            code.WriteLine("    pop   eax");
            code.WriteLine("    cmp   eax, 0");
            string endLabel = GenerateAsmLabel();
            code.WriteLine("    jz    {0}", endLabel);
            ScanDelimiter(6); // ')'

            Operators();
            code.WriteLine("    jmp   {0}", repeatLabel);
            code.WriteLine("{0}: ", endLabel);
        }

        // <read> -> 'read' '(' ((I ('[' E ']' | eps) {',' I ('[' E ']' | eps)}) | eps) | eps ')'
        public void Read(bool ln)
        {
            code.WriteLine((ln) ? "; READLN()" : "; READ()");
            Scan(true); // 'read'
            ScanDelimiter(5); // '('

            Scan(false);
            if (CheckDelimiter(6)) // ')'
                Scan(true);
            else // ((I ('[' E ']' | eps) {',' I ('[' E ']' | eps)}) | eps)
            {
                if (T != 'I')
                    ErrorExpected("variable in read function");
                Identifier id = lexicalParser.identifiers[j];

                Scan(true);

                if (id.isArray) // ('[' E ']' | eps)
                {
                    ScanDelimiter(10); // '['
                    Exp();
                    ScanDelimiter(11); // ']'
                }

                switch (id.type)
                {
                    case 3: // bool
                        ReadBoolean(id);
                        break;
                    case 6: // char
                        ReadChar(id);
                        break;
                    case 13: // int
                        ReadInteger(id);
                        break;
                }

                Scan(false);
                while (!CheckDelimiter(6)) // {',' I ('[' E ']' | eps)}
                {
                    ScanDelimiter(13); // ','
                    Scan(false);

                    if (T != 'I')
                        ErrorExpected("read(ln) indentifier");
                    id = lexicalParser.identifiers[j];

                    Scan(true);

                    if (id.isArray) // ('[' E ']' | eps)
                    {
                        ScanDelimiter(10); // '['
                        Exp();
                        ScanDelimiter(11); // ']'
                    }
                    else // TODO: delete this branch
                        code.WriteLine("    push  0");

                    switch (id.type)
                    {
                        case 3: // bool
                            ReadBoolean(id);
                            break;
                        case 6: // char
                            ReadChar(id);
                            break;
                        case 13: // int
                            ReadInteger(id);
                            break;
                    }

                    Scan(false);
                }
            }
            Scan(true);

            if (ln)
                code.WriteLine("    invoke StdOut, addr clrf");
        }

        public void ReadInteger(Identifier id)
        {
            string loop = GenerateAsmLabel(), end_loop = GenerateAsmLabel();
            code.WriteLine("; READ INTEGER");

            //Вводим в буфер:
            code.WriteLine("    push  255");
            code.WriteLine("    push  offset @buffer");
            code.WriteLine("    call  StdIn");

            //Обнуляем переменные:
            code.WriteLine("    xor   eax, eax");
            code.WriteLine("    xor   ebx, ebx");
            code.WriteLine("    mov   ecx, 10"); //factor
            code.WriteLine("    xor   esi, esi"); //counter of digits

            //Проверяем знак:
            code.WriteLine("    mov   bl, [@buffer]");
            code.WriteLine("    sub   bl, '-'");
            code.WriteLine("    push  0"); // for sign control
            code.WriteLine("    jnz   {0}", loop);
            code.WriteLine("    push  1"); // for sign control
            code.WriteLine("    inc   esi");

            //Основной цикл распознавания числа из буфера
            code.WriteLine("{0}:", loop);
            code.WriteLine("    mov   bl, @buffer[esi]");
            code.WriteLine("    sub   bl, '0'");
            code.WriteLine("    jb    {0}", end_loop);
            code.WriteLine("    cmp   bl, 9");
            code.WriteLine("    ja    {0}", end_loop);
            code.WriteLine("    mul   ecx"); // result in edx:eax, edx (not checking - warning, overflow!)
            code.WriteLine("    add   eax, ebx");
            code.WriteLine("    inc   esi");
            code.WriteLine("    jmp   {0}", loop);

            //Инвертируем число, если был знак минус:
            string end = GenerateAsmLabel();
            code.WriteLine("{0}:", end_loop);
            code.WriteLine("    pop   ebx"); // for sign control
            code.WriteLine("    cmp   ebx, 1");
            code.WriteLine("    jnz   {0}", end);
            code.WriteLine("    neg   eax");

            //Сохраняем значение:
            code.WriteLine("{0}:", end);
            code.WriteLine("    push  eax");
            PopStackToVariable(id);
        }

        public void ReadBoolean(Identifier id)
        {
            code.WriteLine("; READ BOOLEAN");
            string start = GenerateAsmLabel();
            string l4 = GenerateAsmLabel();
            string l5 = GenerateAsmLabel();
            string le = GenerateAsmLabel();
            string lt = GenerateAsmLabel();
            string lf = GenerateAsmLabel();
            string lend = GenerateAsmLabel();
            string lerror = GenerateAsmLabel();
            code.WriteLine("{0}:", start);

            ErrorExpected(".int 21 not supported");
            code.WriteLine("    mov   ah, 0Ah");
            code.WriteLine("    lea   dx, @buffer");
            code.WriteLine("    int   21h");

            code.WriteLine("    cmp   blength, 4");
            code.WriteLine("    je    {0}", l4);
            code.WriteLine("    cmp   blength, 5");
            code.WriteLine("    je    '{0}", l5);
            code.WriteLine("    jmp   {0}", lerror);
            code.WriteLine("{0}:", l4);
            code.WriteLine("    lea   si, @true");
            code.WriteLine("    lea   edi, @buffer");
            code.WriteLine("    mov   ecx, 4");
            code.WriteLine("    repe  cmpsb");
            code.WriteLine("    jz  {0}", le);
            code.WriteLine("    jmp   {0}", lerror);
            code.WriteLine("{0}:", l5);
            code.WriteLine("    lea   si, @false");
            code.WriteLine("    lea   di, @buffer");
            code.WriteLine("    mov   ecx, 5");
            code.WriteLine("    repe  cmpsb");
            code.WriteLine("    jz  {0}", le);
            code.WriteLine("    jmp   {0}", lerror);
            code.WriteLine("{0}:", le);
            code.WriteLine("    cmp   @buffer, \"t\"");
            code.WriteLine("    je    {0}", lt);
            code.WriteLine("    push  0");
            code.WriteLine("    jmp   {0}", lend);
            code.WriteLine("{0}:", lt);
            code.WriteLine("    push  1");
            code.WriteLine("    jmp   {0}", lend);
            code.WriteLine("{0}:", lerror);

            code.WriteLine("    lea   dx, err_msg");
            code.WriteLine("    mov   ah, 9");

            code.WriteLine("    int   21h");
            code.WriteLine("    jmp   {0}", start);
            code.WriteLine("{0}:", lend);
            code.WriteLine("    pop   eax");
            code.WriteLine("    pop   edi");
            code.WriteLine("    mov   {0}[edi], al", id);
        }

        public void ReadChar(Identifier id)
        {
            code.WriteLine("; READ CHAR");
            code.WriteLine("    push  255"); //max count chars
            code.WriteLine("    push  offset @buffer"); //input buffer
            code.WriteLine("    call  StdIn");

            code.WriteLine("    xor   edx, edx");
            code.WriteLine("    mov   dl, @buffer");
            code.WriteLine("    pop   edi");
            code.WriteLine("    mov   {0}[edi], dl", id);
        }

        // <write> -> 'write' '(' (((E | L) {',' (E | L)} ) | eps) ')'
        public void Write(bool ln)
        {
            code.WriteLine((ln) ? "; WRITELN()" : "; WRITE()");
            int typeCode;
            int idenNum;
            Scan(true); // 'write'
            ScanDelimiter(5); // '('

            Scan(false);
            if (CheckDelimiter(6)) // ')'
                Scan(true);
            else // ((E | L) {',' (E | L)} )
            {
                if (T == 'L') // L
                {
                    WriteLiteral();
                    Scan(true);
                }
                else
                {
                    if (T == 'I') // E
                    {
                        idenNum = j;
                        typeCode = lexicalParser.identifiers[j].type;

                        switch (typeCode)
                        {
                            case 3: // bool
                                ExpLogic();
                                WriteBoolean();
                                break;
                            case 6: // char
                                code.WriteLine("    mov   edi, 0");
                                Scan(false);
                                if (CheckDelimiter(10)) // '['
                                {
                                    Scan(true);
                                    Exp();
                                    code.WriteLine("    pop   edi");
                                    ScanDelimiter(11); // ']'
                                }
                                WriteChar(idenNum);
                                Scan(true);
                                break;
                            case 13: // int
                                Exp();
                                WriteInteger();
                                break;
                        }
                    }
                }

                // {',' (E | L)}
                Scan(false);
                while (!CheckDelimiter(6)) // ')'
                {
                    ScanDelimiter(13); // ','

                    Scan(false);
                    if (T == 'L') // L
                    {
                        WriteLiteral();
                        Scan(true);
                    }
                    else
                    {
                        if (T == 'I') // E
                        {
                            idenNum = j;
                            typeCode = lexicalParser.identifiers[j].type;

                            switch (typeCode)
                            {
                                case 3: // bool
                                    ExpLogic();
                                    WriteBoolean();
                                    break;
                                case 6: // char
                                    Scan(true);
                                    code.WriteLine("    mov   edi, 0");
                                    Scan(false);
                                    if (CheckDelimiter(10))
                                    {
                                        Scan(true);
                                        Exp();
                                        code.WriteLine("    pop   edi");
                                        ScanDelimiter(11); // ']'
                                    }
                                    WriteChar(idenNum);
                                    Scan(true);
                                    break;
                                case 13: // int
                                    Exp();
                                    WriteInteger();
                                    break;
                            }
                        }
                    }
                    Scan(false);
                }
                Scan(true);
            }
            if (ln)
                code.WriteLine("    invoke StdOut, addr clrf");
        }

        public void WriteInteger()
        {
            code.WriteLine(";WRITE INTEGER (masm32)");
            //выводим значение с вершины стека
            string labNoMinus = GenerateAsmLabel(), nextDigit = GenerateAsmLabel();
            code.WriteLine("    pop   eax");
            // Вывод знака минуса:
            code.WriteLine("    test  eax, eax");
            code.WriteLine("    jns   {0}", labNoMinus);
            code.WriteLine("    neg   eax");
            code.WriteLine("    push  eax");  //save eax value
            code.WriteLine("    mov   @buffer, '-'");
            code.WriteLine("    mov   [@buffer+1], 0");
            code.WriteLine("    invoke StdOut, addr @buffer");
            code.WriteLine("    pop   eax"); //repaire eax value
            code.WriteLine("{0}:", labNoMinus);
            code.WriteLine("    mov   edi, 11"); // max buffer size, output from end
            code.WriteLine("    mov   bl, 10"); // base

            //цикл:
            code.WriteLine("{0}:", nextDigit);
            code.WriteLine("    cdq");  // sign extension for eax
            code.WriteLine("    div   ebx");
            code.WriteLine("    add   dl, '0'");
            code.WriteLine("    dec   edi");
            code.WriteLine("    mov   @buffer[edi], dl");
            code.WriteLine("    test   eax, eax");
            code.WriteLine("    jne    {0}", nextDigit);
            // output
            code.WriteLine("    invoke StdOut, addr @buffer[edi]");
        }

        public void WriteLiteral()
        {
            code.WriteLine("; WRITE LITERAL");
            string text = GenerateAsmLabel();
            vars.WriteLine("{0}p db \"{1}\", 0", text, lexicalParser.literals[j]);
            code.WriteLine("    invoke StdOut, addr {0}p", text);
        }

        public void WriteChar(int idenNum)
        {
            code.WriteLine("; WRITE CHAR");
            code.WriteLine("    xor eax, eax");
            code.WriteLine("    mov al, {0}[edi]", lexicalParser.identifiers[idenNum].name);
            code.WriteLine("    mov [@buffer], 0");
            code.WriteLine("    mov [@buffer+1], al");
            code.WriteLine("    invoke StdOut, addr @buffer");
        }

        public void WriteBoolean()
        {
            string l0 = GenerateAsmLabel();
            string l1 = GenerateAsmLabel();
            code.WriteLine("; WRITE BOOLEAN");
            code.WriteLine("    pop   eax");
            code.WriteLine("    cmp   eax, 0");
            code.WriteLine("    je    {0}", l0);
            code.WriteLine("    lea   edx, @@true");
            code.WriteLine("    jmp   {0}", l1);
            code.WriteLine("{0}:", l0);
            code.WriteLine("    lea   edx, @@false");
            code.WriteLine("{0}:", l1);

            ErrorExpected(".not supported int 21");
            code.WriteLine("    mov   ah, 9");
            code.WriteLine("    int   21h");
        }

        // 'for' I '=' E 'to' E ['step' E] <operator> {; <operator>} 'next' I 
        public void ForBasic()
        {
            code.WriteLine("; FORBASIC()");
            string lend = GenerateAsmLabel();
            string lcmp = GenerateAsmLabel();
            Scan(true); // 'for'

            Identifier id = ParseIdentifier();

            ScanDelimiter(9); // '='

            Exp();
            PushVariableToStack(id);

            ScanKeyword(29); // 'to'

            code.WriteLine("{0}:", lcmp);
            Exp();
            code.WriteLine("    pop   eax");
            code.WriteLine("    pop   edi");
            code.WriteLine("    mov   ebx, {0}[edi]", id);
            code.WriteLine("    push  edi");
            code.WriteLine("    cmp   ebx, eax");
            code.WriteLine("    jg    {0}", lend);
            Scan(false);
            if (!CheckKeyword(30)) // 'step'
            {
                code.WriteLine("    xor   ecx, ecx");
                code.WriteLine("    mov   ecx, 1");
            }
            else
            {
                Scan(true);
                code.WriteLine("    push  eax");
                Exp();
                code.WriteLine("    pop   ecx");
                code.WriteLine("    pop   eax");
            }
            code.WriteLine("    push  ecx");
            Scan(false);
            while (!CheckKeyword(31)) // 'next'
            {
                Operators();
                ScanDelimiter(15); // ';'
                Scan(false);
            }
            code.WriteLine("    pop   ecx");
            code.WriteLine("    pop   edi");
            code.WriteLine("    mov   eax, {0}[edi]", id);
            code.WriteLine("    add   eax, ecx");
            code.WriteLine("    mov   {0}[edi], eax", id);
            code.WriteLine("    push  edi");
            code.WriteLine("    jmp   {0}", lcmp);
            code.WriteLine("{0}:", lend);
            Scan(true); // 'next'

            ParseIdentifier();
        }

        public Identifier ParseIdentifier()
        {
            Scan(true);
            if (T != 'I')
                ErrorExpected("name of indentifier");
            Identifier id = lexicalParser.identifiers[j];

            if (id.isArray) // ('[' E ']' | eps)
            {
                ScanDelimiter(10); // '['
                Exp();
                ScanDelimiter(11); // ']'
            }

            return id;
        }

        public void PopStackToVariable(Identifier id)
        {
            //TODO: циклический сдвиг для правильного заполнения массива
            // для типа инт будет 2 слова(4 байта) - то есть сдвигаем на 2 разряда (т.е. умножаем на 4)
            // предусмотреть для других типов
            code.WriteLine("    pop   eax");

            if (id.isArray) // array
            {
                code.WriteLine("    pop   edi");
                code.WriteLine("    shl   edi, 2");
                code.WriteLine("    mov   {0}[edi], eax", id);
            }
            else
            {
                code.WriteLine("    mov   {0}, eax", id);
            }
        }

        // Метод загружает значение из перменной и оставляет его в стаке.
        public void PushVariableToStack(Identifier id)
        {
            // TODO: циклический сдвиг для правильного заполнения массива
            // для типа инт будет 2 слова(4 байта) - то есть сдвигаем на 2 разряда (т.е. умножаем на 4)
            // предусмотреть для других типов
            if (id.isArray)  // array
            {
                code.WriteLine("    pop   edi");
                code.WriteLine("    shl   edi, 2");
                code.WriteLine("    mov   eax, {0}[edi]", id);
            }
            else
            {
                code.WriteLine("    mov   eax, {0}", id);
            }
            code.WriteLine("    push  eax");
        }

        // Helper functions
        public void ErrorExpected(string message)
        {
            throw new Exception("Expected " + message);
        }

        private bool CheckKeyword(int i)
        {
            return T == 'K' && j == i;
        }

        private bool CheckDelimiter(int i)
        {
            return T == 'R' && j == i;
        }

        private void ScanKeyword(int i, bool doScan = true)
        {
            if (doScan)
                Scan(true);
            if (T != 'K' || j != i)
                ErrorExpected("keyword " + lexicalParser.keywords[i]);
        }

        private void ScanDelimiter(int expected)
        {
            Scan(true);
            if (T != 'R' || j != expected)
                ErrorExpected("delimiter " + lexicalParser.delimiters[expected]);
        }
    }
}