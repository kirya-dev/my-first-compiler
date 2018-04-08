namespace MyCompiler
{
    public class Token
    {
        public char a;
        public int n;

        public Token(char a, int n)
        {
            this.a = a;
            this.n = n;
        }

        public override string ToString()
        {
            return a + "_" + n;
        }
    }
}
