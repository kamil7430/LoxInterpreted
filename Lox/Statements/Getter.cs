namespace Lox.Statements;

public class Getter : Function
{
    public Getter(Token name, List<Stmt> body) : base(name, [], body)
    { }
}