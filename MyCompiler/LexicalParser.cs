using System;
using System.Collections.Generic;
using System.IO;

namespace MyCompiler
{
    public class LexicalParser
    {
        public List<Identifier> identifiers = new List<Identifier>();
        public List<int> numbers = new List<int>();
        public List<char> delimiters = new List<char>();
        public List<string> literals = new List<string>();
        public List<string> keywords = new List<string>();
        public List<string> doubleDelimiters = new List<string>();

        public int totalIdentifiers = 0;

        public Stream stream;

        public bool eof = false;

        private List<Token> tokens;
        private int tokensPointer = 0;

        public LexicalParser(Stream stream)
        {
            this.stream = stream;
            tokens = new List<Token>();

            InitializeTokens();

            // parse all tokens
            Token lastToken = Lex();
            while (null != lastToken)
            {
                tokens.Add(lastToken);
                lastToken = Lex();
            }
        }

        public void Dispose()
        {
            stream.Close();
        }

        public Token GetToken()
        {
            if (tokens.Count == tokensPointer)
                return null;
            return tokens[tokensPointer++];
        }

        public Token SeekToken()
        {
            return tokens[tokensPointer];
        }

        public Token Lex()
        {
            int state = 0;
            int lexemeNumber;
            char x = '\0';
            string lexem = "";

            for(;;)
            {
                switch (state)
                {
                    case 0:
                        x = ReadChar();

                        // перевод строки, пробел или символ табуляции
                        if (x == '\r' || x == '\n' || x == ' ' || x == '\t')
                            break;
                        if (App.letters.Contains(x)) // char
                            state = 1;
                        if (App.digits.Contains(x)) // number
                            state = 2;
                        if (x == '"')
                            state = 3;
                        if (x == '/')
                            state = 5;
                        if (delimiters.Contains(x) && (state == 0)) // single delimiter и не " (для literal)
                            state = 4;
                        if (eof) // Конец файла
                            return null;
                        break;
                    case 1: // identifier
                        while (App.letters.Contains(x) || App.digits.Contains(x))
                        {
                            lexem += x;
                            if (eof)
                                break;
                            x = ReadChar();
                        }
                        ReturnCharPoint();

                        // search in keywords
                        int result = FindKeyword(lexem);
                        if (result != -1)
                            return new Token('K', result);

                        // return identifier
                        return new Token('I', AddIdentifierToTable(lexem));

                    case 2: // number
                        do
                        {
                            lexem += x;
                            if (eof)
                                break;
                            x = ReadChar();
                        } while (App.digits.Contains(x));
                        ReturnCharPoint();
                        return new Token('C', AddNumberToTable(Int32.Parse(lexem)));
                    case 3: // literal
                        lexem = x.ToString();
                        do
                        {
                            if (eof)
                                return null;
                            x = ReadChar();
                            lexem += x;
                        } while (x != '"');
                        lexem = lexem.Substring(1, lexem.Length - 2);
                        lexemeNumber = AddLiteralToTable(lexem);
                        return new Token('L', lexemeNumber);
                    case 4: // delimiters

                        if (x == '>' || x == '<' || x == '=' || x == '!') // double delimiter
                        {
                            if (ReadChar() == '=') // double
                            {
                                lexemeNumber = DoubleDelimiter(x + "=");
                                if (lexemeNumber != -1)
                                    return new Token('D', lexemeNumber);
                                else
                                    Error();
                            }
                            ReturnCharPoint();
                        }
                        lexemeNumber = SingleDelimiter("" + x);
                        if (lexemeNumber != -1)
                            return new Token('R', lexemeNumber);
                        else
                            Error();
                        break;

                    case 5: // comment
                        lexem = x.ToString();
                        x = ReadChar();
                        switch (x.ToString())
                        {
                            case "/":
                                do
                                {
                                    lexem += x;
                                    x = ReadChar();
                                } while (!((x == '\xA') || eof));
                                lexem = "";
                                state = 0;
                                break;
                            case "*":
                                do
                                {
                                    lexem += x;
                                    x = ReadChar();
                                } while (!((x == '\x2F') && (lexem[lexem.Length - 1] == '*')));
                                lexem += x;
                                state = 0;
                                break;
                            default:
                                ReturnCharPoint();
                                return new Token('R', 4);
                        };
                        break;
                }
            }
        }

        public int AddIdentifierToTable(string currenIdentifier)
        {
            Identifier identifier = new Identifier(currenIdentifier);
            int result = identifiers.IndexOf(identifier);
            if (result != -1)
                return result;

            // add new identifier
            identifiers.Add(identifier);
            return identifiers.Count - 1;
        }

        public int AddLiteralToTable(string currentLiteral)
        {
            // search in literals
            for (int i = 0; i < literals.Count; i++)
                if (literals[i] == currentLiteral)
                    return i;
            // add new literal
            literals.Add(currentLiteral);
            return literals.Count - 1;
        }

        public int AddNumberToTable(int number)
        {
            // search in numbers
            int k = numbers.IndexOf(number);
            if (k != -1)
                return k;
            // add new number
            numbers.Add(number);
            return numbers.Count - 1;
        }
        public int SingleDelimiter(string sDelimiter)
        {
            for (int i = 0; i < delimiters.Count; i++)
                if (delimiters[i] == sDelimiter[0])
                    return i;
            return -1;
        }

        public int DoubleDelimiter(string dDelimiter)
        {
            for (int i = 0; i < doubleDelimiters.Count; i++)
                if (doubleDelimiters[i] == dDelimiter)
                    return i;
            return -1;
        }

        public bool IndexOf(char x, List<char> array)
        {
            return array.Contains(x);
        }

        public int FindKeyword(string maybeKeyword)
        {
            return keywords.IndexOf(maybeKeyword);
        }
        public void InitializeTokens()
        {
            StreamReader stream;
            // load from file double delimiters
            stream = new StreamReader("sys\\doubleDelimiters.txt");
            while (!stream.EndOfStream)
                doubleDelimiters.Add(stream.ReadLine());

            stream.Close();
            // load from file single delimiters
            stream = new StreamReader("sys\\singleDelimiters.txt");
            while (!stream.EndOfStream)
                delimiters.Add(stream.ReadLine()[0]);

            stream.Close();
            // load from file keywords
            stream = new StreamReader("sys\\keywords.txt");
            while (!stream.EndOfStream)
                keywords.Add(stream.ReadLine());

            stream.Close();
            // init elements identifiers and numbers
            App.letters.Add('_');
            for (char i = 'a'; i <= 'z'; i++)
                App.letters.Add(i);
            for (char i = 'A'; i <= 'Z'; i++)
                App.letters.Add(i);
            for (char i = '0'; i <= '9'; i++)
                App.digits.Add(i);
            // follow E arithmetic
            App.FollowE.Add(6);
            App.FollowE.Add(7);
            App.FollowE.Add(8);
            App.FollowE.Add(9);
            App.FollowE.Add(11);
            App.FollowE.Add(13);
            App.FollowE.Add(14);
            App.FollowE.Add(15);
            App.FollowE.Add(16);
            // follow T arithmetic
            App.FollowT.Add(1);
            App.FollowT.Add(2);
            App.FollowT.Add(6);
            App.FollowT.Add(7);
            App.FollowT.Add(8);
            App.FollowT.Add(9);
            App.FollowT.Add(11);
            App.FollowT.Add(13);
            App.FollowT.Add(14);
            App.FollowT.Add(15);
            App.FollowT.Add(16);
            // follow logic
            App.FollowL.Add(6);
            App.FollowL.Add(11);
            App.FollowL.Add(13);
            App.FollowL.Add(15);
            //
            App.SINGLE.Add(7);
            App.SINGLE.Add(8);
            App.SINGLE.Add(9);
            App.SINGLE.Add(18);
            //
            App.@double.Add(0);
            App.@double.Add(1);
            App.@double.Add(2);
            App.@double.Add(3);
            //
            App.literacy.Add(2);
            App.literacy.Add(4);
            App.literacy.Add(8);
            App.literacy.Add(9);
            App.literacy.Add(10);
            App.literacy.Add(12);
            App.literacy.Add(16);
            App.literacy.Add(19);
            App.literacy.Add(20);
            App.literacy.Add(21);
            App.literacy.Add(22);
            App.literacy.Add(23);
            App.literacy.Add(25);
            App.literacy.Add(26);
            App.literacy.Add(27);
            App.literacy.Add(29);
            App.literacy.Add(30);
        }

        public void Error(string message = "Incorrect symbol")
        {
            throw new Exception(message);
        }

        public char ReadChar()
        {
            byte[] buf = new byte[1];
            eof = stream.Read(buf, 0, 1) == 0;
            return (char)buf[0];
        }

        public void ReturnCharPoint(bool flag = true)
        {
            stream.Seek(-1, SeekOrigin.Current);
        }
    }

}
